using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Common;
using NumCalc.Shared.Enums.Integration;
using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;
using NumCalc.UI.Shared.Components.Integration;
using NumCalc.UI.Shared.Enums.Integration;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Integration : BasePage<Integration>
{
    private const string ChartContainerId = "chart--integration";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;
    [Inject] public IPdfExportService PdfExportService { get; set; } = null!;

    private IntegrationMethod _method = IntegrationMethod.Rectangle;
    private RectangleVariant _rectangleVariant = RectangleVariant.Midpoint;
    private IntegrationInput? _input;
    private IntegrationResponse? Result { get; set; }

    private void ResetResult() => Result = null;
    private string? _lastExpression;
    private double _lastLowerBound;
    private double _lastUpperBound;
    private int _lastIntervals;

    private bool IsChartVisible => Result?.ChartData is not null;

    private SolutionStep? SelectedStep => Result?.SolutionSteps?.FirstOrDefault();

    private IList<SolutionStep>? FilteredSteps => Result?.SolutionSteps;

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();
        _lastExpression = formData.FunctionExpression;
        _lastLowerBound = formData.LowerBound;
        _lastUpperBound = formData.UpperBound;
        _lastIntervals = formData.Intervals;

        var request = new IntegrationRequest
        {
            Mode = IntegrationInputMode.Function,
            FunctionExpression = formData.FunctionExpression,
            LowerBound = formData.LowerBound,
            UpperBound = formData.UpperBound,
            Intervals = formData.Intervals,
            RectangleVariant = _method is IntegrationMethod.Rectangle ? _rectangleVariant : null
        };

        Func<Task<IntegrationResponse?>> apiCall = _method switch
        {
            IntegrationMethod.Rectangle => () => CalculationApiService.IntegrateRectangleAsync(request),
            IntegrationMethod.Trapezoid => () => CalculationApiService.IntegrateTrapezoidAsync(request),
            IntegrationMethod.Simpson   => () => CalculationApiService.IntegrateSimpsonAsync(request),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is not null)
            await UpdateChart();
    }

    private async Task UpdateChart()
    {
        if (Result?.ChartData is null) return;

        var chartData = Result.ChartData
            .Where(p => p.X.HasValue && p.Y.HasValue)
            .Select(p => new double[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData.Count == 0) return;

        var useShapes = Result.ShapePoints is not null;

        var curveSeries = new ChartSeries
        {
            Name = "f(x)",
            Data = chartData,
            Color = ColorUtils.GetColor(Enums.Color.Primary),
            LineWidth = 2,
            IsVisible = true,
            FillLowerBound = useShapes ? null : _lastLowerBound,
            FillUpperBound = useShapes ? null : _lastUpperBound
        };

        var seriesList = new List<ChartSeries>();

        if (useShapes)
        {
            var shapeData = Result.ShapePoints!
                .Where(p => p.X.HasValue && p.Y.HasValue)
                .Select(p => new double[] { p.X!.Value, p.Y!.Value })
                .ToList();

            var shapeName = _method is IntegrationMethod.Rectangle
                ? $"{_rectangleVariant} rectangles"
                : "Trapezoids";

            seriesList.Add(new ChartSeries
            {
                Name = shapeName,
                Data = shapeData,
                Color = ColorUtils.GetColor(Enums.Color.Primary),
                LineWidth = 1,
                IsVisible = true,
                FillLowerBound = _lastLowerBound,
                FillUpperBound = _lastUpperBound,
                Step = _method is IntegrationMethod.Rectangle ? "left" : null
            });
        }

        seriesList.Add(curveSeries);

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis { Title = "x", PlotLines = [ChartUtils.CreateZeroLine()] },
            YAxis = new ChartAxis { Title = "f(x)", PlotLines = [ChartUtils.CreateZeroLine()] },
            Series = seriesList
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;
        await SafeExecuteAsync(async () =>
        {
            var steps = new List<StepExportItem>();
            foreach (var step in FilteredSteps ?? [])
            {
                string? imageBase64 = null;
                if (!string.IsNullOrWhiteSpace(step.LatexFormula))
                    imageBase64 = await JsRuntime.InvokeAsync<string>("PdfHelper.renderLatexToPng", step.LatexFormula);
                steps.Add(new StepExportItem
                    { Description = step.Description, ImageBase64 = imageBase64, Value = step.Value });
            }

            var chartImage = IsChartVisible
                ? await JsRuntime.InvokeAsync<string>("PdfHelper.getChartImage", ChartContainerId)
                : null;

            var inputs = new Dictionary<string, string>
            {
                ["Method"] = _method.ToString(),
                ["Lower Bound"] = _lastLowerBound.ToString("G"),
                ["Upper Bound"] = _lastUpperBound.ToString("G"),
                ["Intervals"] = _lastIntervals.ToString()
            };
            if (!string.IsNullOrWhiteSpace(_lastExpression))
                inputs["Expression"] = _lastExpression;
            if (_method is IntegrationMethod.Rectangle)
                inputs["Variant"] = _rectangleVariant.ToString();

            var resultStr = _method is IntegrationMethod.Rectangle
                ? SelectedStep?.Value ?? $"I = {Result.IntegralValue:G10}"
                : $"I = {Result.IntegralValue:G10}";

            var request = new PdfExportRequest
            {
                MethodName = $"Integration — {_method}",
                Inputs = inputs,
                Result = resultStr,
                Steps = steps,
                ChartImage = chartImage
            };

            var pdfBytes = PdfExportService.GeneratePdf(request);
            var base64 = Convert.ToBase64String(pdfBytes);
            await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile",
                $"integration-{_method}.pdf", "application/pdf", base64);
        });
    }
}

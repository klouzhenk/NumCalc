using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Common;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Components.Differentiation;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Differentiation : BasePage<Differentiation>
{
    private const string ChartContainerId = "chart--differentiation";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;
    [Inject] public IPdfExportService PdfExportService { get; set; } = null!;

    private DifferentiationMethod _method = DifferentiationMethod.FiniteDifferences;
    private FiniteDiffVariant _variant = FiniteDiffVariant.Central;
    private DifferentiationInputMode _mode = DifferentiationInputMode.Function;
    private DifferentiationInput? _input;
    private DifferentiationResponse? Result { get; set; }

    private void ResetResult() => Result = null;
    private double _queryPoint;
    private string? _lastExpression;
    private double _lastStepSize;
    private int _lastDerivativeOrder;

    private bool IsChartVisible => Result?.ChartData is not null;

    // Step indices: Forward=1, Backward=2, Central=3
    private SolutionStep? SelectedStep => Result?.SolutionSteps?
        .FirstOrDefault(s => s.StepIndex == (int)_variant + 1);

    private IList<SolutionStep>? FilteredSteps => _method is DifferentiationMethod.FiniteDifferences
        ? Result?.SolutionSteps?.Where(s => s.StepIndex == (int)_variant + 1).ToList()
        : Result?.SolutionSteps;

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();
        _queryPoint = formData.QueryPoint;
        _lastExpression = formData.FunctionExpression;
        _lastStepSize = formData.StepSize;
        _lastDerivativeOrder = formData.DerivativeOrder;

        var request = new DifferentiationRequest
        {
            Mode = formData.Mode,
            FunctionExpression = formData.FunctionExpression,
            XNodes = formData.XNodes,
            YValues = formData.YValues,
            QueryPoint = formData.QueryPoint,
            StepSize = formData.StepSize,
            DerivativeOrder = formData.DerivativeOrder
        };

        Func<Task<DifferentiationResponse?>> apiCall = _method switch
        {
            DifferentiationMethod.FiniteDifferences => () => CalculationApiService.DifferentiateFiniteDiffAsync(request),
            DifferentiationMethod.Lagrange          => () => CalculationApiService.DifferentiateLagrangeAsync(request),
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

        var xMin = chartData.Min(p => p[0]);
        var xMax = chartData.Max(p => p[0]);

        var nearest = chartData.MinBy(p => Math.Abs(p[0] - _queryPoint));
        var fAtXStar = nearest![1];

        var series = new List<ChartSeries>
        {
            new()
            {
                Name = "f(x)",
                Data = chartData,
                Color = ColorUtils.GetColor(Enums.Color.Primary),
                LineWidth = 2,
                IsVisible = true
            }
        };

        if (_lastDerivativeOrder == 1)
        {
            var slope = Result.DerivativeValue;
            series.Add(new ChartSeries
            {
                Name = "Tangent at x*",
                Data =
                [
                    [xMin, fAtXStar + slope * (xMin - _queryPoint)],
                    [xMax, fAtXStar + slope * (xMax - _queryPoint)]
                ],
                Color = ColorUtils.GetColor(Enums.Color.SuccessLight),
                LineWidth = 1,
                IsVisible = true
            });
        }

        series.Add(new ChartSeries
        {
            Name = "x*",
            Type = ChartType.Scatter,
            Data = [[_queryPoint, fAtXStar]],
            Color = ColorUtils.GetColor(Enums.Color.SuccessLight),
            IsVisible = true,
            Marker = new ChartMarker { Radius = 5, Symbol = ChartSymbolType.Circle }
        });

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis { Title = "x", PlotLines = [ChartUtils.CreateZeroLine()] },
            YAxis = new ChartAxis { Title = "f(x)", PlotLines = [ChartUtils.CreateZeroLine()] },
            Series = series
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
                steps.Add(new StepExportItem { Description = step.Description, ImageBase64 = imageBase64, Value = step.Value });
            }

            var chartImage = IsChartVisible
                ? await JsRuntime.InvokeAsync<string>("PdfHelper.getChartImage", ChartContainerId)
                : null;

            var inputs = new Dictionary<string, string>
            {
                ["Method"] = _method.ToString(),
                ["Query Point"] = _queryPoint.ToString("G"),
                ["Derivative Order"] = _lastDerivativeOrder.ToString()
            };
            if (!string.IsNullOrWhiteSpace(_lastExpression))
                inputs["Expression"] = _lastExpression;
            if (_method is DifferentiationMethod.FiniteDifferences)
            {
                inputs["Variant"] = _variant.ToString();
                inputs["Step Size"] = _lastStepSize.ToString("G");
            }

            var resultStr = _method is DifferentiationMethod.FiniteDifferences
                ? SelectedStep?.Value ?? Result.DerivativeValue.ToString("G10")
                : $"f'(x*) = {Result.DerivativeValue:G10}";

            var request = new PdfExportRequest
            {
                MethodName = $"Differentiation — {_method}",
                Inputs = inputs,
                Result = resultStr,
                Steps = steps,
                ChartImage = chartImage
            };

            var pdfBytes = PdfExportService.GeneratePdf(request);
            var base64 = Convert.ToBase64String(pdfBytes);
            await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile",
                $"differentiation-{_method}.pdf", "application/pdf", base64);
        });
    }
}

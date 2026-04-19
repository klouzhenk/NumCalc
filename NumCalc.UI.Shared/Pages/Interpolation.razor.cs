using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.Interpolation;
using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;
using NumCalc.UI.Shared.Components.Interpolation;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Interpolation;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Interpolation : BasePage<Interpolation>
{
    private const string ChartContainerId = "chart--interpolation";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;
    [Inject] public IPdfExportService PdfExportService { get; set; } = null!;

    private InterpolationInputMode _mode = InterpolationInputMode.Function;
    private InterpolationMethod _method = InterpolationMethod.Newton;
    private InterpolationInput? _input;
    private InterpolationResponse? Result { get; set; }

    private void ResetResult() => Result = null;
    private double _queryPoint;
    private string? _lastExpression;
    private string? _lastXNodesText;
    private bool IsChartVisible => Result?.ChartData is not null;

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();
        _lastExpression = formData.FunctionExpression;
        _lastXNodesText = formData.XNodes is not null ? string.Join(", ", formData.XNodes) : null;

        var request = new InterpolationRequest
        {
            Mode = formData.Mode,
            FunctionExpression = formData.FunctionExpression,
            XNodes = formData.XNodes,
            YValues = formData.YValues,
            QueryPoint = _queryPoint
        };

        Func<Task<InterpolationResponse?>> apiCall = _method switch
        {
            InterpolationMethod.Newton   => () => CalculationApiService.InterpolateNewtonAsync(request),
            InterpolationMethod.Lagrange => () => CalculationApiService.InterpolateLagrangeAsync(request),
            InterpolationMethod.Spline   => () => CalculationApiService.InterpolateSplineAsync(request),
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

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis
            {
                Title = Localizer["ArgumentX"],
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            YAxis = new ChartAxis
            {
                Title = Localizer["FunctionValue"],
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            Series =
            [
                new ChartSeries
                {
                    Name = _method.ToString(),
                    Data = chartData,
                    Color = ColorUtils.GetColor(Color.Primary),
                    LineWidth = 2,
                    IsVisible = true
                },
                new ChartSeries
                {
                    Name = "x*",
                    Type = ChartType.Scatter,
                    Data = [[_queryPoint, Result.InterpolatedValue]],
                    Color = ColorUtils.GetColor(Color.SuccessLight),
                    IsVisible = true,
                    Marker = new ChartMarker { Radius = 5, Symbol = ChartSymbolType.Circle }
                }
            ]
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;

        var steps = new List<StepExportItem>();
        foreach (var step in Result.SolutionSteps ?? [])
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
            ["Mode"] = _mode.ToString(),
            ["Query Point"] = _queryPoint.ToString("G")
        };
        if (_mode is InterpolationInputMode.Function && !string.IsNullOrWhiteSpace(_lastExpression))
            inputs["Expression"] = _lastExpression;
        if (!string.IsNullOrWhiteSpace(_lastXNodesText))
            inputs["X Nodes"] = _lastXNodesText;

        var request = new PdfExportRequest
        {
            MethodName = $"Interpolation — {_method}",
            Inputs = inputs,
            Result = $"P(x*) = {Result.InterpolatedValue:G10}",
            Steps = steps,
            ChartImage = chartImage
        };

        var pdfBytes = PdfExportService.GeneratePdf(request);
        var base64 = Convert.ToBase64String(pdfBytes);
        await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile",
            $"interpolation-{_method}.pdf", "application/pdf", base64);
    }
}

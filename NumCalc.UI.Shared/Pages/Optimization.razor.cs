using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;
using NumCalc.UI.Shared.Components.Optimization;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Optimization;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Optimization : BasePage<Optimization>
{
    private const string ChartContainerId = "chart--optimization";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;
    [Inject] public IPdfExportService PdfExportService { get; set; } = null!;

    private OptimizationMethod _method = OptimizationMethod.UniformSearch;
    private bool _maximize;
    private OptimizationInput? _input;
    private OptimizationResponse? Result { get; set; }

    private void ResetResult() => Result = null;
    private string? _lastExpression;
    private double _lastLowerBound;
    private double _lastUpperBound;
    private double _lastTolerance;
    private List<double>? _lastInitialPoint;
    private double _lastLearningRate;
    private int _lastMaxIterations;

    private bool IsChartVisible => Result?.ChartData is not null;

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();
        _lastExpression = formData.FunctionExpression;
        _lastLowerBound = formData.LowerBound;
        _lastUpperBound = formData.UpperBound;
        _lastTolerance = formData.Tolerance;
        _lastInitialPoint = formData.InitialPoint;
        _lastLearningRate = formData.LearningRate;
        _lastMaxIterations = formData.MaxIterations;

        Func<Task<OptimizationResponse?>> apiCall = _method switch
        {
            OptimizationMethod.UniformSearch => () => CalculationApiService.OptimizeUniformSearchAsync(new OptimizationRequest
            {
                FunctionExpression = formData.FunctionExpression,
                LowerBound = formData.LowerBound,
                UpperBound = formData.UpperBound,
                Points = formData.Points,
                Tolerance = formData.Tolerance,
                Maximize = _maximize
            }),
            OptimizationMethod.GoldenSection => () => CalculationApiService.OptimizeGoldenSectionAsync(new OptimizationRequest
            {
                FunctionExpression = formData.FunctionExpression,
                LowerBound = formData.LowerBound,
                UpperBound = formData.UpperBound,
                Points = formData.Points,
                Tolerance = formData.Tolerance,
                Maximize = _maximize
            }),
            OptimizationMethod.GradientDescent => () => CalculationApiService.OptimizeGradientDescentAsync(new GradientDescentRequest
            {
                FunctionExpression = formData.FunctionExpression,
                InitialPoint = formData.InitialPoint,
                LearningRate = formData.LearningRate,
                Tolerance = formData.Tolerance,
                MaxIterations = formData.MaxIterations,
                Maximize = _maximize
            }),
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
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new double[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData.Count == 0) return;

        var xStar = Result.ArgMinX ?? Result.ArgMinPoint?.FirstOrDefault();

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

        if (xStar.HasValue)
        {
            series.Add(new ChartSeries
            {
                Name = "x*",
                Type = ChartType.Scatter,
                Data = [[xStar.Value, Result.MinimumValue]],
                Color = ColorUtils.GetColor(Enums.Color.SuccessLight),
                IsVisible = true,
                Marker = new ChartMarker { Radius = 5, Symbol = ChartSymbolType.Circle }
            });
        }

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
            ["Goal"] = _maximize ? "Maximize" : "Minimize",
            ["Tolerance"] = _lastTolerance.ToString("G")
        };
        if (!string.IsNullOrWhiteSpace(_lastExpression))
            inputs["Expression"] = _lastExpression;

        if (_method is OptimizationMethod.GradientDescent)
        {
            if (_lastInitialPoint is { Count: > 0 })
                inputs["Initial Point"] = $"({string.Join(", ", _lastInitialPoint)})";
            inputs["Learning Rate"] = _lastLearningRate.ToString("G");
            inputs["Max Iterations"] = _lastMaxIterations.ToString();
        }
        else
        {
            inputs["Lower Bound"] = _lastLowerBound.ToString("G");
            inputs["Upper Bound"] = _lastUpperBound.ToString("G");
        }

        var resultStr = $"f(x*) = {Result.MinimumValue:G10}";
        if (Result.ArgMinX.HasValue)
            resultStr += $", x* = {Result.ArgMinX.Value:G10}";
        else if (Result.ArgMinPoint is { Count: > 0 })
            resultStr += $", x* = ({string.Join(", ", Result.ArgMinPoint.Select(v => v.ToString("G10")))})";

        var request = new PdfExportRequest
        {
            MethodName = $"Optimization — {_method}",
            Inputs = inputs,
            Result = resultStr,
            Steps = steps,
            ChartImage = chartImage
        };

        var pdfBytes = PdfExportService.GeneratePdf(request);
        var base64 = Convert.ToBase64String(pdfBytes);
        await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile",
            $"optimization-{_method}.pdf", "application/pdf", base64);
    }
}

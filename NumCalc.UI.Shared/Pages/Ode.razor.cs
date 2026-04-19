using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.ODE.Requests;
using NumCalc.Shared.ODE.Responses;
using NumCalc.UI.Shared.Components.ODE;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.ODE;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Ode : BasePage<Ode>
{
    private const string ChartContainerId = "chart--ode";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;
    [Inject] public IPdfExportService PdfExportService { get; set; } = null!;

    private OdeMethod _method = OdeMethod.EulerImproved;
    private OdeInput? _input;
    private OdeResponse? Result { get; set; }

    private void ResetResult() => Result = null;
    private double _initialX;
    private string? _lastExpression;
    private double _lastInitialY;
    private double _lastTargetX;
    private double _lastStepSize;
    private int _lastPicardOrder;

    private bool IsChartVisible => Result?.SolutionPoints is { Count: > 0 };

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();
        _initialX = formData.InitialX;
        _lastExpression = formData.FunctionExpression;
        _lastInitialY = formData.InitialY;
        _lastTargetX = formData.TargetX;
        _lastStepSize = formData.StepSize;
        _lastPicardOrder = formData.PicardOrder ?? 4;

        var request = new OdeRequest
        {
            FunctionExpression = formData.FunctionExpression,
            InitialX = formData.InitialX,
            InitialY = formData.InitialY,
            TargetX = formData.TargetX,
            StepSize = formData.StepSize,
            PicardOrder = formData.PicardOrder ?? 4
        };

        Func<Task<OdeResponse?>> apiCall = _method switch
        {
            OdeMethod.Euler         => () => CalculationApiService.SolveEuler(request),
            OdeMethod.EulerImproved => () => CalculationApiService.SolveEulerImproved(request),
            OdeMethod.RungeKutta2   => () => CalculationApiService.SolveRungeKutta2(request),
            OdeMethod.RungeKutta4   => () => CalculationApiService.SolveRungeKutta4(request),
            OdeMethod.Picard        => () => CalculationApiService.SolvePicard(request),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is not null)
            await UpdateChart();
    }

    private async Task UpdateChart()
    {
        if (Result?.SolutionPoints is not { Count: > 0 }) return;

        var chartData = Result.SolutionPoints
            .Where(p => p.X.HasValue && p.Y.HasValue)
            .Select(p => new double[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData.Count == 0) return;

        var xAxisPlotLines = new List<PlotLine> { ChartUtils.CreateZeroLine() };

        if (_method is OdeMethod.Picard)
        {
            xAxisPlotLines.Add(new PlotLine
            {
                Value = _initialX,
                Color = ColorUtils.GetColor(Color.SuccessLight),
                Width = 1,
                DashStyle = LineStyle.Dash
            });
        }

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis { Title = "x", PlotLines = xAxisPlotLines },
            YAxis = new ChartAxis { Title = "y(x)", PlotLines = [ChartUtils.CreateZeroLine()] },
            Series =
            [
                new ChartSeries
                {
                    Name = "y(x)",
                    Data = chartData,
                    Color = ColorUtils.GetColor(Color.Primary),
                    LineWidth = 2,
                    IsVisible = true
                }
            ]
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;
        await SafeExecuteAsync(async () =>
        {
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
                ["x₀"] = _initialX.ToString("G"),
                ["y₀"] = _lastInitialY.ToString("G"),
                ["Target x"] = _lastTargetX.ToString("G"),
                ["Step Size h"] = _lastStepSize.ToString("G")
            };
            if (!string.IsNullOrWhiteSpace(_lastExpression))
                inputs["f(x, y)"] = _lastExpression;
            if (_method is OdeMethod.Picard)
                inputs["Picard Order"] = _lastPicardOrder.ToString();

            var lastPoint = Result.SolutionPoints?.LastOrDefault();
            var resultStr = lastPoint is not null
                ? $"y({lastPoint.X?.ToString("F4") ?? "?"}) ≈ {lastPoint.Y?.ToString("G10") ?? "?"}"
                : "No solution";

            var request = new PdfExportRequest
            {
                MethodName = $"ODE — {_method}",
                Inputs = inputs,
                Result = resultStr,
                Steps = steps,
                ChartImage = chartImage
            };

            var pdfBytes = PdfExportService.GeneratePdf(request);
            var base64 = Convert.ToBase64String(pdfBytes);
            await JsRuntime.InvokeVoidAsync("PdfHelper.downloadFile",
                $"ode-{_method}.pdf", "application/pdf", base64);
        });
    }
}

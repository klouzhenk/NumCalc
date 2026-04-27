using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.ODE.Requests;
using NumCalc.Shared.ODE.Responses;
using NumCalc.UI.Shared.Components.ODE;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.ODE;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils;
using OdeMethod = NumCalc.Shared.Enums.ODE.OdeMethod;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class Ode : CalculationPage<Ode>
{
    private const string ChartContainerId = "chart--ode";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private AnalysisMode _mode = AnalysisMode.Single;
    private OdeMethod _method = OdeMethod.EulerImproved;
    private List<OdeMethod> _benchmarkMethods = [];
    private OdeInput? _input;
    private OdeResponse? Result { get; set; }
    private OdeComparisonResponse? ComparisonResult { get; set; }

    private void ResetResult()
    {
        Result = null;
        ComparisonResult = null;
    }

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
        ComparisonResult = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();

        if (_mode is AnalysisMode.Benchmark)
        {
            if (_benchmarkMethods.Count == 0)
            {
                UiService.ShowError(Localizer["SelectAtLeastOneMethod"]);
                return;
            }

            var compRequest = new OdeComparisonRequest
            {
                FunctionExpression = formData.FunctionExpression,
                InitialX = formData.InitialX,
                InitialY = formData.InitialY,
                TargetX = formData.TargetX,
                StepSize = formData.StepSize,
                Methods = _benchmarkMethods
            };

            ComparisonResult = await SafeExecuteAsync(() => CalculationApiService.GetOdeComparisonAsync(compRequest));
            return;
        }

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
        {
            var inputs = new Dictionary<string, string>
            {
                ["Method"] = _method.ToString(),
                ["f(x, y)"] = formData.FunctionExpression,
                ["x₀"] = formData.InitialX.ToString("G"),
                ["y₀"] = formData.InitialY.ToString("G"),
                ["Target x"] = formData.TargetX.ToString("G"),
                ["Step Size h"] = formData.StepSize.ToString("G")
            };
            if (_method is OdeMethod.Picard)
                inputs["Picard Order"] = (formData.PicardOrder ?? 4).ToString();

            var lastPoint = Result.SolutionPoints?.LastOrDefault();
            var resultSummary = lastPoint is not null
                ? $"y({lastPoint.X?.ToString("F4") ?? "?"}) ≈ {lastPoint.Y?.ToString("G10") ?? "?"}"
                : "No solution";

            await TrySaveHistoryAsync(new SaveCalculationRecordRequest
            {
                Type = CalculationType.Ode,
                MethodName = _method.ToString(),
                InputsJson = JsonSerializer.Serialize(inputs),
                ResultSummary = resultSummary,
                ExecutionTimeMs = Result.ExecutionTimeMs
            });

            await UpdateChart();
        }
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

    private async Task SaveInputAsync(string name)
    {
        if (_input is null) return;
        var data = await _input.GetFormData();
        await TrySaveInputAsync(name, CalculationType.Ode, JsonSerializer.Serialize(data));
    }

    private async Task LoadFromJsonAsync(string json)
    {
        if (_input is null) return;
        var data = JsonSerializer.Deserialize<OdeFormData>(json);
        if (data is null) return;
        await _input.SetFormDataAsync(data);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;

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

        await ExportPdfCoreAsync(
            methodName: $"ODE — {_method}",
            inputs: inputs,
            result: resultStr,
            steps: Result.SolutionSteps,
            chartContainerId: IsChartVisible ? ChartContainerId : null,
            fileName: $"ode-{_method}.pdf",
            type: CalculationType.Ode);
    }
}

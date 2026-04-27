using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.Optimization;
using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;
using NumCalc.UI.Shared.Components.Optimization;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Optimization;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Optimization;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages.Calculation;

public partial class Optimization : CalculationPage<Optimization>
{
    private const string ChartContainerId = "chart--optimization";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private AnalysisMode _mode = AnalysisMode.Single;
    private OptimizationMethod _method = OptimizationMethod.UniformSearch;
    private List<OptimizationComparisonMethod> _benchmarkMethods = [];
    private bool _maximize;
    private OptimizationInput? _input;
    private OptimizationResponse? Result { get; set; }
    private OptimizationComparisonResponse? ComparisonResult { get; set; }

    private void ResetResult()
    {
        Result = null;
        ComparisonResult = null;
    }
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
        ComparisonResult = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();

        if (_mode is AnalysisMode.Benchmark)
        {
            var comparisonRequest = new OptimizationComparisonRequest
            {
                FunctionExpression = formData.FunctionExpression,
                LowerBound = formData.LowerBound,
                UpperBound = formData.UpperBound,
                Points = formData.Points,
                Tolerance = formData.Tolerance,
                Maximize = _maximize,
                Methods = _benchmarkMethods
            };
            ComparisonResult = await SafeExecuteAsync(() => CalculationApiService.GetOptimizationComparisonAsync(comparisonRequest));
            return;
        }
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
        {
            var inputs = new Dictionary<string, string>
            {
                ["Method"] = _method.ToString(),
                ["Expression"] = formData.FunctionExpression,
                ["Goal"] = _maximize ? "Maximize" : "Minimize",
                ["Tolerance"] = formData.Tolerance.ToString("G")
            };
            if (_method is OptimizationMethod.GradientDescent)
            {
                inputs["Initial Point"] = $"({string.Join(", ", formData.InitialPoint)})";
                inputs["Learning Rate"] = formData.LearningRate.ToString("G");
                inputs["Max Iterations"] = formData.MaxIterations.ToString();
            }
            else
            {
                inputs["Lower Bound"] = formData.LowerBound.ToString("G");
                inputs["Upper Bound"] = formData.UpperBound.ToString("G");
            }

            var resultSummary = $"f(x*) = {Result.MinimumValue:G10}";
            if (Result.ArgMinX.HasValue)
                resultSummary += $", x* = {Result.ArgMinX.Value:G10}";

            await TrySaveHistoryAsync(new SaveCalculationRecordRequest
            {
                Type = CalculationType.Optimization,
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
        if (Result?.ChartData is null) return;

        var is3D = Result.ChartData.Any(p => p.Z.HasValue);

        if (is3D)
        {
            await UpdateChart3dAsync();
            return;
        }

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
                Color = ColorUtils.GetColor(Enums.Color.PrimaryLight),
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
                Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
                IsVisible = true,
                Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Circle }
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

    private async Task UpdateChart3dAsync()
    {
        var surfaceData = Result!.ChartData!
            .Where(p => p is { X: not null, Y: not null, Z: not null })
            .Select(p => new double[] { p.X!.Value, p.Y!.Value, p.Z!.Value })
            .ToList();

        var series = new List<ChartSeries>
        {
            new()
            {
                Name = "f(x, y)",
                Data = surfaceData,
                Color = ColorUtils.GetColor(Enums.Color.PrimaryLight),
                IsVisible = true
            }
        };

        if (Result.PathData is not null)
        {
            var pathData = Result.PathData
                .Where(p => p is { X: not null, Y: not null, Z: not null })
                .Select(p => new[] { p.X!.Value, p.Y!.Value, p.Z!.Value })
                .ToList();

            series.Add(new ChartSeries
            {
                Name = "Descent path",
                Data = pathData,
                Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
                Type = ChartType.Scatter,
                IsVisible = true,
                Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Circle }
            });
        }

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            ShowLegend = true,
            XAxis = new ChartAxis { Title = "x" },
            YAxis = new ChartAxis { Title = "y" },
            ZAxis = new ChartAxis { Title = "f(x, y)" },
            Series = series
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot3d", config);
    }

    private async Task SaveInputAsync(string name)
    {
        if (_input is null) return;
        var data = await _input.GetFormData();
        await TrySaveInputAsync(name, CalculationType.Optimization, JsonSerializer.Serialize(data));
    }

    private async Task LoadFromJsonAsync(string json)
    {
        if (_input is null) return;
        var data = JsonSerializer.Deserialize<OptimizationFormData>(json);
        if (data is null) return;
        await _input.SetFormDataAsync(data);
    }

    private async Task ExportPdfAsync()
    {
        if (Result is null) return;

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

        await ExportPdfCoreAsync(
            methodName: $"Optimization — {_method}",
            inputs: inputs,
            result: resultStr,
            steps: Result.SolutionSteps,
            chartContainerId: IsChartVisible ? ChartContainerId : null,
            fileName: $"optimization-{_method}.pdf",
            type: CalculationType.Optimization);
    }
}

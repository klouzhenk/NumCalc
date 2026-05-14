using System.Text.Json;
using NumCalc.Shared.Enums.Optimization;
using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Optimization;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Optimization;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;

namespace NumCalc.UI.Shared.Utils.Calculation;

public static class OptimizationUtils
{
    public const string ChartContainerId = "chart--optimization";
    
    public static OptimizationComparisonRequest GetComparisonRequest(
        this OptimizationFormData formData,
        List<OptimizationComparisonMethod> benchmarkMethods,
        bool maximize)
    {
        return new OptimizationComparisonRequest
        {
            FunctionExpression = formData.FunctionExpression,
            LowerBound = formData.LowerBound,
            UpperBound = formData.UpperBound,
            Points = formData.Points,
            Tolerance = formData.Tolerance,
            Maximize = maximize,
            Methods = benchmarkMethods
        };
    }
    
    public static OptimizationRequest GetOptimizationRequest(this OptimizationFormData formData, bool maximize)
    {
        return new OptimizationRequest
        {
            FunctionExpression = formData.FunctionExpression,
            LowerBound = formData.LowerBound,
            UpperBound = formData.UpperBound,
            Points = formData.Points,
            Tolerance = formData.Tolerance,
            Maximize = maximize
        };
    }

    public static GradientDescentRequest GetGradientRequest(this OptimizationFormData formData, bool maximize)
    {
        return new GradientDescentRequest
        {
            FunctionExpression = formData.FunctionExpression,
            InitialPoint = formData.InitialPoint,
            LearningRate = formData.LearningRate,
            Tolerance = formData.Tolerance,
            MaxIterations = formData.MaxIterations,
            Maximize = maximize
        };
    }

    public static SaveCalculationRecordRequest GetHistoryRecord(
        this OptimizationFormData formData,
        OptimizationResponse result,
        OptimizationMethod method,
        bool maximize)
    {
        var inputs = formData.GetMethodInputs(method, maximize);

        var resultSummary = result.GetResultSummary(formData);

        return new SaveCalculationRecordRequest
        {
            Type = CalculationType.Optimization,
            MethodName = method.ToString(),
            InputsJson = JsonSerializer.Serialize(inputs),
            ResultSummary = resultSummary,
            ExecutionTimeMs = result.ExecutionTimeMs
        };
    }
    
    public static Dictionary<string, string> GetMethodInputs(this OptimizationFormData formData, OptimizationMethod method, bool maximize)
    {
        var inputs = new Dictionary<string, string>
        {
            ["Method"] = method.ToString(),
            ["Goal"] = maximize ? "Maximize" : "Minimize",
            ["Tolerance"] = formData.Tolerance.ToString("G")
        };
        
        if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
            inputs["Expression"] = formData.FunctionExpression;
        
        if (method is OptimizationMethod.GradientDescent)
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
        
        return inputs;
    }

    public static Chart? Create2DChartConfig(        
        this OptimizationFormData formData,
        OptimizationResponse result)
    {
        var chartData = result.ChartData?
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData is not { Count: > 0 }) return null;

        var xStar = result.ArgMinX ?? result.ArgMinPoint?.FirstOrDefault();

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
                Data = [[xStar.Value, result.MinimumValue]],
                Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
                IsVisible = true,
                Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond }
            });
        }

        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            Decimals = MathUtils.DecimalsFromTolerance(formData.Tolerance),
            XAxis = new ChartAxis 
            { 
                Title = "x", 
                PlotLines =
                [
                    ChartUtils.CreateZeroLine(),
                    ChartUtils.CreateConstant(formData.LowerBound),
                    ChartUtils.CreateConstant(formData.UpperBound)
                ]
            },
            YAxis = new ChartAxis { Title = "f(x)", PlotLines = [ChartUtils.CreateZeroLine()] },
            Series = series
        };
    }
    
    public static Chart? Create3DChartConfig(        
        this OptimizationFormData formData,
        OptimizationResponse result)
    {
        var surfaceData = result.ChartData?
            .Where(p => p is { X: not null, Y: not null, Z: not null })
            .Select(p => new[] { p.X!.Value, p.Y!.Value, p.Z!.Value })
            .ToList();
        
        if (surfaceData is not { Count: > 0 }) return null;

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

        if (result.PathData is not null)
        {
            var pathData = result.PathData
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
                Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond }
            });
        }

        return new Chart
        {
            ContainerId = ChartContainerId,
            ShowLegend = true,
            Decimals = MathUtils.DecimalsFromTolerance(formData.Tolerance),
            XAxis = new ChartAxis { Title = "x" },
            YAxis = new ChartAxis { Title = "y" },
            ZAxis = new ChartAxis { Title = "f(x, y)" },
            Series = series
        };
    }

    public static string GetResultSummary(this OptimizationResponse result, OptimizationFormData formData)
    {
        var resultSummary = $"f(x*) = {result.MinimumValue.FormatResult(formData.Tolerance)}";

        if (result.ArgMinX.HasValue)
            resultSummary += $", x* = {result.ArgMinX.Value.FormatResult(formData.Tolerance)}";
        else if (result.ArgMinPoint is { Count: > 0 })
            resultSummary += $", x* = ({string.Join(", ", result.ArgMinPoint.Select(v => v.FormatResult(formData.Tolerance)))})";
        
        return resultSummary;
    }
}
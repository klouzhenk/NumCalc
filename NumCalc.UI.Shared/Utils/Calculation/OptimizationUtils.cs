using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.Optimization;
using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Optimization;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Optimization;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Utils.Calculation;

public static class OptimizationUtils
{
    public const string ChartContainerId = "chart--optimization";

    private record ExpressionValidationResult(bool Valid, string[] Variables);

    public static async Task<(bool isValid, string? errorMessage)> ValidateFormData(
        this OptimizationFormData formData,
        IJSRuntime jsRuntime)
    {
        if (!formData.LowerBound.HasValue
            || !formData.UpperBound.HasValue
            || !formData.Points.HasValue
            || !formData.Tolerance.HasValue)
            return (false, "SettingValueIsRequired");
        
        if (string.IsNullOrWhiteSpace(formData.FunctionExpression))
            return (false, "ExpressionRequired");

        var result = await jsRuntime.InvokeAsync<ExpressionValidationResult>(
            "NumCalc.validateExpression", formData.FunctionExpression);
        
        if (!result.Valid)
            return (false, "ExpressionInvalid");

        if (result.Variables.Any(v => v != "x"))
            return (false, "ExpressionOnlyX");

        return (true, null);
    }
    
    public static OptimizationComparisonRequest GetComparisonRequest(
        this OptimizationFormData formData,
        List<OptimizationComparisonMethod> benchmarkMethods,
        bool maximize)
    {
        return new OptimizationComparisonRequest
        {
            FunctionExpression = formData.FunctionExpression,
            LowerBound = formData.LowerBound ?? 0,
            UpperBound = formData.UpperBound ?? 0,
            Points = formData.Points ?? 100,
            Tolerance = formData.Tolerance ?? 1e-6,
            Maximize = maximize,
            Methods = benchmarkMethods
        };
    }
    
    public static OptimizationRequest GetOptimizationRequest(this OptimizationFormData formData, bool maximize)
    {
        return new OptimizationRequest
        {
            FunctionExpression = formData.FunctionExpression,
            LowerBound = formData.LowerBound ?? 0,
            UpperBound = formData.UpperBound ?? 0,
            Points = formData.Points ?? 100,
            Tolerance = formData.Tolerance ?? 1e-6,
            Maximize = maximize
        };
    }

    public static GradientDescentRequest GetGradientRequest(this OptimizationFormData formData, bool maximize)
    {
        return new GradientDescentRequest
        {
            FunctionExpression = formData.FunctionExpression,
            InitialPoint = formData.InitialPoint.Select(v => v ?? 0).ToList(),
            LearningRate = formData.LearningRate ?? 0.01,
            Tolerance = formData.Tolerance ?? 1e-6,
            MaxIterations = formData.MaxIterations ?? 200,
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
            ["Tolerance"] = (formData.Tolerance ?? 1e-6).ToString("G")
        };

        if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
            inputs["Expression"] = formData.FunctionExpression;

        if (method is OptimizationMethod.GradientDescent)
        {
            inputs["Initial Point"] = $"({string.Join(", ", formData.InitialPoint.Select(v => v ?? 0))})";
            inputs["Learning Rate"] = (formData.LearningRate ?? 0.01).ToString("G");
            inputs["Max Iterations"] = (formData.MaxIterations ?? 200).ToString();
        }
        else
        {
            inputs["Lower Bound"] = (formData.LowerBound ?? 0).ToString("G");
            inputs["Upper Bound"] = (formData.UpperBound ?? 0).ToString("G");
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
                    ChartUtils.CreateConstant(formData.LowerBound ?? 0),
                    ChartUtils.CreateConstant(formData.UpperBound ?? 0)
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

    public static Chart CreateBenchmarkChartConfig(
        this OptimizationFormData formData,
        OptimizationComparisonResponse comparison,
        IStringLocalizer<Localization> localizer)
    {
        var series = new List<ChartSeries>
        {
            new()
            {
                Name = "f(x)",
                Expression = formData.FunctionExpression,
                Color = ColorUtils.GetColor(Enums.Color.Primary),
                LineWidth = 2,
                IsVisible = true
            }
        };

        foreach (var result in comparison.Results)
        {
            series.Add(new ChartSeries
            {
                Name = $"x* ({localizer[result.Method.ToString()]})",
                Type = ChartType.Scatter,
                Data = result is { ArgMinX: not null, MinimumValue: not null }
                    ? [[result.ArgMinX.Value, result.MinimumValue.Value]]
                    : null,
                Color = ColorUtils.GetSeriesColor((int)result.Method),
                IsVisible = true,
                Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond },
                Opacity = 0.8
            });
        }

        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            Decimals = MathUtils.DecimalsFromTolerance(formData.Tolerance),
            XAxis = new ChartAxis
            {
                Min = formData.LowerBound,
                Max = formData.UpperBound,
                Title = "x",
                PlotLines =
                [
                    ChartUtils.CreateZeroLine(),
                    ChartUtils.CreateConstant(formData.LowerBound ?? 0),
                    ChartUtils.CreateConstant(formData.UpperBound ?? 0)
                ]
            },
            YAxis = new ChartAxis { Title = "f(x)", PlotLines = [ChartUtils.CreateZeroLine()] },
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
using System.Text.Json;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.RootFinding;
using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.RootFinding;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Utils.Calculation;

public static class RootFindingUtils
{
    public const string ChartContainerId = "chart--root-finding";
    
    private record ExpressionValidationResult(bool Valid, string[] Variables);
    
    public static async Task<(bool isValid, string? errorMessage)> ValidateFormData(
        this RootFindingFormData formData,
        AnalysisMode mode,
        List<RootFindingMethod> benchmarkMethods,
        IJSRuntime jsRuntime)
    {
        if (string.IsNullOrWhiteSpace(formData.FunctionExpression))
            return (false, "ExpressionRequired");

        var result = await jsRuntime.InvokeAsync<ExpressionValidationResult>(
            "NumCalc.validateExpression", formData.FunctionExpression);
        
        if (!result.Valid)
            return (false, "ExpressionInvalid");

        if (result.Variables.Any(v => v != "x"))
            return (false, "ExpressionOnlyX");

        var isNewton = mode is AnalysisMode.Single && formData.Method is RootFindingMethod.Newton;
        if (!isNewton && formData.StartPoint >= formData.EndPoint)
            return (false, "StartMustBeLessThanEnd");

        if (mode is AnalysisMode.Benchmark && benchmarkMethods.Count == 0)
            return (false, "SelectAtLeastOneMethod");

        return (true, null);
    }

    public static RootFindingRequest GetSingleCalculationRequest(this RootFindingFormData formData)
    {
        return new RootFindingRequest
        {
            FunctionExpression = formData.FunctionExpression ?? string.Empty,
            StartRange = formData.StartPoint,
            EndRange = formData.EndPoint,
            Error = formData.Tolerance
        };
    }
    
    public static RootFindingComparisonRequest GetComparisonRequest(this RootFindingFormData formData, List<RootFindingMethod> benchmarkMethods)
    {
        return new RootFindingComparisonRequest
        {
            FunctionExpression = formData.FunctionExpression ?? string.Empty,
            StartRange = formData.StartPoint,
            EndRange = formData.EndPoint,
            Tolerance = formData.Tolerance,
            Methods = benchmarkMethods
        };
    }
    
    public static SaveCalculationRecordRequest GetHistoryRecord(this RootFindingFormData formData, RootFindingResponse result)
    {
        return new SaveCalculationRecordRequest
        {
            Type = CalculationType.RootFinding,
            MethodName = formData.Method.ToString(),
            InputsJson = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["Expression"] = formData.FunctionExpression ?? string.Empty,
                ["Start"] = formData.StartPoint.ToString("G"),
                ["End"] = formData.EndPoint.ToString("G"),
                ["Tolerance"] = formData.Tolerance.ToString("G")
            }),
            ResultSummary = result.Root.HasValue
                ? $"Root: {result.Root.Value.FormatResult(formData.Tolerance)}"
                : "No root found",
            ExecutionTimeMs = result.ExecutionTimeMs
        };
    }
    
    public static Chart CreateChartConfig(
        this RootFindingFormData formData,
        string expression,
        IStringLocalizer<Localization> localizer)
    {
        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            Decimals = MathUtils.DecimalsFromTolerance(formData.Tolerance),
            XAxis = new ChartAxis
            {
                Min = formData.StartPoint,
                Max = formData.EndPoint,
                Title = localizer["ArgumentX"],
                PlotLines = [ ChartUtils.CreateZeroLine() ]
            },

            YAxis = new ChartAxis
            {
                Title = localizer["FunctionValue"],
                PlotLines = [ ChartUtils.CreateZeroLine() ]
            },

            Series =
            [
                new ChartSeries
                {
                    Name = "f(x)",
                    Expression = expression,
                    Color = ColorUtils.GetColor(Color.Primary),
                    LineWidth = 2,
                    IsVisible = true
                }
            ]
        };
    }

    public static void AppendSingleResult(
        this Chart config,
        RootFindingFormData formData,
        RootFindingResponse result,
        IStringLocalizer<Localization> localizer)
    {
        config.Series.Add(new ChartSeries
        {
            Name = $"{localizer["Root"]} ({formData.Method})",
            Type = ChartType.Scatter,
            Data = result.Root.HasValue ? [[result.Root.Value, 0]] : null,
            Color = ColorUtils.GetColor(Color.PrimaryDark),
            IsVisible = true,
            Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Circle }
        });
    }

    public static void AppendBenchmarkResults(
        this Chart config,
        RootFindingComparisonResponse comparison,
        IStringLocalizer<Localization> localizer)
    {
        foreach (var result in comparison.Results)
        {
            config.Series.Add(new ChartSeries
            {
                Name = $"{localizer["Root"]} ({localizer[result.Method.ToString()]})",
                Type = ChartType.Scatter,
                Data = result.Root.HasValue ? [[result.Root.Value, 0]] : null,
                Color = ColorUtils.GetSeriesColor((int)result.Method),
                IsVisible = true,
                Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Circle },
                Opacity = 0.8
            });
        }
    }

    public static Dictionary<string, string> GetPdfInputs(this RootFindingFormData formData)
    {
        var isNewton = formData.Method is RootFindingMethod.Newton;
        
        var inputs = new Dictionary<string, string>
        {
            ["Method"] = formData.Method.ToString(),
            ["Expression"] = formData.FunctionExpression ?? string.Empty,
            [isNewton ? "Initial Guess" : "Start"] = formData.StartPoint.ToString("G"),
        };
        
        if (!isNewton) 
            inputs["End"] = formData.EndPoint.ToString("G");
        inputs["Tolerance"] = formData.Tolerance.ToString("G");

        return inputs;
    }

    public static void CopyFrom(this RootFindingFormData original, RootFindingFormData copy)
    {
        original.StartPoint = copy.StartPoint;
        original.EndPoint = copy.EndPoint;
        original.Tolerance = copy.Tolerance;
        original.Method = copy.Method;
        original.FunctionExpression = copy.FunctionExpression;
    }
}
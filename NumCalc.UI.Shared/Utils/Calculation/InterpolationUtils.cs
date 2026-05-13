using System.Text.Json;
using Microsoft.Extensions.Localization;
using NumCalc.Shared.Enums.Interpolation;
using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Interpolation;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Utils.Calculation;

public static class InterpolationUtils
{
    public const string ChartContainerId = "chart--interpolation";
    
    public static InterpolationRequest GetSingleCalculationRequest(this InterpolationFormData formData)
    {
        return new InterpolationRequest
        {
            Mode = formData.Mode,
            FunctionExpression = formData.FunctionExpression,
            XNodes = formData.XNodes,
            YValues = formData.YValues,
            QueryPoint = formData.QueryPoint
        };
    }

    public static InterpolationComparisonRequest GetComparisonRequest(
        this InterpolationFormData formData,
        List<InterpolationMethod> benchmarkMethods)
    {
        return new InterpolationComparisonRequest
        {
            FunctionExpression = formData.FunctionExpression ?? string.Empty,
            XNodes = formData.XNodes,
            YValues = formData.YValues,
            QueryPoint = formData.QueryPoint,
            Methods = benchmarkMethods
        };
    }

    public static SaveCalculationRecordRequest GetHistoryRecord(
        this InterpolationFormData formData,
        InterpolationResponse result,
        InterpolationMethod method)
    {
        var inputs = formData.GetMethodInputs(method);

        return new SaveCalculationRecordRequest
        {
            Type = CalculationType.Interpolation,
            MethodName = method.ToString(),
            InputsJson = JsonSerializer.Serialize(inputs),
            ResultSummary = $"P(x*) = {result.InterpolatedValue:G10}",
            ExecutionTimeMs = result.ExecutionTimeMs
        };
    }

    public static Dictionary<string, string> GetMethodInputs(
        this InterpolationFormData formData,
        InterpolationMethod method)
    {
        var inputs = new Dictionary<string, string>
        {
            ["Method"] = method.ToString(),
            ["Mode"] = formData.Mode.ToString(),
            ["Query Point"] = formData.QueryPoint.ToString("G")
        };
        if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
            inputs["Expression"] = formData.FunctionExpression;
        if (formData.XNodes is { Count: > 0 })
            inputs["X Nodes"] = string.Join(", ", formData.XNodes);

        return inputs;
    }

    public static Chart? CreateChartConfig(
        this InterpolationFormData formData, 
        InterpolationResponse? result,
        InterpolationMethod method,
        IStringLocalizer<Localization> localizer)
    {
        if (result is null) 
            return null;
        
        var chartData = result.ChartData?
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new[] { p.X!.Value, p.Y!.Value })
            .ToList();

        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            Decimals = MathUtils.DecimalsFromTolerance(null),
            XAxis = new ChartAxis
            {
                Title = localizer["ArgumentX"],
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            YAxis = new ChartAxis
            {
                Title = localizer["FunctionValue"],
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            Series =
            [
                new ChartSeries
                {
                    Name = method.ToString(),
                    Data = chartData,
                    Color = ColorUtils.GetColor(Color.Primary),
                    LineWidth = 2,
                    IsVisible = true
                },
                new ChartSeries
                {
                    Name = "x*",
                    Type = ChartType.Scatter,
                    Data = [[formData.QueryPoint, result.InterpolatedValue]],
                    Color = ColorUtils.GetColor(Color.PrimaryDark),
                    IsVisible = true,
                    Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond }
                }
            ]
        };
    }
}
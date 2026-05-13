using System.Text.Json;
using NumCalc.Shared.Enums.Integration;
using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;
using NumCalc.UI.Shared.Enums.Integration;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Integration;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;

namespace NumCalc.UI.Shared.Utils.Calculation;

public static class IntegrationUtils
{
    public const string ChartContainerId = "chart--integration";
    
    public static IntegrationComparisonRequest GetComparisonRequest(
        this IntegrationFormData formData,
        List<IntegrationComparisonMethod> benchmarkMethods)
    {
        return new IntegrationComparisonRequest
        {
            FunctionExpression = formData.FunctionExpression ?? string.Empty,
            LowerBound = formData.LowerBound,
            UpperBound = formData.UpperBound,
            Intervals = formData.Intervals,
            Methods = benchmarkMethods
        };
    }
    
    public static IntegrationRequest GetSingleCalculationRequest(
        this IntegrationFormData formData,
        IntegrationMethod method,
        RectangleVariant variant)
    {
        return new IntegrationRequest
        {
            Mode = IntegrationInputMode.Function,
            FunctionExpression = formData.FunctionExpression,
            LowerBound = formData.LowerBound,
            UpperBound = formData.UpperBound,
            Intervals = formData.Intervals,
            RectangleVariant = method is IntegrationMethod.Rectangle ? variant : null
        };
    }

    public static SaveCalculationRecordRequest GetHistoryRecord(
        this IntegrationFormData formData,
        IntegrationResponse result,
        IntegrationMethod method,
        RectangleVariant variant)
    {
        var inputs = formData.GetMethodInputs(method, variant);

        return new SaveCalculationRecordRequest
        {
            Type = CalculationType.Integration,
            MethodName = method.ToString(),
            InputsJson = JsonSerializer.Serialize(inputs),
            ResultSummary = $"I = {result.IntegralValue:G10}",
            ExecutionTimeMs = result.ExecutionTimeMs
        };
    }
    
    public static Dictionary<string, string> GetMethodInputs(
        this IntegrationFormData formData,
        IntegrationMethod method,
        RectangleVariant variant)
    {
        var inputs = new Dictionary<string, string>
        {
            ["Method"] = method.ToString(),
            ["Lower Bound"] = formData.LowerBound.ToString("G"),
            ["Upper Bound"] = formData.UpperBound.ToString("G"),
            ["Intervals"] = formData.Intervals.ToString()
        };
        if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
            inputs["Expression"] = formData.FunctionExpression;
        if (method is IntegrationMethod.Rectangle)
            inputs["Variant"] = variant.ToString();

        return inputs;
    }

    public static Chart? CreateChartConfig(        
        this IntegrationFormData formData,
        IntegrationResponse? result,
        IntegrationMethod method,
        RectangleVariant variant)
    {
        var chartData = result?.ChartData?
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData is not { Count: > 0 }) return null;

        var useShapes = result!.ShapePoints is not null;
        var curveSeries = new ChartSeries
        {
            Name = "f(x)",
            Data = chartData,
            Color = ColorUtils.GetColor(Enums.Color.Primary),
            LineWidth = 2,
            IsVisible = true,
            FillLowerBound = useShapes ? null : formData.LowerBound,
            FillUpperBound = useShapes ? null : formData.UpperBound
        };

        var seriesList = new List<ChartSeries>();

        if (useShapes)
        {
            seriesList.AddShapes(formData, result, method, variant);
        }
        
        seriesList.Add(curveSeries);

        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            Decimals = MathUtils.DecimalsFromTolerance(null),
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
            Series = seriesList
        };
    }

    private static void AddShapes(
        this List<ChartSeries> seriesList,
        IntegrationFormData formData,
        IntegrationResponse result,
        IntegrationMethod method,
        RectangleVariant variant)
    {
        var shapeData = result.ShapePoints?
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new[] { p.X!.Value, p.Y!.Value })
            .ToList();

        var shapeName = method is IntegrationMethod.Rectangle
            ? $"{variant} rectangles"
            : "Trapezoids";

        seriesList.Add(new ChartSeries
        {
            Name = shapeName,
            Data = shapeData,
            Color = ColorUtils.GetColor(Enums.Color.Primary),
            LineWidth = 1,
            IsVisible = true,
            FillLowerBound = formData.LowerBound,
            FillUpperBound = formData.UpperBound,
            Step = method is IntegrationMethod.Rectangle ? "left" : null
        });
    }
}
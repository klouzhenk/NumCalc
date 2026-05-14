using System.Text.Json;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Differentiation;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;

namespace NumCalc.UI.Shared.Utils.Calculation;

public static class DifferentiationUtils
{
    public const string ChartContainerId = "chart--differentiation";
    
    public static DifferentiationComparisonRequest GetComparisonRequest(
        this DifferentiationFormData formData,
        List<DifferentiationComparisonMethod> benchmarkMethods)
    {
        return new DifferentiationComparisonRequest
        {
            FunctionExpression = formData.FunctionExpression ?? string.Empty,
            XNodes = formData.XNodes,
            QueryPoint = formData.QueryPoint,
            StepSize = formData.StepSize,
            DerivativeOrder = formData.DerivativeOrder,
            Methods = benchmarkMethods
        };
    }
    
    public static DifferentiationRequest GetSingleCalculationRequest(
        this DifferentiationFormData formData)
    {
        return new DifferentiationRequest
        {
            Mode = formData.Mode,
            FunctionExpression = formData.FunctionExpression,
            XNodes = formData.XNodes,
            YValues = formData.YValues,
            QueryPoint = formData.QueryPoint,
            StepSize = formData.StepSize,
            DerivativeOrder = formData.DerivativeOrder
        };
    }

    public static SaveCalculationRecordRequest GetHistoryRecord(
        this DifferentiationFormData formData,
        DifferentiationResponse result,
        DifferentiationMethod method,
        FiniteDiffVariant variant)
    {
        var (inputs, methodLabel) = formData.GetMethodInputs(method, variant);
        
        var order = formData.DerivativeOrder == 2 ? "f''" : "f'";
        return new SaveCalculationRecordRequest
        {
            Type = CalculationType.Differentiation,
            MethodName = methodLabel,
            InputsJson = JsonSerializer.Serialize(inputs),
            ResultSummary = $"{order}(x*) = {result.DerivativeValue:G10}",
            ExecutionTimeMs = result.ExecutionTimeMs
        };
    }
    
    public static (Dictionary<string, string> inputs, string methodLabel) GetMethodInputs(
        this DifferentiationFormData formData,
        DifferentiationMethod method,
        FiniteDiffVariant variant)
    {
        var methodLabel = method is DifferentiationMethod.FiniteDifferences
            ? $"Finite Differences ({variant})"
            : "Lagrange";
        
        var inputs = new Dictionary<string, string>
        {
            ["Method"] = methodLabel,
            ["Query Point"] = formData.QueryPoint.ToString("G"),
            ["Derivative Order"] = formData.DerivativeOrder.ToString()
        };
        if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
            inputs["Expression"] = formData.FunctionExpression;
        
        if (method is DifferentiationMethod.FiniteDifferences)
            inputs["Step Size"] = formData.StepSize.ToString("G");

        return (inputs, methodLabel);
    }

    public static Chart? CreateChartConfig(this DifferentiationFormData formData, DifferentiationResponse? result)
    {
        var chartData = result?.ChartData?
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData is not { Count: > 0 })
            return null;

        var series = GetChartSeries(chartData, formData, result!);

        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            Decimals = MathUtils.DecimalsFromTolerance(null),
            XAxis = new ChartAxis { Title = "x", PlotLines = [ChartUtils.CreateZeroLine()] },
            YAxis = new ChartAxis { Title = "f(x)", PlotLines = [ChartUtils.CreateZeroLine()] },
            Series = series
        };
    }

    private static List<ChartSeries> GetChartSeries(
        List<double[]> chartData,
        DifferentiationFormData formData,
        DifferentiationResponse result)
    {
        var xMin = chartData.Min(p => p[0]);
        var xMax = chartData.Max(p => p[0]);
        var nearest = chartData.MinBy(p => Math.Abs(p[0] - formData.QueryPoint));
        var fAtXStar = nearest![1];
        
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

        if (formData.DerivativeOrder == 1)
        {
            series.Add(new ChartSeries
            {
                Name = "Tangent at x*",
                Data =
                [
                    [xMin, fAtXStar + result.DerivativeValue * (xMin - formData.QueryPoint)],
                    [xMax, fAtXStar + result.DerivativeValue * (xMax - formData.QueryPoint)]
                ],
                Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
                LineWidth = 1,
                IsVisible = true
            });
        }

        series.Add(new ChartSeries
        {
            Name = "x*",
            Type = ChartType.Scatter,
            Data = [[formData.QueryPoint, fAtXStar]],
            Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
            IsVisible = true,
            Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond }
        });

        return series;
    }
}
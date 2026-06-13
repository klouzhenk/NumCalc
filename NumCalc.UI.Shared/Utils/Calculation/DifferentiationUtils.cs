using System.Text.Json;
using Microsoft.Extensions.Localization;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.Differentiation;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Utils.Calculation;

public static class DifferentiationUtils
{
    public const string ChartContainerId = "chart--differentiation";

    public static (bool isValid, string? errorMessage) ValidateFormData(this DifferentiationFormData formData)
    {
        if (formData.Mode is DifferentiationInputMode.Function
            && string.IsNullOrWhiteSpace(formData.FunctionExpression))
            return (false, "ExpressionRequired");

        if (!formData.QueryPoint.HasValue || !formData.StepSize.HasValue)
            return (false, "SettingValueIsRequired");

        return (true, null);
    }

    public static DifferentiationComparisonRequest GetComparisonRequest(
        this DifferentiationFormData formData,
        List<DifferentiationComparisonMethod> benchmarkMethods)
    {
        return new DifferentiationComparisonRequest
        {
            FunctionExpression = formData.FunctionExpression ?? string.Empty,
            XNodes = formData.XNodes,
            QueryPoint = formData.QueryPoint ?? 0,
            StepSize = formData.StepSize ?? 0.001,
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
            QueryPoint = formData.QueryPoint ?? 0,
            StepSize = formData.StepSize ?? 0.001,
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
            ["Query Point"] = (formData.QueryPoint ?? 0).ToString("G"),
            ["Derivative Order"] = formData.DerivativeOrder.ToString()
        };
        if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
            inputs["Expression"] = formData.FunctionExpression;

        if (method is DifferentiationMethod.FiniteDifferences)
            inputs["Step Size"] = (formData.StepSize ?? 0.001).ToString("G");

        return (inputs, methodLabel);
    }

    public static Chart? CreateBenchmarkChartConfig(
        this DifferentiationFormData formData,
        DifferentiationComparisonResponse comparison,
        IStringLocalizer<Localization> localizer)
    {
        var chartData = comparison.ChartData?
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData is not { Count: > 0 })
            return null;

        var queryPoint = formData.QueryPoint ?? 0;
        var xMin = chartData.Min(p => p[0]);
        var xMax = chartData.Max(p => p[0]);
        var nearest = chartData.MinBy(p => Math.Abs(p[0] - queryPoint))!;
        var fAtXStar = nearest[1];

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
            foreach (var result in comparison.Results)
            {
                if (!result.DerivativeValue.HasValue) continue;

                var slope = result.DerivativeValue.Value;
                series.Add(new ChartSeries
                {
                    Name = $"Tangent at x* ({localizer[result.Method.ToString()]})",
                    Data =
                    [
                        [xMin, fAtXStar + slope * (xMin - queryPoint)],
                        [xMax, fAtXStar + slope * (xMax - queryPoint)]
                    ],
                    Color = ColorUtils.GetSeriesColor((int)result.Method),
                    LineWidth = 1,
                    IsVisible = true,
                    Opacity = 0.8
                });
            }
        }

        series.Add(new ChartSeries
        {
            Name = "x*",
            Type = ChartType.Scatter,
            Data = [[queryPoint, fAtXStar]],
            Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
            IsVisible = true,
            Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond }
        });

        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            ShowLegend = true,
            Decimals = MathUtils.DecimalsFromTolerance(null),
            XAxis = new ChartAxis
            {
                Title = "x",
                PlotLines = [ChartUtils.CreateZeroLine(), ChartUtils.CreateConstant(queryPoint)]
            },
            YAxis = new ChartAxis { Title = "f(x)", PlotLines = [ChartUtils.CreateZeroLine()] },
            Series = series
        };
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
        var queryPoint = formData.QueryPoint ?? 0;
        var xMin = chartData.Min(p => p[0]);
        var xMax = chartData.Max(p => p[0]);
        var nearest = chartData.MinBy(p => Math.Abs(p[0] - queryPoint));
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
                    [xMin, fAtXStar + result.DerivativeValue * (xMin - queryPoint)],
                    [xMax, fAtXStar + result.DerivativeValue * (xMax - queryPoint)]
                ],
                Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
                LineWidth = 1,
                IsVisible = true
            });
        }
        // TODO: for DerivativeOrder == 2, draw the osculating parabola at x*:
        //       y = f(x*) + f'(x*)(x - x*) + 0.5 * f''(x*)(x - x*)^2
        //       Requires f'(x*) too — currently we only have f''(x*).

        series.Add(new ChartSeries
        {
            Name = "x*",
            Type = ChartType.Scatter,
            Data = [[queryPoint, fAtXStar]],
            Color = ColorUtils.GetColor(Enums.Color.PrimaryDark),
            IsVisible = true,
            Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond }
        });

        return series;
    }
}
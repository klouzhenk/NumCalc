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

    public static (bool isValid, string? errorMessage) ValidateFormData(this InterpolationFormData formData)
    {
        if (formData.Mode is InterpolationInputMode.Function
            && string.IsNullOrWhiteSpace(formData.FunctionExpression))
            return (false, "ExpressionRequired");

        if (!formData.QueryPoint.HasValue)
            return (false, "SettingValueIsRequired");

        if (formData.XNodes is not { Count: >= 2 })
            return (false, "SettingValueIsRequired");

        return (true, null);
    }

    public static InterpolationRequest GetSingleCalculationRequest(this InterpolationFormData formData)
    {
        return new InterpolationRequest
        {
            Mode = formData.Mode,
            FunctionExpression = formData.FunctionExpression,
            XNodes = formData.XNodes,
            YValues = formData.YValues,
            QueryPoint = formData.QueryPoint ?? 0
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
            QueryPoint = formData.QueryPoint ?? 0,
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
            ["Query Point"] = (formData.QueryPoint ?? 0).ToString("G")
        };
        if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
            inputs["Expression"] = formData.FunctionExpression;
        if (formData.XNodes is { Count: > 0 })
            inputs["X Nodes"] = string.Join(", ", formData.XNodes);

        return inputs;
    }

    public static Chart CreateBenchmarkChartConfig(
        this InterpolationFormData formData,
        InterpolationComparisonResponse comparison,
        IStringLocalizer<Localization> localizer)
    {
        var series = new List<ChartSeries>();

        if (formData.Mode is InterpolationInputMode.Function
            && !string.IsNullOrWhiteSpace(formData.FunctionExpression))
        {
            series.Add(new ChartSeries
            {
                Name = "f(x)",
                Expression = formData.FunctionExpression,
                Color = ColorUtils.GetColor(Color.Primary),
                LineWidth = 2,
                IsVisible = true
            });
        }
        else if (formData.YValues is { Count: > 0 })
        {
            series.Add(new ChartSeries
            {
                Name = localizer["XNodes"],
                Type = ChartType.Scatter,
                Data = formData.XNodes.Zip(formData.YValues, (x, y) => new[] { x, y }).ToList(),
                Color = ColorUtils.GetColor(Color.Primary),
                IsVisible = true,
                Marker = new ChartMarker { Radius = 5 }
            });
        }

        foreach (var result in comparison.Results)
        {
            series.Add(new ChartSeries
            {
                Name = $"x* ({localizer[result.Method.ToString()]})",
                Type = ChartType.Scatter,
                Data = result.InterpolatedValue.HasValue
                    ? [[formData.QueryPoint ?? 0, result.InterpolatedValue.Value]]
                    : null,
                Color = ColorUtils.GetSeriesColor((int)result.Method),
                IsVisible = true,
                Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond },
                Opacity = 0.8
            });
        }

        double? xMin = null, xMax = null;
        if (formData.XNodes is { Count: > 0 })
        {
            var queryPoint = formData.QueryPoint ?? 0;
            xMin = Math.Min(formData.XNodes.Min(), queryPoint);
            xMax = Math.Max(formData.XNodes.Max(), queryPoint);
        }

        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            ShowLegend = true,
            Decimals = MathUtils.DecimalsFromTolerance(null),
            XAxis = new ChartAxis
            {
                Min = xMin,
                Max = xMax,
                Title = localizer["ArgumentX"],
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            YAxis = new ChartAxis
            {
                Title = localizer["FunctionValue"],
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            Series = series
        };
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
                    Data = [[formData.QueryPoint ?? 0, result.InterpolatedValue]],
                    Color = ColorUtils.GetColor(Color.PrimaryDark),
                    IsVisible = true,
                    Marker = new ChartMarker { Radius = 8, Symbol = ChartSymbolType.Diamond }
                }
            ]
        };
    }
}
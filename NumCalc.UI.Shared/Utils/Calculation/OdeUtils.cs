using System.Text.Json;
using Microsoft.Extensions.Localization;
using NumCalc.Shared.Enums.ODE;
using NumCalc.Shared.ODE.Requests;
using NumCalc.Shared.ODE.Responses;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.ODE;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Models.User.Enums;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Utils.Calculation;

public static class OdeUtils
{
    public const string ChartContainerId = "chart--ode";
    
    public static OdeComparisonRequest GetComparisonRequest(
        this OdeFormData formData,
        List<OdeMethod> benchmarkMethods)
    {
        return new OdeComparisonRequest
        {
            FunctionExpression = formData.FunctionExpression,
            InitialX = formData.InitialX,
            InitialY = formData.InitialY,
            TargetX = formData.TargetX,
            StepSize = formData.StepSize,
            Methods = benchmarkMethods
        };
    }
    
    public static OdeRequest GetSingleCalculationRequest(this OdeFormData formData)
    {
        return new OdeRequest
        {
            FunctionExpression = formData.FunctionExpression,
            InitialX = formData.InitialX,
            InitialY = formData.InitialY,
            TargetX = formData.TargetX,
            StepSize = formData.StepSize,
            PicardOrder = formData.PicardOrder ?? 4
        };
    }

    public static SaveCalculationRecordRequest GetHistoryRecord(
        this OdeFormData formData,
        OdeResponse result,
        OdeMethod method)
    {
        var inputs = formData.GetMethodInputs(method);
        var resultSummary = result.GetResultSummary();
        
        return new SaveCalculationRecordRequest
        {
            Type = CalculationType.Ode,
            MethodName = method.ToString(),
            InputsJson = JsonSerializer.Serialize(inputs),
            ResultSummary = resultSummary,
            ExecutionTimeMs = result.ExecutionTimeMs
        };
    }
    
    public static Dictionary<string, string> GetMethodInputs(this OdeFormData formData, OdeMethod method)
    {
        var inputs = new Dictionary<string, string>
        {
            ["Method"] = method.ToString(),
            ["x₀"] = formData.InitialX.ToString("G"),
            ["y₀"] = formData.InitialY.ToString("G"),
            ["Target x"] = formData.TargetX.ToString("G"),
            ["Step Size h"] = formData.StepSize.ToString("G")
        };
        if (!string.IsNullOrWhiteSpace(formData.FunctionExpression))
            inputs["f(x, y)"] = formData.FunctionExpression;
        if (method is OdeMethod.Picard)
            inputs["Picard Order"] = (formData.PicardOrder ?? 4).ToString();

        return inputs;
    }

    public static Chart? CreateChartConfig(        
        this OdeFormData formData,
        OdeResponse result,
        OdeMethod method)
    {
        var chartData = result.SolutionPoints?
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData is not { Count: > 0 }) return null;

        var xAxisPlotLines = new List<PlotLine> { ChartUtils.CreateZeroLine() };

        if (method is OdeMethod.Picard)
        {
            xAxisPlotLines.Add(new PlotLine
            {
                Value = formData.InitialX,
                Color = ColorUtils.GetColor(Color.SuccessLight),
                Width = 1,
                DashStyle = LineStyle.Dash
            });
        }

        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            Decimals = MathUtils.DecimalsFromTolerance(null),
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
    }

    public static Chart? CreateBenchmarkChartConfig(
        this OdeFormData formData,
        OdeComparisonResponse comparison,
        IStringLocalizer<Localization> localizer)
    {
        var series = new List<ChartSeries>();

        foreach (var result in comparison.Results)
        {
            var data = result.SolutionPoints?
                .Where(p => p is { X: not null, Y: not null })
                .Select(p => new[] { p.X!.Value, p.Y!.Value })
                .ToList();

            if (data is not { Count: > 0 }) continue;

            series.Add(new ChartSeries
            {
                Name = localizer[result.Method.ToString()],
                Data = data,
                Color = ColorUtils.GetSeriesColor((int)result.Method),
                LineWidth = 2,
                IsVisible = true
            });
        }

        if (series.Count == 0) return null;

        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            ShowLegend = true,
            Decimals = MathUtils.DecimalsFromTolerance(null),
            XAxis = new ChartAxis { Title = "x", PlotLines = [ChartUtils.CreateZeroLine()] },
            YAxis = new ChartAxis { Title = "y(x)", PlotLines = [ChartUtils.CreateZeroLine()] },
            Series = series
        };
    }

    public static string GetResultSummary(this OdeResponse result)
    {
        var lastPoint = result.SolutionPoints?.LastOrDefault();

        return lastPoint is not null
            ? $"y({lastPoint.X.FormatResult()}) ≈ {lastPoint.Y.FormatResult()}"
            : "No solution";
    }
}
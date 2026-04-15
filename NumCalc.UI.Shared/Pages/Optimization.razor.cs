using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Optimization.Requests;
using NumCalc.Shared.Optimization.Responses;
using NumCalc.UI.Shared.Components.Optimization;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Optimization;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Optimization : BasePage<Optimization>
{
    private const string ChartContainerId = "chart--optimization";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private OptimizationMethod _method = OptimizationMethod.UniformSearch;
    private OptimizationInput? _input;
    private OptimizationResponse? Result { get; set; }

    private bool IsChartVisible => Result?.ChartData is not null;

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();

        Func<Task<OptimizationResponse?>> apiCall = _method switch
        {
            OptimizationMethod.UniformSearch => () => CalculationApiService.OptimizeUniformSearchAsync(new OptimizationRequest
            {
                FunctionExpression = formData.FunctionExpression,
                LowerBound = formData.LowerBound,
                UpperBound = formData.UpperBound,
                Points = formData.Points,
                Tolerance = formData.Tolerance
            }),
            OptimizationMethod.GoldenSection => () => CalculationApiService.OptimizeGoldenSectionAsync(new OptimizationRequest
            {
                FunctionExpression = formData.FunctionExpression,
                LowerBound = formData.LowerBound,
                UpperBound = formData.UpperBound,
                Points = formData.Points,
                Tolerance = formData.Tolerance
            }),
            OptimizationMethod.GradientDescent => () => CalculationApiService.OptimizeGradientDescentAsync(new GradientDescentRequest
            {
                FunctionExpression = formData.FunctionExpression,
                InitialPoint = formData.InitialPoint,
                LearningRate = formData.LearningRate,
                Tolerance = formData.Tolerance,
                MaxIterations = formData.MaxIterations
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is not null)
            await UpdateChart();
    }

    private async Task UpdateChart()
    {
        if (Result?.ChartData is null) return;

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
                Color = ColorUtils.GetColor(Enums.Color.Primary),
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
                Color = ColorUtils.GetColor(Enums.Color.SuccessLight),
                IsVisible = true,
                Marker = new ChartMarker { Radius = 5, Symbol = ChartSymbolType.Circle }
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
}

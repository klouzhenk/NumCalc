using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Common;
using NumCalc.Shared.Differentiation.Requests;
using NumCalc.Shared.Differentiation.Responses;
using NumCalc.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Components.Differentiation;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Differentiation : BasePage<Differentiation>
{
    private const string ChartContainerId = "chart--differentiation";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private DifferentiationMethod _method = DifferentiationMethod.FiniteDifferences;
    private FiniteDiffVariant _variant = FiniteDiffVariant.Central;
    private DifferentiationInputMode _mode = DifferentiationInputMode.Function;
    private DifferentiationInput? _input;
    private DifferentiationResponse? Result { get; set; }
    private double _queryPoint;

    private bool IsChartVisible => Result?.ChartData is not null;

    // Step indices: Forward=1, Backward=2, Central=3
    private SolutionStep? SelectedStep => Result?.SolutionSteps?
        .FirstOrDefault(s => s.StepIndex == (int)_variant + 1);

    private IList<SolutionStep>? FilteredSteps => _method is DifferentiationMethod.FiniteDifferences
        ? Result?.SolutionSteps?.Where(s => s.StepIndex == (int)_variant + 1).ToList()
        : Result?.SolutionSteps;

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();
        _queryPoint = formData.QueryPoint;

        var request = new DifferentiationRequest
        {
            Mode = formData.Mode,
            FunctionExpression = formData.FunctionExpression,
            XNodes = formData.XNodes,
            YValues = formData.YValues,
            QueryPoint = formData.QueryPoint,
            StepSize = formData.StepSize,
            DerivativeOrder = formData.DerivativeOrder
        };

        Func<Task<DifferentiationResponse?>> apiCall = _method switch
        {
            DifferentiationMethod.FiniteDifferences => () => CalculationApiService.DifferentiateFiniteDiffAsync(request),
            DifferentiationMethod.Lagrange          => () => CalculationApiService.DifferentiateLagrangeAsync(request),
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
            .Where(p => p.X.HasValue && p.Y.HasValue)
            .Select(p => new double[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData.Count == 0) return;

        var xMin = chartData.Min(p => p[0]);
        var xMax = chartData.Max(p => p[0]);

        // Compute f(x*) from the nearest chart point
        var nearest = chartData.MinBy(p => Math.Abs(p[0] - _queryPoint));
        var fAtXStar = nearest![1];
        var slope = Result.DerivativeValue;

        var tangentData = new List<double[]>
        {
            new[] { xMin, fAtXStar + slope * (xMin - _queryPoint) },
            new[] { xMax, fAtXStar + slope * (xMax - _queryPoint) }
        };

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis
            {
                Title = "x",
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            YAxis = new ChartAxis
            {
                Title = "f(x)",
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            Series =
            [
                new ChartSeries
                {
                    Name = "f(x)",
                    Data = chartData,
                    Color = ColorUtils.GetColor(Enums.Color.Primary),
                    LineWidth = 2,
                    IsVisible = true
                },
                new ChartSeries
                {
                    Name = "Tangent at x*",
                    Data = tangentData,
                    Color = ColorUtils.GetColor(Enums.Color.SuccessLight),
                    LineWidth = 1,
                    IsVisible = true
                },
                new ChartSeries
                {
                    Name = "x*",
                    Type = ChartType.Scatter,
                    Data = [[_queryPoint, fAtXStar]],
                    Color = ColorUtils.GetColor(Enums.Color.SuccessLight),
                    IsVisible = true,
                    Marker = new ChartMarker { Radius = 5, Symbol = ChartSymbolType.Circle }
                }
            ]
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }
}

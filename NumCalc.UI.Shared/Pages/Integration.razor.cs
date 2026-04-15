using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Common;
using NumCalc.Shared.Enums.Integration;
using NumCalc.Shared.Integration.Requests;
using NumCalc.Shared.Integration.Responses;
using NumCalc.UI.Shared.Components.Integration;
using NumCalc.UI.Shared.Enums.Integration;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Integration : BasePage<Integration>
{
    private const string ChartContainerId = "chart--integration";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private IntegrationMethod _method = IntegrationMethod.Trapezoid;
    private RectangleVariant _rectangleVariant = RectangleVariant.Midpoint;
    private IntegrationInput? _input;
    private IntegrationResponse? Result { get; set; }

    private bool IsChartVisible => Result?.ChartData is not null;

    private SolutionStep? SelectedStep => Result?.SolutionSteps?
        .FirstOrDefault(s => s.StepIndex == (int)_rectangleVariant + 1);

    private IList<SolutionStep>? FilteredSteps =>
        _method is IntegrationMethod.RectangleLeft
                or IntegrationMethod.RectangleRight
                or IntegrationMethod.RectangleMiddle
            ? Result?.SolutionSteps?.Where(s => s.StepIndex == (int)_rectangleVariant + 1).ToList()
            : Result?.SolutionSteps;

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();

        var request = new IntegrationRequest
        {
            Mode = IntegrationInputMode.Function,
            FunctionExpression = formData.FunctionExpression,
            LowerBound = formData.LowerBound,
            UpperBound = formData.UpperBound,
            Intervals = formData.Intervals
        };

        Func<Task<IntegrationResponse?>> apiCall = _method switch
        {
            IntegrationMethod.RectangleLeft
                or IntegrationMethod.RectangleRight
                or IntegrationMethod.RectangleMiddle => () => CalculationApiService.IntegrateRectangleAsync(request),
            IntegrationMethod.Trapezoid              => () => CalculationApiService.IntegrateTrapezoidAsync(request),
            IntegrationMethod.Simpson                => () => CalculationApiService.IntegrateSimpsonAsync(request),
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
                }
            ]
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }
}

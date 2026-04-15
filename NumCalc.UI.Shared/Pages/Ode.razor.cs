using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.ODE.Requests;
using NumCalc.Shared.ODE.Responses;
using NumCalc.UI.Shared.Components.ODE;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.ODE;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Ode : BasePage<Ode>
{
    private const string ChartContainerId = "chart--ode";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private OdeMethod _method = OdeMethod.EulerImproved;
    private OdeInput? _input;
    private OdeResponse? Result { get; set; }
    private double _initialX;

    private bool IsChartVisible => Result?.SolutionPoints is { Count: > 0 };

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();
        _initialX = formData.InitialX;

        var request = new OdeRequest
        {
            FunctionExpression = formData.FunctionExpression,
            InitialX = formData.InitialX,
            InitialY = formData.InitialY,
            TargetX = formData.TargetX,
            StepSize = formData.StepSize,
            PicardOrder = formData.PicardOrder ?? 4
        };

        Func<Task<OdeResponse?>> apiCall = _method switch
        {
            OdeMethod.Euler         => () => CalculationApiService.SolveEuler(request),
            OdeMethod.EulerImproved => () => CalculationApiService.SolveEulerImproved(request),
            OdeMethod.RungeKutta2   => () => CalculationApiService.SolveRungeKutta2(request),
            OdeMethod.RungeKutta4   => () => CalculationApiService.SolveRungeKutta4(request),
            OdeMethod.Picard        => () => CalculationApiService.SolvePicard(request),
            _ => throw new ArgumentOutOfRangeException(nameof(_method))
        };

        Result = await SafeExecuteAsync(apiCall);

        if (Result is not null)
            await UpdateChart();
    }

    private async Task UpdateChart()
    {
        if (Result?.SolutionPoints is not { Count: > 0 }) return;

        var chartData = Result.SolutionPoints
            .Where(p => p.X.HasValue && p.Y.HasValue)
            .Select(p => new double[] { p.X!.Value, p.Y!.Value })
            .ToList();

        if (chartData.Count == 0) return;

        var xAxisPlotLines = new List<PlotLine> { ChartUtils.CreateZeroLine() };

        if (_method is OdeMethod.Picard)
        {
            xAxisPlotLines.Add(new PlotLine
            {
                Value = _initialX,
                Color = ColorUtils.GetColor(Color.SuccessLight),
                Width = 1,
                DashStyle = LineStyle.Dash
            });
        }

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
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

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }
}

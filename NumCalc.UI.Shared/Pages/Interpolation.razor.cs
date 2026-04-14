using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.Interpolation;
using NumCalc.Shared.Interpolation.Requests;
using NumCalc.Shared.Interpolation.Responses;
using NumCalc.UI.Shared.Components.Interpolation;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Interpolation;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class Interpolation : BasePage<Interpolation>
{
    private const string ChartContainerId = "chart--interpolation";

    [Inject] private ICalculationApiService CalculationApiService { get; set; } = null!;

    private InterpolationInputMode _mode = InterpolationInputMode.Function;
    private InterpolationMethod _method = InterpolationMethod.Newton;
    private InterpolationInput? _input;
    private InterpolationResponse? Result { get; set; }
    private double _queryPoint;
    private bool IsChartVisible => Result?.ChartData is not null;

    private async Task Calculate()
    {
        Result = null;

        if (_input is null) return;

        var formData = await _input.GetFormData();
        _queryPoint = formData.QueryPoint;

        var request = new InterpolationRequest
        {
            Mode = formData.Mode,
            FunctionExpression = formData.FunctionExpression,
            XNodes = formData.XNodes,
            YValues = formData.YValues,
            QueryPoint = formData.QueryPoint
        };

        Func<Task<InterpolationResponse?>> apiCall = _method switch
        {
            InterpolationMethod.Newton   => () => CalculationApiService.InterpolateNewtonAsync(request),
            InterpolationMethod.Lagrange => () => CalculationApiService.InterpolateLagrangeAsync(request),
            InterpolationMethod.Spline   => () => CalculationApiService.InterpolateSplineAsync(request),
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

        var config = new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis
            {
                Title = Localizer["ArgumentX"],
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            YAxis = new ChartAxis
            {
                Title = Localizer["FunctionValue"],
                PlotLines = [ChartUtils.CreateZeroLine()]
            },
            Series =
            [
                new ChartSeries
                {
                    Name = _method.ToString(),
                    Data = chartData,
                    Color = ColorUtils.GetColor(Color.Primary),
                    LineWidth = 2,
                    IsVisible = true
                },
                new ChartSeries
                {
                    Name = "x*",
                    Type = ChartType.Scatter,
                    Data = [[_queryPoint, Result.InterpolatedValue]],
                    Color = ColorUtils.GetColor(Color.SuccessLight),
                    IsVisible = true,
                    Marker = new ChartMarker { Radius = 5, Symbol = ChartSymbolType.Circle }
                }
            ]
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }
}

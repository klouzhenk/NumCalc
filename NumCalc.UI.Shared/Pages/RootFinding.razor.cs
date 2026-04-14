using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Enums.RootFinding;
using NumCalc.Shared.RootFinding.Requests;
using NumCalc.Shared.RootFinding.Responses;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.RootFinding;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class RootFinding : BasePage<RootFinding>
{
    private const string ChartContainerId = "chart--root-finding";
    
    [Inject] public ICalculationApiService CalculationApiService { get; set; } = null!;
    
    private AnalysisMode Mode { get; set; }
    private List<RootFindingMethod> _benchmarkMethods = [];
    
    private readonly RootFindingFormData _formData = new();
    
    private RootFindingResponse? Result { get; set; }
    private RootFindingComparisonResponse? ComparisonResult { get; set; }
    private MathInput? _mathInputComponent;
    private bool IsChartVisible => !string.IsNullOrWhiteSpace(_formData.FunctionExpression);

    private async Task Calculate()
    {
        Result = null;
        ComparisonResult = null;

        if (Mode == AnalysisMode.Single) await DoSingleMethodCalculation();
        else await DoMultipleMethodCalculations();
    }

    private async Task DoSingleMethodCalculation()
    {
        var requestModel = new RootFindingRequest()
        {
            FunctionExpression = _formData.FunctionExpression ?? string.Empty,
            StartRange = _formData.StartPoint,
            EndRange = _formData.EndPoint,
            Error = _formData.Tolerance
        };

        Func<Task<RootFindingResponse?>> apiCall = _formData.Method switch                                                                                                                                                                
        {
            RootFindingMethod.Dichotomy        => () => CalculationApiService.GetDichotomyResultAsync(requestModel),
            RootFindingMethod.Newton           => () => CalculationApiService.GetNewtonResultAsync(requestModel),
            RootFindingMethod.SimpleIterations => () => CalculationApiService.GetSimpleIterationsResultAsync(requestModel),
            RootFindingMethod.Secant           => () => CalculationApiService.GetSecantResultAsync(requestModel),
            RootFindingMethod.Combined         => () => CalculationApiService.GetCombinedResultAsync(requestModel),
            _ => throw new ArgumentOutOfRangeException(nameof(_formData.Method))
        };

        Result = await SafeExecuteAsync(apiCall);
        
        await UpdateChart();
    }

    private async Task DoMultipleMethodCalculations()
    {
        var request = new RootFindingComparisonRequest()
        {
            FunctionExpression = _formData.FunctionExpression ?? string.Empty,
            StartRange = _formData.StartPoint,
            EndRange = _formData.EndPoint,
            Tolerance = _formData.Tolerance,
            Methods = _benchmarkMethods
        };

        ComparisonResult = await SafeExecuteAsync(() => CalculationApiService.GetBenchmarkResultAsync(request));

        await UpdateChart();
    }

    private async Task OnParametersChanged()
    {
        Result = null;
        ComparisonResult = null;
        await UpdateChart();
    }

    private async Task UpdateChart()
    {
        var asciiEquation = _mathInputComponent is not null
            ? await _mathInputComponent.GetAsciiValue()
            : null;
        if (string.IsNullOrWhiteSpace(asciiEquation)) return;

        var (min, max) = GetChartRange();
        var config = CreateChartConfig(asciiEquation.NormalizeForChart(), min, max);
        AppendResultSeries(config);

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }

    private (double min, double max) GetChartRange()
    {
        var min = _formData.StartPoint;
        var max = _formData.EndPoint;

        if (!(min >= max)) return (min, max);

        max = min + 10;
        min -= 10;

        return (min, max);
    }

    private void AppendResultSeries(Chart config)
    {
        if (Mode is AnalysisMode.Single && Result?.Root.HasValue == true)
        {
            config.Series.Add(new ChartSeries
            {
                Name = $"{Localizer["Root"]} ({_formData.Method})",
                Type = ChartType.Scatter,
                Data = [[Result.Root.Value, 0]],
                Color = ColorUtils.GetColor(Color.SuccessLight),
                IsVisible = true
            });
        }
        else if (Mode is AnalysisMode.Benchmark && ComparisonResult?.Results is { Count: > 0 })
        {
            foreach (var result in ComparisonResult.Results)
            {
                config.Series.Add(new ChartSeries
                {
                    Name = Localizer[result.Method.ToString()],
                    Type = ChartType.Scatter,
                    Data = result.Root.HasValue ? [[result.Root.Value, 0]] : null,
                    Color = ColorUtils.GetSeriesColor((int)result.Method),
                    IsVisible = true,
                    Marker = new ChartMarker { Radius = 5, Symbol = ChartSymbolType.Circle },
                    Opacity = 0.8
                });
            }
        }
    }
    
    private Chart CreateChartConfig(string expression, double min, double max)
    {
        return new Chart
        {
            ContainerId = ChartContainerId,
            Title = null,
            XAxis = new ChartAxis 
            { 
                Min = min, 
                Max = max, 
                Title = Localizer["ArgumentX"],
                PlotLines = [ ChartUtils.CreateZeroLine() ]
            },

            YAxis = new ChartAxis 
            { 
                Title = Localizer["FunctionValue"],
                PlotLines = [ ChartUtils.CreateZeroLine() ]
            },

            Series =
            [
                new ChartSeries
                {
                    Name = "f(x)",
                    Expression = expression,
                    Color = ColorUtils.GetColor(Color.Primary),
                    LineWidth = 2,
                    IsVisible = true
                }
            ]
        };
    }
}
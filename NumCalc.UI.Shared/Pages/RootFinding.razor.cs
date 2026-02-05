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
using NumCalc.UI.Shared.Models;
using NumCalc.UI.Shared.Models.Charts;
using NumCalc.UI.Shared.Models.RootFinding;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Pages;

public partial class RootFinding : BasePage
{
    private const string ChartContainerId = "chart--root-finding";
    
    [Inject] public ICalculationApiService CalculationApiService { get; set; } = null!;
    
    private AnalysisMode Mode { get; set; }
    private RootFindingMethod _singleMethod = RootFindingMethod.Newton;
    private List<RootFindingMethod> _benchmarkMethods = [];
    
    private readonly RootFindingFormData _formData = new();
    
    private RootFindingResponse? Result { get; set; }
    private RootFindingComparisonResponse? ComparisonResult { get; set; }
    private MathInput? _mathInputComponent;
    private bool _isChartBuilt;

    private async Task Calculate()
    {
        Result = null;

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

        Result = _formData.Method switch
        {
            RootFindingMethod.Dichotomy =>
                await SafeExecuteAsync(() =>
                    CalculationApiService.GetDichotomyResultAsync(requestModel)),
            RootFindingMethod.Newton =>
                await SafeExecuteAsync(() =>
                    CalculationApiService.GetNewtonResultAsync(requestModel)),
            RootFindingMethod.SimpleIterations =>
                await SafeExecuteAsync(() =>
                    CalculationApiService.GetSimpleIterationsResultAsync(requestModel)),
            RootFindingMethod.Secant =>
                await SafeExecuteAsync(() =>
                    CalculationApiService.GetSecantResultAsync(requestModel)),
            RootFindingMethod.Combined =>
                await SafeExecuteAsync(() =>
                    CalculationApiService.GetCombinedResultAsync(requestModel)),
            _ => null
            // UiService.ShowError(Localizer["ThereIsNoProperMethod"]);
        };
        
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

    private void ChangeChartAppearing()
    {
        if (!_isChartBuilt)
            _isChartBuilt = true;
        
        if (string.IsNullOrEmpty(_formData.FunctionExpression)) _isChartBuilt = false;
    }

    private async Task OnParametersChanged()
    {
        Result = null;
        ComparisonResult = null;
        await UpdateChart();
    }

    private async Task UpdateChart()
    {
        ChangeChartAppearing();
        var asciiEquation = await _mathInputComponent?.GetAsciiValue();
        if (string.IsNullOrWhiteSpace(asciiEquation)) return;
        
        var min = _formData.StartPoint;
        var max = _formData.EndPoint;
        
        if (min >= max)
        {
            var center = min;
            min = center - 10;
            max = center + 10;
        }

        var config = CreateChartConfig(asciiEquation.NormalizeForChart(), min, max);
        
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
                    Marker = new ChartMarker()
                    {
                        Radius = 5,
                        Symbol = ChartSymbolType.Circle
                    },
                    Opacity = 0.8
                });
            }
        }

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
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
                PlotLines = [ CreateZeroLine() ]
            },

            YAxis = new ChartAxis 
            { 
                Title = Localizer["FunctionValue"],
                PlotLines = [ CreateZeroLine() ]
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
    
    private static PlotLine CreateZeroLine() => new()
    {
        Value = 0,
        Color = ColorUtils.GetColor(Color.GrayUltraLight),
        Width = 2,
        DashStyle = LineStyle.LongDash
    };
}
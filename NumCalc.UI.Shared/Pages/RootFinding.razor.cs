using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
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
    }

    private async Task DoMultipleMethodCalculations()
    {
        var request = new RootFindingRequest
        {
            FunctionExpression = _formData.FunctionExpression ?? string.Empty,
            StartRange = _formData.StartPoint,
            EndRange = _formData.EndPoint,
            Error = _formData.Tolerance
        };

        await SafeExecuteAsync(() => CalculationApiService.GetBenchmarkResultAsync(request, _benchmarkMethods));
    }
    
    private async Task RenderChartAsync()
    {
        var seriesData = Result!.ChartData!
            .Where(p => p is { X: not null, Y: not null })
            .Select(p => new { x = p.X!.Value, y = p.Y!.Value })
            .OrderBy(p => p.x)
            .ToArray();

        var chartOptions = new
        {
            xAxis = new { title = new { text = "X" } },
            yAxis = new { title = new { text = "Y" }, plotLines = new[] { new { value = 0, width = 2, color = "black" } } },
            series = new object[]
            {
                new 
                { 
                    name = "Function", 
                    data = seriesData,
                    color = "#007bff"
                },
                new 
                {
                    type = "scatter",
                    name = "Root",
                    data = new[] { (x: Result.Root, y: 0) },
                    marker = new { radius = 6, fillColor = "red" }
                }
            }
        };
        
        await JsRuntime.InvokeVoidAsync("renderHighchart", "chart--root-finding", chartOptions);
    }

    private void ChangeChartAppearing()
    {
        if (!_isChartBuilt)
            _isChartBuilt = true;
        
        if (string.IsNullOrEmpty(_formData.FunctionExpression)) _isChartBuilt = false;
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
        
        var config = new Chart()
        {
            ContainerId = ChartContainerId,
            Title = null,

            XAxis = new ChartAxis 
            { 
                Min = min, 
                Max = max, 
                Title = Localizer["ArgumentX"],
                PlotLines = 
                [
                    new PlotLine
                    {
                        Value = 0,
                        Color = ColorUtils.GetColor(Color.GrayUltraLight),
                        Width = 2,
                        DashStyle = LineStyle.LongDash
                    }
                ]
            },
        
            YAxis = new ChartAxis 
            { 
                Title = Localizer["FunctionValue"],
                PlotLines =
                [
                    new PlotLine
                    {
                        Value = 0,
                        Color = ColorUtils.GetColor(Color.GrayUltraLight),
                        Width = 2,
                        DashStyle = LineStyle.LongDash
                    }
                ]
            },

            Series =
            [
                new ChartSeries
                {
                    Name = "f(x)",
                    Expression = asciiEquation,
                    Color = ColorUtils.GetColor(Color.Primary),
                    LineWidth = 3
                }
            ]
        };

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", config);
    }
}
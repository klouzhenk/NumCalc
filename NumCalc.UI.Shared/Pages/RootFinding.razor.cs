using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using NumCalc.UI.Shared.Components;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models;

namespace NumCalc.UI.Shared.Pages;

public partial class RootFinding : BasePage
{
    [Inject] public ICalculationApiService CalculationApiService { get; set; } = null!;
    
    private AnalysisMode Mode { get; set; }
    
    private RootFindingModel Model = new();
    private RootFindingComparisonModel ComparisonModel = new();
    
    private MathInput _mathInputComponent;
    private ElementReference _startPointInput;
    private ElementReference _endPointInput;
    private const string ChartContainerId = "chart--root-finding";
    
    private RootFindingResponse? Result { get; set; }
    private bool _shouldRerend;
    private bool _isChartBuilt;

    private async Task Calculate()
    {
        Result = null;

        if (Mode == AnalysisMode.Single) await DoSingleMethodCalculation();
        else await DoMultipleMethodCalculations();
        
        if (Result?.ChartData != null && Result.ChartData.Any())
            _shouldRerend = true;
    }

    private async Task DoSingleMethodCalculation()
    {
        var requestModel = new RootFindingRequest()
        {
            FunctionExpression = Model.FunctionExpression ?? string.Empty,
            StartRange = Model.StartPoint,
            EndRange = Model.EndPoint,
            Error = Model.Tolerance
        };

        Result = Model.Method switch
        {
            RootFindingMethod.Dichotomy =>
                await SafeExecuteAsync<RootFindingResponse?>(() =>
                    CalculationApiService.GetDichotomyResultAsync(requestModel)),
            RootFindingMethod.Newton =>
                await SafeExecuteAsync<RootFindingResponse?>(() =>
                    CalculationApiService.GetNewtonResultAsync(requestModel)),
            RootFindingMethod.SimpleIterations =>
                await SafeExecuteAsync<RootFindingResponse?>(() =>
                    CalculationApiService.GetSimpleIterationsResultAsync(requestModel)),
            RootFindingMethod.Secant =>
                await SafeExecuteAsync<RootFindingResponse?>(() =>
                    CalculationApiService.GetSecantResultAsync(requestModel)),
            RootFindingMethod.Combined =>
                await SafeExecuteAsync<RootFindingResponse?>(() =>
                    CalculationApiService.GetCombinedResultAsync(requestModel)),
            _ => null
            // UiService.ShowError(Localizer["ThereIsNoProperMethod"]);
        };
    }

    private async Task DoMultipleMethodCalculations()
    {
        
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
        
        if (string.IsNullOrEmpty(Model.FunctionExpression)) _isChartBuilt = false;
    }

    private async Task UpdateChart()
    {
        ChangeChartAppearing();
        var asciiEquation = await _mathInputComponent.GetAsciiValue();
        if (string.IsNullOrWhiteSpace(asciiEquation)) return;

        await JsRuntime.InvokeVoidAsync("NumCalc.drawPlot", 
            ChartContainerId,
            asciiEquation,
            Model.StartPoint,
            Model.EndPoint
        );
    }
}
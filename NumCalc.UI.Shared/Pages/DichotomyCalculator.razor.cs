using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public partial class DichotomyCalculator : BasePage
{
    [Inject] protected IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] public ICalculationApiService CalculationApiService { get; set; } = null!;
    
    private RootFindingRequest RequestModel { get; set; } = new();
    
    private RootFindingResponse? Result { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await HandleCalculate();
    }
    
    private async Task HandleCalculate()
    {
        Result = null;

        Result = await SafeExecuteAsync<RootFindingResponse?>(() => CalculationApiService.GetDichotomyResultAsync(RequestModel));
        
        if (Result?.ChartData != null && Result.ChartData.Any())
            await RenderChartAsync();
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
            title = new { text = $"Графік функції: {RequestModel.FunctionExpression}" },
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

        await JsRuntime.InvokeVoidAsync("renderHighchart", "dichotomy-chart", chartOptions);
    }
}
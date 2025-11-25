using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NumCalc.UI.Shared.Pages;

public partial class ChartExample
{
    [Inject] private IJSRuntime JSRuntime { get; set; }
    
    protected override void OnInitialized()
    {
        Console.Write("Hi");
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var options = new
            {
                chart = new { type = "line" },
                title = new { text = "Приклад графіка" },
                series = new object[]
                {
                    new { name = "y = x²", data = new[] { 1, 4, 9, 16, 25 } }
                }
            };

            try
            {
                await JSRuntime.InvokeVoidAsync("renderHighchart", "myChart", options);
            }
            catch (Exception e)
            {
            }
        }
    }
}
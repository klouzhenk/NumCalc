using Microsoft.AspNetCore.Components;
using NumCalc.Shared.Calculation.Requests;
using NumCalc.Shared.Calculation.Responses;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public partial class DichotomyCalculator
{
    [Inject] public ICalculationApiService CalculationApiService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await HandleCalculate();
    }

    private RootFindingResponse? Result { get; set; } 
    
    private async Task HandleCalculate()
    {
        try
        {
            var request = new RootFindingRequest
            {
                FunctionExpression = "x + 5",
                StartRange = -10,
                EndRange = 10,
                Error = 0.0001
            };
            Result = await CalculationApiService.GetDichotomyResultAsync(request);
        }
        catch (Exception e)
        {
            // ignore
        }
    }
}
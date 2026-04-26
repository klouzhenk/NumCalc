using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public partial class CalculationHistory : BasePage<CalculationHistory>
{
    [Inject] private ICalculationHistoryApiService HistoryApiService { get; set; } = null!;
    [Inject] private IAuthStateService AuthStateService { get; set; } = null!;

    private List<CalculationHistoryDto>? Records { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!AuthStateService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        Records = await SafeExecuteAsync(() => HistoryApiService.GetHistoryAsync());
    }

    private async Task DeleteAsync(Guid id)
    {
        await SafeExecuteAsync(async () =>
        {
            await HistoryApiService.DeleteHistoryAsync(id);
            await LoadAsync();
        });
    }
}

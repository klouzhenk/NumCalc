using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;

namespace NumCalc.UI.Shared.Pages;

public partial class CalculationHistory : AuthorizedPage<CalculationHistory>
{
    [Inject] private ICalculationHistoryApiService HistoryApiService { get; set; } = null!;

    private List<CalculationHistoryDto>? Records { get; set; }

    protected override Task OnAuthenticatedInitAsync() => LoadAsync();

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

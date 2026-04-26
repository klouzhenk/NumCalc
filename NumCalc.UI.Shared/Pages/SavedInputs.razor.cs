using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.HttpServices.Interfaces;
using NumCalc.UI.Shared.Models.User;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Pages;

public partial class SavedInputs : BasePage<SavedInputs>
{
    [Inject] private ISavedInputApiService SavedInputApiService { get; set; } = null!;
    [Inject] private IAuthStateService AuthStateService { get; set; } = null!;

    private List<SavedInputDto>? SavedInputsData { get; set; }

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
        SavedInputsData = await SafeExecuteAsync(() => SavedInputApiService.GetSavedInputsAsync());
    }

    private async Task DeleteAsync(Guid id)
    {
        await SafeExecuteAsync(async () =>
        {
            await SavedInputApiService.DeleteSavedInputAsync(id);
            await LoadAsync();
        });
    }
}

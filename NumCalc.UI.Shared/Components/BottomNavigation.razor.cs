using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class BottomNavigation : ComponentBase, IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = null!;

    private bool IsMenuOpen => UiStateService.IsNavMenuOpen;

    protected override void OnInitialized()
    {
        UiStateService.OnNavMenuChanged += OnNavMenuChanged;
    }

    private void OnNavMenuChanged() => InvokeAsync(StateHasChanged);

    private void ToggleMenu() => UiStateService.ToggleNavMenu();

    private void GoHome() => NavigationManager.NavigateTo("/");

    public void Dispose()
    {
        UiStateService.OnNavMenuChanged -= OnNavMenuChanged;
    }
}

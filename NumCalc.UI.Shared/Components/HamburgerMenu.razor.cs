using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Components;

public partial class HamburgerMenu : ComponentBase, IDisposable
{
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject] protected IStringLocalizer<Localization> Localizer { get; set; } = null!;
    [Inject] protected IUiStateService UiStateService { get; set; } = null!;

    protected bool IsMenuOpen => UiStateService.IsNavMenuOpen;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        UiStateService.OnNavMenuChanged += OnNavMenuChanged;
    }

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        => UiStateService.CloseNavMenu();

    private void OnNavMenuChanged() => InvokeAsync(StateHasChanged);

    private void ToggleMenu() => UiStateService.ToggleNavMenu();

    private void CloseMenu() => UiStateService.CloseNavMenu();

    private void OnListItemClick(NavigationItem item)
    {
        if (!NavigationUtils.NavigationItems.TryGetValue(item, out string? relativePath)
            || string.IsNullOrEmpty(relativePath))
            return;

        CloseMenu();
        NavigationManager.NavigateTo($"/{relativePath}");
    }

    private void OpenTopicInfo(NavigationItem item)
    {
        CloseMenu();
        UiStateService.RequestTopicInfo(item);
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        UiStateService.OnNavMenuChanged -= OnNavMenuChanged;
    }
}

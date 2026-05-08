using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Components;

public partial class HamburgerMenu : ComponentBase
{
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject] protected IStringLocalizer<Localization> Localizer { get; set; } = null!;
    [Inject] protected IAuthStateService AuthStateService { get; set; } = null!;
    
    private bool _isMenuOpen;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += (_, _) => CloseMenu();
    }
    
    private void ToggleMenu() => _isMenuOpen = !_isMenuOpen;

    private void CloseMenu()
    {
        _isMenuOpen = false;
        InvokeAsync(StateHasChanged);
    }

    private void OnListItemClick(NavigationItem item)
    {
        if (!NavigationUtils.NavigationItems.TryGetValue(item, out string? relativePath) 
            || string.IsNullOrEmpty(relativePath)) 
            return;
        
        CloseMenu();
        NavigationManager.NavigateTo($"/{relativePath}");
    }
}
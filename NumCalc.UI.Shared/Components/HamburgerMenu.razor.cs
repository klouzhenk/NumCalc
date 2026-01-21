using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components;

public partial class HamburgerMenu : ComponentBase
{
    [Inject] protected NavigationManager NavigationManager { get; set; } = null!;
    [Inject] protected IStringLocalizer<Localization> Localizer { get; set; } = null!;

    private Dictionary<NavigationItem, string> NavigationItems { get; } = new()
    {
        { NavigationItem.Roots, "root-finding" },   
        { NavigationItem.EquationSystems, "equation-systems" },  
        { NavigationItem.Integration, "integration" }, 
        { NavigationItem.Interpolation, "interpolation" },
        { NavigationItem.Differentiation, "differentiation" },
        { NavigationItem.Optimization, "optimization" },
        { NavigationItem.Ode, "ode" },
    };
    
    private bool _isMenuOpen;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += (_, _) => CloseMenu();
    }
    
    private void ToggleMenu() => _isMenuOpen = !_isMenuOpen;
    
    private void CloseMenu() => _isMenuOpen = false;

    private void OnListItemClick(NavigationItem item)
    {
        if (!NavigationItems.TryGetValue(item, out string? relativePath) 
            || string.IsNullOrEmpty(relativePath)) 
            return;
        
        NavigationManager.NavigateTo($"/{relativePath}");
    }
}
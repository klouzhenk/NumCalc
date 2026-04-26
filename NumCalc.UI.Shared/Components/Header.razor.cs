using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Components;

public partial class Header : ComponentBase, IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IAuthStateService AuthStateService { get; set; } = null!;
    [Inject] private ITokenStorage TokenStorage { get; set; } = null!;

    private NavigationItem? CurrentNavItem
    {
        get
        {
            var path = new Uri(NavigationManager.Uri).AbsolutePath.Trim('/');
            var match = NavigationUtils.NavigationItems
                .FirstOrDefault(kv => kv.Value == path);
            return match.Value == path ? match.Key : null;
        }
    }

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
        AuthStateService.OnAuthChanged += OnAuthStateChanged;
    }

    private void OnAuthStateChanged()
    {
        StateHasChanged();
    }

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        StateHasChanged();
    }

    private void OnHeaderLogoClick() =>
        NavigationManager.NavigateTo("/");

    private async Task Logout()
    {
        await TokenStorage.ClearAsync();
        AuthStateService.ClearAuth();
        NavigationManager.NavigateTo("/", true);
    }
    
    private void Login()
    {
        NavigationManager.NavigateTo("/login");
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
        AuthStateService.OnAuthChanged -= OnAuthStateChanged;
    }
}
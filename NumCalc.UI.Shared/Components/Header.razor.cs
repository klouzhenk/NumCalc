using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Components;

public partial class Header : ComponentBase, IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

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
    }

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        StateHasChanged();
    }

    private void OnHeaderLogoClick() =>
        NavigationManager.NavigateTo("/");

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
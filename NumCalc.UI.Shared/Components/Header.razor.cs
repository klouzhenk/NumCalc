using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class Header : ComponentBase, IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private string CurrentPath =>
        new Uri(NavigationManager.Uri).AbsolutePath.TrimEnd('/');

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
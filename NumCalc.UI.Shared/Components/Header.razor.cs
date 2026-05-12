using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Resources;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Components;

public partial class Header : ComponentBase, IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = null!;
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;

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

    private void OpenTopicInfo()
    {
        if (!CurrentNavItem.HasValue) return;
        UiStateService.RequestTopicInfo(CurrentNavItem.Value);
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}

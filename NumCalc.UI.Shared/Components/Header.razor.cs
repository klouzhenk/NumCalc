using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class Header : ComponentBase
{
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private void OnHeaderLogoClick() =>
        NavigationManager.NavigateTo("/");
}
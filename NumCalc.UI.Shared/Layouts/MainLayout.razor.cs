using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Layouts;

public partial class MainLayout : LayoutComponentBase
{
    [Inject] protected IUiStateService UiStateService { get; set; } = null!;

    private void OnGlobalMouseDown()
    {
        UiStateService.RequestCloseDropdown();
    }
}
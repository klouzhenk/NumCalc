using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using NumCalc.UI.Shared.Services.Interfaces;

namespace NumCalc.UI.Shared.Components;

public partial class LanguageSwitch : ComponentBase
{
    [Inject] private ICultureService CultureService { get; set; } = null!;
    [Inject] private ILogger<LanguageSwitch> Logger { get; set; } = null!;
    [Inject] private IUiStateService UiStateService { get; set; } = null!;

    private async Task SetCulture(string culture)
    {
        try
        {
            await CultureService.SetCulture(culture);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{msg}", ex.Message);
            UiStateService.ShowError("Unable to set culture");
        }
    }
}

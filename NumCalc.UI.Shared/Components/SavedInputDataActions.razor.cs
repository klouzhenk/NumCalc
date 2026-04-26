using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components;

public partial class SavedInputDataActions : ComponentBase
{
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    [Parameter] public bool ShowSaveForm { get; set; }
    [Parameter] public string SaveName { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> SaveNameChanged { get; set; }
    [Parameter] public EventCallback OnShowSave { get; set; }
    [Parameter] public EventCallback OnHideSave { get; set; }
    [Parameter] public EventCallback OnConfirmSave { get; set; }
    [Parameter] public EventCallback OnLoad { get; set; }
}

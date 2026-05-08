using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components;

public partial class SavedInputDataActions : ComponentBase
{
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    [Parameter] public EventCallback OnSave { get; set; }
    [Parameter] public EventCallback OnLoad { get; set; }
}

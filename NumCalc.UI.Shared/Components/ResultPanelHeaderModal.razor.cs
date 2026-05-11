using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components;

public partial class ResultPanelHeaderModal : ComponentBase
{
    [Parameter] public RenderFragment? Header { get; set; }
    [Inject] protected IStringLocalizer<Localization> Localizer { get; set; } = null!;
    
    private BaseModal? _modal;
    
    public void Show()
    {
        _modal?.Show();
    }
}
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NumCalc.UI.Shared.Components;

public partial class Tooltip : ComponentBase
{
    [Parameter] public RenderFragment? Content { get; set; }
    [Parameter] public RenderFragment? Hint { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    private readonly string _popupId = $"tooltip-popup-{Guid.NewGuid():N}";
    private bool _rendered;

    private async Task OnFirstHover()
    {
        if (_rendered) return;
        _rendered = true;
        await JsRuntime.InvokeVoidAsync("TooltipHelper.renderLatexInContainer", _popupId);
    }
}

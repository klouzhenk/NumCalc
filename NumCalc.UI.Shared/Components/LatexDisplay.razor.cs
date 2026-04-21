using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NumCalc.UI.Shared.Components;

public partial class LatexDisplay : ComponentBase
{
    [Parameter] public string? Latex { get; set; }
    [Parameter] public string? Prefix { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    private readonly string _containerId = $"latex-display-{Guid.NewGuid():N}";
    private bool _rendered;

    protected override void OnParametersSet() => _rendered = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_rendered || string.IsNullOrEmpty(Latex)) return;
        _rendered = true;
        await JsRuntime.InvokeVoidAsync("TooltipHelper.renderLatexById", _containerId, Prefix + Latex);
    }
}

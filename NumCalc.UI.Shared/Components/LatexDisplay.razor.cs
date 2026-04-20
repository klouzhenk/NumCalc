using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NumCalc.UI.Shared.Components;

public partial class LatexDisplay : ComponentBase
{
    [Parameter] public string? Latex { get; set; }
    [Parameter] public string? Prefix { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    private readonly string _containerId = $"latex-display-{Guid.NewGuid():N}";
    private string? _lastLatex;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Latex == _lastLatex || string.IsNullOrEmpty(Latex)) return;
        _lastLatex = Latex;
        await JsRuntime.InvokeVoidAsync("TooltipHelper.renderStepFormulas", _containerId);
    }
}

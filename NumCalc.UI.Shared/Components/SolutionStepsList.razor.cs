using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using NumCalc.Shared.Common;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components;

public partial class SolutionStepsList : ComponentBase
{
    [Parameter] public IList<SolutionStep>? Steps { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;

    private readonly string _containerId = $"solution-steps-{Guid.NewGuid():N}";
    private bool _latexRendered;

    protected override void OnParametersSet() => _latexRendered = false;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_latexRendered && Steps is { Count: > 0 })
        {
            _latexRendered = true;
            await JsRuntime.InvokeVoidAsync("TooltipHelper.renderStepFormulas", _containerId);
        }
    }
}

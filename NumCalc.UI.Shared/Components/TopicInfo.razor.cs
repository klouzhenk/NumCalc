using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace NumCalc.UI.Shared.Components;

public partial class TopicInfo : ComponentBase
{
    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Subtitle { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    private BaseModal? _modal;
    private readonly string _bodyId = $"topic-info-body-{Guid.NewGuid():N}";

    private async Task Open()
    {
        _modal?.Show();
        await Task.Yield();
        await JsRuntime.InvokeVoidAsync("TooltipHelper.renderLatexInContainer", _bodyId);
    }
}

using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class ResultPanel : ComponentBase
{
    [Parameter, EditorRequired] public RenderFragment ChartContent { get; set; } = null!;
    [Parameter, EditorRequired] public RenderFragment ResultContent { get; set; } = null!;
    [Parameter] public bool HasResult { get; set; }
    [Parameter] public RenderFragment? Header { get; set; }

    private bool _showChart = true;

    protected override void OnParametersSet()
    {
        if (!HasResult) _showChart = true;
    }
}

using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class Divider : ComponentBase
{
    [Parameter, EditorRequired] public string Text { get; set; } = null!;
}
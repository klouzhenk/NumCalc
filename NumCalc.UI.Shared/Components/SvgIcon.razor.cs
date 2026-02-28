using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class SvgIcon : ComponentBase
{
    [Parameter, EditorRequired] public string Name { get; set; } = string.Empty;
    [Parameter] public string CssClass { get; set; } = "icon";
}
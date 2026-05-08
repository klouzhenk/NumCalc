using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class AuthGuard
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
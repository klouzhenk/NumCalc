using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Components;

public partial class AuthGuard
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Inject] private IStringLocalizer<Localization> Localizer { get; set; } = null!;
}
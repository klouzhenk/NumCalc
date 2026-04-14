using Microsoft.AspNetCore.Components;
using NumCalc.Shared.Common;

namespace NumCalc.UI.Shared.Components;

public partial class SolutionStepsList : ComponentBase
{
    [Parameter] public IList<SolutionStep>? Steps { get; set; }
}

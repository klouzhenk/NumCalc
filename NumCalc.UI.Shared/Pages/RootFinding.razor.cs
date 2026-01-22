using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Models;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Pages;

public partial class RootFinding : ComponentBase
{
    [Inject] protected IStringLocalizer<Localization> Localizer { get; set; } = null!;
    
    private RootFindingModel Model = new();
    private RootFindingComparisonModel ComparisonModel = new();
}
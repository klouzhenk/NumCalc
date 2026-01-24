using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.Models;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Pages;

public partial class RootFinding : ComponentBase
{
    [Inject] protected IStringLocalizer<Localization> Localizer { get; set; } = null!;
    
    private AnalysisMode Mode { get; set; }
    
    private RootFindingModel Model = new();
    private RootFindingComparisonModel ComparisonModel = new();

    private Task Calculate()
    {
        throw new NotImplementedException();
    }
}
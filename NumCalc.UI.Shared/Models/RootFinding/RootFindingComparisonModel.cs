using System.ComponentModel.DataAnnotations;
using NumCalc.Shared.Enums.RootFinding;
using NumCalc.UI.Shared.Enums.Roots;
using NumCalc.UI.Shared.Resources;

namespace NumCalc.UI.Shared.Models.RootFinding;

public class RootFindingComparisonModel
{
    [MinLength(1, ErrorMessageResourceName = "PleaseSelectAtLeastOneMethodForBenchmarking", ErrorMessageResourceType = typeof(Localization))]
    public List<RootFindingMethod> SelectedMethods { get; set; } = [];
}
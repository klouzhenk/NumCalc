using NumCalc.Shared.Enums.RootFinding;
using NumCalc.UI.Shared.Enums.Roots;

namespace NumCalc.UI.Shared.Models.RootFinding;

public class RootFindingFormData : RootFindingDetailsModel
{
    public AnalysisMode Mode { get; set; }
    public List<RootFindingMethod> BenchmarkMethods { get; set; } = [];
}
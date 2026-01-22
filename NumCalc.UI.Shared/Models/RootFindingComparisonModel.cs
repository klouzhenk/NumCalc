using NumCalc.UI.Shared.Enums.Roots;

namespace NumCalc.UI.Shared.Models;

public class RootFindingComparisonModel
{
    public List<RootFindingMethod> SelectedMethods { get; set; } = [];
    public double StartPoint { get; set; }
    public double EndPoint { get; set; }
    public double Tolerance { get; set; } = 1e-4;
}
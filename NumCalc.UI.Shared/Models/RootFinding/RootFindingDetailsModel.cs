using NumCalc.Shared.Enums.RootFinding;
using NumCalc.UI.Shared.Enums.Roots;

namespace NumCalc.UI.Shared.Models.RootFinding;

public class RootFindingDetailsModel : RootFindingBaseModel
{
    public RootFindingMethod Method { get; set; }
}
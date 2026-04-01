using NumCalc.Shared.Common;

namespace NumCalc.Calculation.Api.Entities.EquationSystems;

public class SystemSolvingData
{
    public List<double>? Roots { get; set; }
    public List<SolutionStep>? SolutionSteps { get; set; }
}
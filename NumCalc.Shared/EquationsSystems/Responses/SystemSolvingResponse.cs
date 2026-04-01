using NumCalc.Shared.Common;

namespace NumCalc.Shared.EquationsSystems.Responses;

public class SystemSolvingResponse
{
    public List<double>? Roots { get; set; }
    public List<SolutionStep>? SolutionSteps { get; set; }
}
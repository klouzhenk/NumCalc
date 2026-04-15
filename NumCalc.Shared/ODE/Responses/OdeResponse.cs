using NumCalc.Shared.Common;

namespace NumCalc.Shared.ODE.Responses;

public class OdeResponse
{
    public List<Point>? SolutionPoints { get; set; }
    public string? PolynomialLatex { get; set; }
    public List<SolutionStep>? SolutionSteps { get; set; }
    public double ExecutionTimeMs { get; set; }
}
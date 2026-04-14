using NumCalc.Shared.Common;

namespace NumCalc.Shared.Interpolation.Responses;

public class InterpolationResponse
{
    public double InterpolatedValue { get; set; }
    public string? PolynomialLatex { get; set; }
    public IEnumerable<Point>? ChartData { get; set; }
    public List<SolutionStep>? SolutionSteps { get; set; }
    public double ExecutionTimeMs { get; set; }
}

using NumCalc.Shared.Common;

namespace NumCalc.Shared.Integration.Responses;

public class IntegrationResponse
{
    public double IntegralValue { get; set; }
    public IEnumerable<Point>? ChartData { get; set; }
    public List<SolutionStep>? SolutionSteps { get; set; }
    public double ExecutionTimeMs { get; set; }
}

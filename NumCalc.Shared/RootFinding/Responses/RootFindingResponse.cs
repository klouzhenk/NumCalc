using NumCalc.Shared.Common;

namespace NumCalc.Shared.RootFinding.Responses;

public class RootFindingResponse
{
    public double? Root { get; set; }
    public int Iterations { get; set; }
    public IEnumerable<Point>? ChartData { get; set; }
    public List<SolutionStep>? SolutionSteps { get; set; }
    public double ExecutionTimeMs { get; set; }
}
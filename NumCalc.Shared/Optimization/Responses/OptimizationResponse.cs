using NumCalc.Shared.Common;

namespace NumCalc.Shared.Optimization.Responses;

public class OptimizationResponse
{
    public double MinimumValue { get; set; }
    public double? ArgMinX { get; set; }
    public List<double>? ArgMinPoint { get; set; }
    public IEnumerable<Point>? ChartData { get; set; }
    public IEnumerable<Point>? PathData { get; set; }
    public List<SolutionStep>? SolutionSteps { get; set; }
    public double ExecutionTimeMs { get; set; }
}

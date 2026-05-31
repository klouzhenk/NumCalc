using NumCalc.Shared.Common;

namespace NumCalc.Shared.Optimization.Responses;

public class OptimizationResponse
{
    public double ExtremumValue { get; set; }
    public double? ArgExtremumX { get; set; }
    public List<double>? ArgExtremumPoint { get; set; }
    public IEnumerable<Point>? ChartData { get; set; }
    public IEnumerable<Point>? PathData { get; set; }
    public List<SolutionStep>? SolutionSteps { get; set; }
    public double ExecutionTimeMs { get; set; }
}

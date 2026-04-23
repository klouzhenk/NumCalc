using NumCalc.Shared.Enums.Optimization;

namespace NumCalc.Shared.DTOs.Optimization;

public class OptimizationBenchmarkResultDto
{
    public OptimizationComparisonMethod Method { get; set; }
    public double? MinimumValue { get; set; }
    public double? ArgMinX { get; set; }
    public double ExecutionTimeMs { get; set; }
}

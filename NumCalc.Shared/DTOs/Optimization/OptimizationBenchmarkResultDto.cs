using NumCalc.Shared.Enums.Optimization;

namespace NumCalc.Shared.DTOs.Optimization;

public class OptimizationBenchmarkResultDto
{
    public OptimizationComparisonMethod Method { get; set; }
    public double? ExtremumValue { get; set; }
    public double? ArgExtremumX { get; set; }
    public double ExecutionTimeMs { get; set; }
}

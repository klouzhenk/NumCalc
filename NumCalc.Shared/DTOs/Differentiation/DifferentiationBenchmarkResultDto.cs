using NumCalc.Shared.Enums.Differentiation;

namespace NumCalc.Shared.DTOs.Differentiation;

public class DifferentiationBenchmarkResultDto
{
    public DifferentiationComparisonMethod Method { get; set; }
    public double? DerivativeValue { get; set; }
    public double ExecutionTimeMs { get; set; }
}

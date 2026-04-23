using NumCalc.Shared.Enums.Integration;

namespace NumCalc.Shared.DTOs.Integration;

public class IntegrationBenchmarkResultDto
{
    public IntegrationComparisonMethod Method { get; set; }
    public double? IntegralValue { get; set; }
    public double ExecutionTimeMs { get; set; }
}

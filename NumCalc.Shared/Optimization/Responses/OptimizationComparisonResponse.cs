using NumCalc.Shared.DTOs.Optimization;
using NumCalc.Shared.Enums.Optimization;

namespace NumCalc.Shared.Optimization.Responses;

public class OptimizationComparisonResponse
{
    public List<OptimizationBenchmarkResultDto> Results { get; set; } = [];
    public OptimizationComparisonMethod? BestMethod { get; set; }
}

using NumCalc.Shared.DTOs.Differentiation;
using NumCalc.Shared.Enums.Differentiation;

namespace NumCalc.Shared.Differentiation.Responses;

public class DifferentiationComparisonResponse
{
    public List<DifferentiationBenchmarkResultDto> Results { get; set; } = [];
    public DifferentiationComparisonMethod? BestMethod { get; set; }
}

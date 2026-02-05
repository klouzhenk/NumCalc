using NumCalc.Shared.DTOs.RootFinding;
using NumCalc.Shared.Enums.RootFinding;

namespace NumCalc.Shared.RootFinding.Responses;

public class RootFindingComparisonResponse
{
    public List<BenchmarkResultDto> Results { get; set; } = [];
    
    public RootFindingMethod? BestMethod { get; set; }
}
using NumCalc.Shared.DTOs.Integration;
using NumCalc.Shared.Enums.Integration;

namespace NumCalc.Shared.Integration.Responses;

public class IntegrationComparisonResponse
{
    public List<IntegrationBenchmarkResultDto> Results { get; set; } = [];
    public IntegrationComparisonMethod? BestMethod { get; set; }
}

using NumCalc.Shared.DTOs.ODE;
using NumCalc.Shared.Enums.ODE;

namespace NumCalc.Shared.ODE.Responses;

public class OdeComparisonResponse
{
    public List<OdeBenchmarkResultDto> Results { get; set; } = [];
    public OdeMethod? BestMethod { get; set; }
}

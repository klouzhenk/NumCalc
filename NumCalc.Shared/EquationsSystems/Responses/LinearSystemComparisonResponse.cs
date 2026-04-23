using NumCalc.Shared.DTOs.EquationSystems;
using NumCalc.Shared.Enums.EquationSystems;

namespace NumCalc.Shared.EquationsSystems.Responses;

public class LinearSystemComparisonResponse
{
    public List<LinearSystemBenchmarkResultDto> Results { get; set; } = [];
    public LinearSystemMethod? BestMethod { get; set; }
}

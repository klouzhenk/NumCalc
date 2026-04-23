using NumCalc.Shared.DTOs.EquationSystems;
using NumCalc.Shared.Enums.EquationSystems;

namespace NumCalc.Shared.EquationsSystems.Responses;

public class NonLinearSystemComparisonResponse
{
    public List<NonLinearSystemBenchmarkResultDto> Results { get; set; } = [];
    public NonLinearSystemComparisonMethod? BestMethod { get; set; }
}

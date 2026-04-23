using NumCalc.Shared.DTOs.Interpolation;
using NumCalc.Shared.Enums.Interpolation;

namespace NumCalc.Shared.Interpolation.Responses;

public class InterpolationComparisonResponse
{
    public List<InterpolationBenchmarkResultDto> Results { get; set; } = [];
    public InterpolationMethod? BestMethod { get; set; }
}
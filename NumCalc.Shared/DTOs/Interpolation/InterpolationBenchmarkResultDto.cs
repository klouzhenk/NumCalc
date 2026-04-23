using NumCalc.Shared.Enums.Interpolation;

namespace NumCalc.Shared.DTOs.Interpolation;

public class InterpolationBenchmarkResultDto
{
    public InterpolationMethod Method { get; set; }
    public double? InterpolatedValue { get; set; }
    public double ExecutionTimeMs { get; set; }
}
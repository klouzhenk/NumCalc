using NumCalc.Shared.Enums.RootFinding;

namespace NumCalc.Shared.DTOs.RootFinding;

public class BenchmarkResultDto
{
    public RootFindingMethod Method { get; set; }
    public double? Root { get; set; }
    public int Iterations { get; set; }
    public double ExecutionTimeMs { get; set; }
}
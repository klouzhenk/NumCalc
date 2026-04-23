using NumCalc.Shared.Enums.ODE;

namespace NumCalc.Shared.DTOs.ODE;

public class OdeBenchmarkResultDto
{
    public OdeMethod Method { get; set; }
    public double? FinalY { get; set; }
    public double ExecutionTimeMs { get; set; }
}

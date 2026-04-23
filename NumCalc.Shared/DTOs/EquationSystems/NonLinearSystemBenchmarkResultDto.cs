using NumCalc.Shared.Enums.EquationSystems;

namespace NumCalc.Shared.DTOs.EquationSystems;

public class NonLinearSystemBenchmarkResultDto
{
    public NonLinearSystemMethod Method { get; set; }
    public List<double>? Roots { get; set; }
    public double ExecutionTimeMs { get; set; }
}

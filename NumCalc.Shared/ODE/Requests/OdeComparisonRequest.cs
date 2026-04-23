using System.ComponentModel.DataAnnotations;
using NumCalc.Shared.Enums.ODE;

namespace NumCalc.Shared.ODE.Requests;

public class OdeComparisonRequest
{
    public IEnumerable<OdeMethod>? Methods { get; set; }

    [Required]
    public string? FunctionExpression { get; set; }

    public double InitialX { get; set; }
    public double InitialY { get; set; }
    public double TargetX { get; set; }

    [Range(double.Epsilon, double.MaxValue, ErrorMessage = "StepSize must be positive")]
    public double StepSize { get; set; } = 0.1;
}

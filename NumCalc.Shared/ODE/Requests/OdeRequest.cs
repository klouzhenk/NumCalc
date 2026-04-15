using System.ComponentModel.DataAnnotations;

namespace NumCalc.Shared.ODE.Requests;

public class OdeRequest
{
    [Required]
    public string? FunctionExpression { get; set; }
    public double InitialX { get; set; }
    public double InitialY { get; set; }
    public double TargetX { get; set; }
    [Range(double.Epsilon, double.MaxValue, ErrorMessage = "StepSize must be positive")]
    public double StepSize { get; set; } = 0.1;
    [Range(1, 10)]
    public int PicardOrder { get; set; } = 4;
}
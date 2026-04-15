using System.ComponentModel.DataAnnotations;

namespace NumCalc.Shared.ODE.Requests;

public class OdeRequest
{
    [Required]
    public string? FunctionExpression { get; set; }
    public double InitialX { get; set; }
    public double InitialY { get; set; }
    public double TargetX { get; set; }
    public double StepSize { get; set; } = 0.1;
    public int PicardOrder { get; set; } = 4;
}
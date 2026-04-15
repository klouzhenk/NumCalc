namespace NumCalc.UI.Shared.Models.ODE;

public class OdeFormData
{
    public string? FunctionExpression { get; set; }
    public double InitialX { get; set; }
    public double InitialY { get; set; }
    public double TargetX { get; set; } = 1;
    public double StepSize { get; set; } = 0.1;
    public int? PicardOrder { get; set; }
}

using NumCalc.Shared.Enums.ODE;
using NumCalc.UI.Shared.Enums.Roots;

namespace NumCalc.UI.Shared.Models.ODE;

public class OdeFormData
{
    public string? FunctionExpression { get; set; }
    public double InitialX { get; set; }
    public double InitialY { get; set; }
    public double TargetX { get; set; } = 1;
    public double StepSize { get; set; } = 0.1;
    public int? PicardOrder { get; set; }

    public AnalysisMode AnalysisMode { get; set; }
    public OdeMethod Method { get; set; }
    public List<OdeMethod> BenchmarkMethods { get; set; } = [];
}

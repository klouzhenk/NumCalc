using NumCalc.Shared.Enums.Interpolation;
using NumCalc.UI.Shared.Enums.Roots;

namespace NumCalc.UI.Shared.Models.Interpolation;

public class InterpolationFormData
{
    public InterpolationInputMode Mode { get; set; }
    public string? FunctionExpression { get; set; }
    public List<double> XNodes { get; set; } = [];
    public List<double>? YValues { get; set; }
    public double QueryPoint { get; set; }

    public AnalysisMode AnalysisMode { get; set; }
    public InterpolationMethod Method { get; set; }
    public List<InterpolationMethod> BenchmarkMethods { get; set; } = [];
}

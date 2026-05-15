using NumCalc.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Enums.Differentiation;
using NumCalc.UI.Shared.Enums.Roots;

namespace NumCalc.UI.Shared.Models.Differentiation;

public class DifferentiationFormData
{
    public string? FunctionExpression { get; set; }
    public List<double> XNodes { get; set; } = [];
    public List<double>? YValues { get; set; }
    public double? QueryPoint { get; set; }
    public double? StepSize { get; set; } = 0.001;
    public int DerivativeOrder { get; set; } = 1;
    public DifferentiationInputMode Mode { get; set; }

    public AnalysisMode AnalysisMode { get; set; }
    public DifferentiationMethod Method { get; set; }
    public FiniteDiffVariant Variant { get; set; }
    public List<DifferentiationComparisonMethod> BenchmarkMethods { get; set; } = [];
}

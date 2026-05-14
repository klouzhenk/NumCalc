using NumCalc.Shared.Enums.EquationSystems;
using NumCalc.UI.Shared.Enums.EquationSystems;
using NumCalc.UI.Shared.Enums.Roots;

namespace NumCalc.UI.Shared.Models.EquationSystems;

public class EquationSystemsSaveData
{
    public EquationSystemCategory Category { get; set; }
    public int Size { get; set; }
    public AnalysisMode AnalysisMode { get; set; }

    public LinearSystemMethod LinearMethod { get; set; }
    public NonLinearSystemMethod NonLinearMethod { get; set; }
    public List<LinearSystemMethod> LinearBenchmarkMethods { get; set; } = [];
    public List<NonLinearSystemMethod> NonLinearBenchmarkMethods { get; set; } = [];

    public double[][]? Coefficients { get; set; }
    public double[]? Rhs { get; set; }

    public NonLinearSystemFormData? NonLinear { get; set; }
}

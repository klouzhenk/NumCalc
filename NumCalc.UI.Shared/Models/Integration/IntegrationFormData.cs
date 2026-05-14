using NumCalc.Shared.Enums.Integration;
using NumCalc.UI.Shared.Enums.Integration;
using NumCalc.UI.Shared.Enums.Roots;

namespace NumCalc.UI.Shared.Models.Integration;

public class IntegrationFormData
{
    public string? FunctionExpression { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public int Intervals { get; set; } = 100;

    public AnalysisMode AnalysisMode { get; set; }
    public IntegrationMethod Method { get; set; }
    public RectangleVariant RectangleVariant { get; set; }
    public List<IntegrationComparisonMethod> BenchmarkMethods { get; set; } = [];
}

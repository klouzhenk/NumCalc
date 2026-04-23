using System.ComponentModel.DataAnnotations;
using NumCalc.Shared.Enums.Differentiation;

namespace NumCalc.Shared.Differentiation.Requests;

public class DifferentiationComparisonRequest
{
    public IEnumerable<DifferentiationComparisonMethod>? Methods { get; set; }

    [Required]
    public string FunctionExpression { get; set; } = string.Empty;

    public List<double>? XNodes { get; set; }

    [Required]
    public double QueryPoint { get; set; }

    public double StepSize { get; set; } = 0.001;

    [Range(1, 2)]
    public int DerivativeOrder { get; set; } = 1;
}

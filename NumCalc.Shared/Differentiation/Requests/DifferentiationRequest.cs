using System.ComponentModel.DataAnnotations;
using NumCalc.Shared.Enums.Differentiation;

namespace NumCalc.Shared.Differentiation.Requests;

public class DifferentiationRequest
{
    [Required]
    public DifferentiationInputMode Mode { get; set; }

    public string? FunctionExpression { get; set; }

    public List<double>? XNodes { get; set; }

    public List<double>? YValues { get; set; }

    [Required]
    public double QueryPoint { get; set; }

    public double StepSize { get; set; } = 0.001;

    [Range(1, 2)]
    public int DerivativeOrder { get; set; } = 1;
}

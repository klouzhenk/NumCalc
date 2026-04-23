using System.ComponentModel.DataAnnotations;
using NumCalc.Shared.Enums.EquationSystems;

namespace NumCalc.Shared.EquationsSystems.Requests;

public class NonLinearSystemComparisonRequest
{
    [Required]
    [MinLength(2)]
    public required List<string> IterationFunctions { get; set; }

    [Required]
    [MinLength(2)]
    public required List<string> Variables { get; set; }

    [Required]
    public required List<double> InitialGuess { get; set; }

    [Range(1e-15, 1.0)]
    public double Tolerance { get; set; } = 1e-6;

    [Range(1, 10000)]
    public int MaxIterations { get; set; } = 500;

    public IEnumerable<NonLinearSystemMethod>? Methods { get; set; }
}

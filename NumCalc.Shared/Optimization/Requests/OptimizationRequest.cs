using System.ComponentModel.DataAnnotations;

namespace NumCalc.Shared.Optimization.Requests;

public class OptimizationRequest
{
    [Required]
    public string FunctionExpression { get; set; } = string.Empty;

    [Required]
    public double LowerBound { get; set; }

    [Required]
    public double UpperBound { get; set; }

    [Range(2, 10000)]
    public int Points { get; set; } = 100;

    public double Tolerance { get; set; } = 1e-6;

    public bool Maximize { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace NumCalc.Shared.Optimization.Requests;

public class GradientDescentRequest
{
    [Required]
    public string FunctionExpression { get; set; } = string.Empty;

    [Required]
    public List<double> InitialPoint { get; set; } = [];

    public double LearningRate { get; set; } = 0.01;

    public double Tolerance { get; set; } = 1e-6;

    [Range(1, 10000)]
    public int MaxIterations { get; set; } = 200;

    public bool Maximize { get; set; }
}

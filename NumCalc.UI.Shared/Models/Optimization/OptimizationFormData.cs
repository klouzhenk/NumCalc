namespace NumCalc.UI.Shared.Models.Optimization;

public class OptimizationFormData
{
    public string FunctionExpression { get; set; } = string.Empty;

    // 2D methods
    public double LowerBound { get; set; }
    public double UpperBound { get; set; } = 1;
    public int Points { get; set; } = 100;
    public double Tolerance { get; set; } = 1e-6;

    // Gradient descent
    public List<double> InitialPoint { get; set; } = [0.0];
    public double LearningRate { get; set; } = 0.01;
    public int MaxIterations { get; set; } = 200;

    public bool IsGradientDescent { get; set; }
}

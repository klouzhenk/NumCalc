namespace NumCalc.UI.Shared.Models.EquationSystems;

public class NonLinearSystemFormData
{
    public string[] IterationFunctions { get; set; } = [];
    public string[] Variables { get; set; } = [];
    public double[] InitialGuess { get; set; } = [];
    public double Tolerance { get; set; } = 1e-4;
    public int MaxIterations { get; set; } = 100;
}
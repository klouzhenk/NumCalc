namespace NumCalc.UI.Shared.Models.Integration;

public class IntegrationFormData
{
    public string? FunctionExpression { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public int Intervals { get; set; } = 100;
}

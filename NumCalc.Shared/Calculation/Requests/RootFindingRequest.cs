namespace NumCalc.Shared.Calculation.Requests;

public class RootFindingRequest
{
    public string? FunctionExpression { get; set; }
    public double StartRange { get; set; }
    public double EndRange { get; set; }
    public double Error { get; set; }
}
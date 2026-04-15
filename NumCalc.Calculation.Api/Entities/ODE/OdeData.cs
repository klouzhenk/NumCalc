using System.Text.Json.Serialization;
using NumCalc.Shared.Common;

namespace NumCalc.Calculation.Api.Entities.ODE;

public class OdeData
{
    [JsonPropertyName("solution_points")]
    public List<Point>? SolutionPoints { get; set; }

    [JsonPropertyName("polynomial_latex")]
    public string? PolynomialLatex { get; set; }
    
    [JsonPropertyName("solution_steps")]
    public List<SolutionStep>? SolutionSteps { get; set; }
}
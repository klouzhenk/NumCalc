using System.Text.Json.Serialization;
using NumCalc.Shared.Common;

namespace NumCalc.Calculation.Api.Entities.Interpolation;

public class InterpolationData
{
    [JsonPropertyName("interpolated_value")]
    public double InterpolatedValue { get; set; }

    [JsonPropertyName("polynomial_latex")]
    public string? PolynomialLatex { get; set; }

    [JsonPropertyName("chart_points")]
    public IEnumerable<Point>? ChartPoints { get; set; }

    [JsonPropertyName("solution_steps")]
    public List<SolutionStep>? SolutionSteps { get; set; }
}

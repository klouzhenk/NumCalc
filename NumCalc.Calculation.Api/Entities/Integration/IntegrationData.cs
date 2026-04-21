using System.Text.Json.Serialization;
using NumCalc.Shared.Common;

namespace NumCalc.Calculation.Api.Entities.Integration;

public class IntegrationData
{
    [JsonPropertyName("integral_value")]
    public double IntegralValue { get; set; }

    [JsonPropertyName("chart_points")]
    public IEnumerable<Point>? ChartPoints { get; set; }

    [JsonPropertyName("shape_points")]
    public IEnumerable<Point>? ShapePoints { get; set; }

    [JsonPropertyName("solution_steps")]
    public List<SolutionStep>? SolutionSteps { get; set; }
}

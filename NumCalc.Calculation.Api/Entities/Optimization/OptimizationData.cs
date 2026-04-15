using System.Text.Json.Serialization;
using NumCalc.Shared.Common;

namespace NumCalc.Calculation.Api.Entities.Optimization;

public class OptimizationData
{
    [JsonPropertyName("minimum_value")]
    public double MinimumValue { get; set; }

    [JsonPropertyName("arg_min_x")]
    public double? ArgMinX { get; set; }

    [JsonPropertyName("arg_min_point")]
    public List<double>? ArgMinPoint { get; set; }

    [JsonPropertyName("chart_points")]
    public IEnumerable<Point>? ChartPoints { get; set; }

    [JsonPropertyName("solution_steps")]
    public List<SolutionStep>? SolutionSteps { get; set; }
}

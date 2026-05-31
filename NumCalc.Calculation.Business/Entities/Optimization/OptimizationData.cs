using System.Text.Json.Serialization;
using NumCalc.Shared.Common;

namespace NumCalc.Calculation.Business.Entities.Optimization;

public class OptimizationData
{
    [JsonPropertyName("extremum_value")]
    public double ExtremumValue { get; set; }

    [JsonPropertyName("arg_extremum_x")]
    public double? ArgExtremumX { get; set; }

    [JsonPropertyName("arg_extremum_point")]
    public List<double>? ArgExtremumPoint { get; set; }

    [JsonPropertyName("chart_points")]
    public IEnumerable<Point>? ChartPoints { get; set; }

    [JsonPropertyName("path_points")]
    public IEnumerable<Point>? PathPoints { get; set; }

    [JsonPropertyName("solution_steps")]
    public List<SolutionStep>? SolutionSteps { get; set; }
}

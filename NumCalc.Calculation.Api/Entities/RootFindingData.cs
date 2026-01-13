using NumCalc.Shared.Common;
using Point = NumCalc.Shared.Common.Point;
using System.Text.Json.Serialization;

namespace NumCalc.Calculation.Api.Entities;

public class RootFindingData
{
    public double Root { get; set; }
    
    public int Iterations { get; set; }
    
    [JsonPropertyName("chart_points")]
    public IEnumerable<Point>? ChartPoints { get; set; }

    [JsonPropertyName("solution_steps")]
    public List<SolutionStep>? SolutionSteps { get; set; }
}

using System.Text.Json.Serialization;

namespace NumCalc.UI.Shared.Enums.Charts;

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum ChartType
{
    Line,
    Spline,
    Area,
    AreaSpline,
    Column,
    Scatter,
    Bar
}
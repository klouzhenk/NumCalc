
using System.Text.Json.Serialization;

namespace NumCalc.UI.Shared.Enums.Charts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChartSymbolType
{
    Circle,
    Square,
    Diamond,
    Triangle,
    [JsonPropertyName("triangle-down")] 
    TriangleDown
}
using System.Text.Json.Serialization;

namespace NumCalc.UI.Shared.Enums.Charts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChartSymbolType
{
    [JsonStringEnumMemberName("circle")] 
    Circle,
    [JsonStringEnumMemberName("square")] 
    Square,
    [JsonStringEnumMemberName("diamond")] 
    Diamond,
    [JsonStringEnumMemberName("triangle")] 
    Triangle,
    [JsonStringEnumMemberName("triangle-down")] 
    TriangleDown
}

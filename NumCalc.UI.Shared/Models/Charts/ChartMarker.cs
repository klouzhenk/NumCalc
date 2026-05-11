using System.Text.Json.Serialization;
using NumCalc.UI.Shared.Enums.Charts;

namespace NumCalc.UI.Shared.Models.Charts;

public class ChartMarker
{
    public bool Enabled { get; set; } = true;
    public double Radius { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ChartSymbolType Symbol { get; set; }
}
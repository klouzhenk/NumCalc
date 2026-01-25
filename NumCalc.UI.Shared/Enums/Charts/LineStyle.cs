using System.Text.Json.Serialization;

namespace NumCalc.UI.Shared.Enums.Charts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LineStyle
{
    Solid,
    ShortDash,
    ShortDot,
    ShortDashDot,
    ShortDashDotDot,
    Dot,
    Dash,
    LongDash,
    DashDot,
    LongDashDot,
    LongDashDotDot
}
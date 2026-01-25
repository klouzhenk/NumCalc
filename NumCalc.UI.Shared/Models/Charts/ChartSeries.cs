using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Models.Charts;

public class ChartSeries
{
    public string? Name { get; set; }
    public string? Expression { get; set; }
    public string Color { get; set; } = ColorUtils.GetColor(Enums.Color.Primary);
    public ChartType Type { get; set; } = ChartType.Line;
    public LineStyle DashStyle { get; set; } = LineStyle.Solid;
    public int LineWidth { get; set; } = 2;
    public bool IsVisible { get; set; } = true;
}
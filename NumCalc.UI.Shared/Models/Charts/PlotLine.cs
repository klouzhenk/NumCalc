using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Utils;

namespace NumCalc.UI.Shared.Models.Charts;

public class PlotLine
{
    public double Value { get; set; }
    public string Color { get; set; } = ColorUtils.GetColor(Enums.Color.Black);
    public int Width { get; set; } = 2;
    public LineStyle DashStyle { get; set; } = LineStyle.Solid;
}
using NumCalc.UI.Shared.Enums;
using NumCalc.UI.Shared.Enums.Charts;
using NumCalc.UI.Shared.Models.Charts;

namespace NumCalc.UI.Shared.Utils;

public static class ChartUtils
{
    public static PlotLine CreateZeroLine() => new()
    {
        Value = 0,
        Color = ColorUtils.GetColor(Color.GrayUltraLight),
        Width = 2,
        DashStyle = LineStyle.LongDash
    };
    
    public static PlotLine CreateConstant(double constant) => new()
    {
        Value = constant,
        Color = ColorUtils.GetColor(Color.Gray),
        Width = 2,
        DashStyle = LineStyle.LongDash
    };
}
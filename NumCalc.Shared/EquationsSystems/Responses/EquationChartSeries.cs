using NumCalc.Shared.Common;

namespace NumCalc.Shared.EquationsSystems.Responses;

public class EquationChartSeries
{
    public string Label { get; set; } = "";
    public List<Point> Points { get; set; } = [];
}

using Point = NumCalc.Shared.Common.Point;

namespace NumCalc.Calculation.Api.Entities;

public class RootFindingData
{
    public double Root { get; set; }
    public int Iterations { get; set; }
    public IEnumerable<Point>? ChartPoints { get; set; }
}
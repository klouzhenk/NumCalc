using NumCalc.Shared.Common;

namespace NumCalc.Shared.Calculation.Responses;

public class RootFindingResponse
{
    public double? Root { get; set; }
    public int Iterations { get; set; }
    public IEnumerable<Point> ChartData { get; set; } = [];    // TODO: is this necessary?
}
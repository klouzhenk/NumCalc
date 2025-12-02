using NumCalc.Shared.Common;

namespace NumCalc.Shared.Calculation.Responses;

public class RootFindingResponse
{
    public string? Method { get; set; }
    public double? Root { get; set; }
    public int Iterations { get; set; }
    public List<Point> ChartData { get; set; } = [];    // TODO: is this necessary?
}
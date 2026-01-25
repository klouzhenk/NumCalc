namespace NumCalc.UI.Shared.Models.Charts;

public class ChartAxis
{
    public string? Title { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public bool ShowGrid { get; set; } = true;
    public List<PlotLine> PlotLines { get; set; } = [];
}
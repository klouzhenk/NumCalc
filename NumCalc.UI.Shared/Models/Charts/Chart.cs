namespace NumCalc.UI.Shared.Models.Charts;

public class Chart
{
    public required string ContainerId { get; set; }
    public ChartAxis XAxis { get; set; } = new();
    public ChartAxis YAxis { get; set; } = new();

    public List<ChartSeries> Series { get; set; } = [];
    public string? Title { get; set; }
    
    public bool ShowLegend { get; set; } = false;
    public string? TooltipSuffix { get; set; }
}
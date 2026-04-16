namespace NumCalc.UI.Shared.Models.Export;

public class PdfExportRequest
{
    public string MethodName { get; set; } = string.Empty;
    public Dictionary<string, string> Inputs { get; set; } = [];
    public string Result { get; set; } = string.Empty;
    public List<StepExportItem> Steps { get; set; } = [];
    public string? ChartImage { get; set; }
}
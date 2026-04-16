using NumCalc.UI.Shared.Models.Export;

namespace NumCalc.UI.Shared.Services.Interfaces;

public interface IPdfExportService
{
    byte[] GeneratePdf(PdfExportRequest request);
}
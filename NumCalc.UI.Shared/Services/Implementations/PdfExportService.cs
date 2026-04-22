using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Services.Interfaces;
using NumCalc.UI.Shared.Utils;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Color = NumCalc.UI.Shared.Enums.Color;

namespace NumCalc.UI.Shared.Services.Implementations;

public class PdfExportService : IPdfExportService
{
    private static readonly string ColorPrimary       = ColorUtils.GetHexColor(Color.Primary);
    private static readonly string ColorPrimaryLight  = ColorUtils.GetHexColor(Color.PrimaryLight);
    private static readonly string ColorPrimaryBorder = ColorUtils.GetHexColor(Color.PrimaryBorder);
    private static readonly string ColorBorderLight   = ColorUtils.GetHexColor(Color.GrayUltraLight);
    private static readonly string ColorTableHeader   = ColorUtils.GetHexColor(Color.GrayUltraLight);
    private static readonly string ColorTextMuted     = ColorUtils.GetHexColor(Color.Gray);
    private static readonly string ColorFooter        = ColorUtils.GetHexColor(Color.GrayLight);

    static PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GeneratePdf(PdfExportRequest request)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(ComposeHeader(request.MethodName));
                page.Content().Element(ComposeContent(request));
                page.Footer().AlignCenter().DefaultTextStyle(s => s.FontColor(ColorFooter)).Text(text =>
                {
                    text.Span("NumCalc — ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static Action<IContainer> ComposeHeader(string methodName) =>
        container => container
            .BorderBottom(2)
            .BorderColor(ColorPrimary)
            .PaddingBottom(10)
            .Row(row =>
            {
                row.RelativeItem()
                    .Text(methodName).FontSize(18).Bold().FontColor(ColorPrimary);
                row.AutoItem()
                    .AlignBottom()
                    .Text($"{DateTime.Now:dd MMM yyyy, HH:mm}")
                    .FontSize(9).FontColor(ColorTextMuted);
            });

    private static Action<IContainer> ComposeContent(PdfExportRequest request) =>
        container => container
            .PaddingVertical(20)
            .Column(col =>
            {
                col.Spacing(16);

                if (request.Inputs.Count > 0)
                {
                    col.Item().Element(SectionLabel("Input Parameters"));
                    col.Item().Border(1).BorderColor(ColorBorderLight).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(5);
                        });

                        foreach (var (key, value) in request.Inputs)
                        {
                            table.Cell().Background(ColorTableHeader).Padding(6).Text(key).SemiBold();
                            table.Cell().Padding(6).Text(value);
                        }
                    });
                }

                if (request.Steps.Count > 0)
                {
                    col.Item().Element(SectionLabel("Solution Steps"));
                    col.Item().Column(stepsCol =>
                    {
                        stepsCol.Spacing(8);
                        foreach (var (step, index) in request.Steps.Select((s, i) => (s, i + 1)))
                        {
                            stepsCol.Item().ShowEntire().Border(1).BorderColor(ColorBorderLight).Padding(8).Column(stepCol =>
                            {
                                stepCol.Spacing(4);

                                if (!string.IsNullOrWhiteSpace(step.Description))
                                    stepCol.Item()
                                        .Text($"{index}. {step.Description}")
                                        .SemiBold()
                                        .FontColor(ColorPrimary);

                                if (!string.IsNullOrWhiteSpace(step.ImageBase64))
                                {
                                    var imageBytes = DecodeDataUrl(step.ImageBase64);
                                    if (imageBytes is not null)
                                    {
                                        var logicalWidth = PngLogicalWidthPt(imageBytes);
                                        stepCol.Item().MaxWidth(logicalWidth).AlignCenter().Image(imageBytes).FitWidth();
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(step.Value))
                                    stepCol.Item().Text(step.Value).FontColor(ColorTextMuted);
                            });
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(request.Result))
                {
                    col.Item().Element(SectionLabel("Result"));
                    col.Item()
                        .Background(ColorPrimaryLight)
                        .Border(1).BorderColor(ColorPrimaryBorder)
                        .Padding(12)
                        .Text(request.Result)
                        .FontSize(14).Bold().FontColor(ColorPrimary);
                }

                if (!string.IsNullOrWhiteSpace(request.ChartImage))
                {
                    var chartBytes = DecodeDataUrl(request.ChartImage);
                    if (chartBytes is not null)
                    {
                        col.Item().Element(SectionLabel("Chart"));
                        col.Item().AlignCenter().Image(chartBytes).FitWidth();
                    }
                }
            });

    private static byte[]? DecodeDataUrl(string dataUrl)
    {
        try
        {
            var commaIndex = dataUrl.IndexOf(',');
            var base64 = commaIndex >= 0 ? dataUrl[(commaIndex + 1)..] : dataUrl;
            return string.IsNullOrWhiteSpace(base64) ? null : Convert.FromBase64String(base64);
        }
        catch
        {
            return null;
        }
    }

    // html2canvas renders at this scale — divide pixel width by it to get CSS px, then convert to points
    private const float LatexRenderScale = 1.5f;
    private const float CssPxToPt = 72f / 96f;

    private static float PngLogicalWidthPt(byte[] png)
    {
        if (png.Length < 24) return 400f;
        var pxWidth = (png[16] << 24) | (png[17] << 16) | (png[18] << 8) | png[19];
        return pxWidth / LatexRenderScale * CssPxToPt;
    }

    private static Action<IContainer> SectionLabel(string title) =>
        container => container
            .BorderBottom(1)
            .BorderColor(ColorPrimary)
            .PaddingBottom(3)
            .Text(title)
            .FontSize(13)
            .Bold()
            .FontColor(ColorPrimary);
}

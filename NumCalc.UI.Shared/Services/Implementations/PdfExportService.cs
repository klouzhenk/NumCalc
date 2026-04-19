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
    private static readonly string ColorTextMuted = ColorUtils.GetHexColor(Color.Gray);
    private static readonly string ColorFooter   = ColorUtils.GetHexColor(Color.GrayLight);

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
        container => container.Column(col =>
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
                                var imageBytes = Convert.FromBase64String(
                                    step.ImageBase64.Replace("data:image/png;base64,", ""));
                                stepCol.Item().MaxHeight(38).AlignCenter().Image(imageBytes).FitHeight();
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
                col.Item().Element(SectionLabel("Chart"));
                var chartBytes = Convert.FromBase64String(
                    request.ChartImage.Replace("data:image/png;base64,", ""));
                col.Item().AlignCenter().Image(chartBytes).FitWidth();
            }
        });

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

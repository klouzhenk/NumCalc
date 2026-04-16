using NumCalc.UI.Shared.Models.Export;
using NumCalc.UI.Shared.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NumCalc.UI.Shared.Services.Implementations;

public class PdfExportService : IPdfExportService
{
    public byte[] GeneratePdf(PdfExportRequest request)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(ComposeHeader(request.MethodName));
                page.Content().Element(ComposeContent(request));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("NumCalc — ").FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontColor(Colors.Grey.Medium);
                    text.Span(" / ").FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();
    }

    private static Action<IContainer> ComposeHeader(string methodName) =>
        container => container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten1)
            .PaddingBottom(8)
            .Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(methodName).FontSize(18).Bold();
                    col.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy, HH:mm}")
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });

    private static Action<IContainer> ComposeContent(PdfExportRequest request) =>
        container => container.Column(col =>
        {
            col.Spacing(16);

            // Inputs section
            if (request.Inputs.Count > 0)
            {
                col.Item().Element(SectionLabel("Input Parameters"));
                col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(5);
                    });

                    foreach (var (key, value) in request.Inputs)
                    {
                        table.Cell().Background(Colors.Grey.Lighten4).Padding(6).Text(key).SemiBold();
                        table.Cell().Padding(6).Text(value);
                    }
                });
            }

            // Solution steps section
            if (request.Steps.Count > 0)
            {
                col.Item().Element(SectionLabel("Solution Steps"));
                col.Item().Column(stepsCol =>
                {
                    stepsCol.Spacing(8);
                    foreach (var (step, index) in request.Steps.Select((s, i) => (s, i + 1)))
                    {
                        stepsCol.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(stepCol =>
                        {
                            stepCol.Spacing(4);

                            if (!string.IsNullOrWhiteSpace(step.Description))
                                stepCol.Item().Text($"{index}. {step.Description}").SemiBold();

                            if (!string.IsNullOrWhiteSpace(step.ImageBase64))
                            {
                                var imageBytes = Convert.FromBase64String(
                                    step.ImageBase64.Replace("data:image/png;base64,", ""));
                                stepCol.Item().AlignCenter().Image(imageBytes).FitWidth();
                            }

                            if (!string.IsNullOrWhiteSpace(step.Value))
                                stepCol.Item().Text($"= {step.Value}").FontColor(Colors.Grey.Darken2);
                        });
                    }
                });
            }

            // Result section
            if (!string.IsNullOrWhiteSpace(request.Result))
            {
                col.Item().Element(SectionLabel("Result"));
                col.Item()
                    .Background(Colors.Blue.Lighten5)
                    .Border(1).BorderColor(Colors.Blue.Lighten3)
                    .Padding(12)
                    .Text(request.Result)
                    .FontSize(14).Bold();
            }

            // Chart section
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
            .PaddingBottom(4)
            .Text(title)
            .FontSize(13)
            .Bold()
            .FontColor(Colors.Blue.Darken2);
}

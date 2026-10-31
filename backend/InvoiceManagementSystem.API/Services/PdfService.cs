using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using InvoiceManagementSystem.API.Models;
using InvoiceManagementSystem.API.Models.Enums;

namespace InvoiceManagementSystem.API.Services;

public class PdfService : IPdfService
{
    static PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateInvoicePdf(Invoice invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Element(c => ComposeHeader(c, invoice));
                page.Content().Element(c => ComposeContent(c, invoice));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("INVOICE MANAGEMENT SYSTEM")
                        .FontSize(18)
                        .ExtraBold()
                        .FontColor(Colors.Blue.Darken3);

                    col.Item().Text("Sistemi i Menaxhimit të Faturave")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text($"FATURË")
                        .FontSize(20)
                        .ExtraBold()
                        .FontColor(Colors.Grey.Darken4);

                    col.Item().Text($"Nr: {invoice.InvoiceNumber}")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);
                });
            });

            column.Item().PaddingTop(12).PaddingBottom(12).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeContent(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Faturuar për:").FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
                    col.Item().Text(invoice.Customer?.FullName ?? "N/A").FontSize(12).Bold().FontColor(Colors.Grey.Darken4);
                    col.Item().Text(invoice.Customer?.Email ?? string.Empty).FontSize(10).FontColor(Colors.Grey.Darken2);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Data e lëshimit:").FontSize(9).FontColor(Colors.Grey.Darken1);
                        r.ConstantItem(85).AlignRight().Text($"{invoice.IssueDate:dd/MM/yyyy}").FontSize(9).SemiBold();
                    });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Afati i pagesës:").FontSize(9).FontColor(Colors.Grey.Darken1);
                        r.ConstantItem(85).AlignRight().Text($"{invoice.DueDate:dd/MM/yyyy}").FontSize(9).SemiBold();
                    });
                });
            });

            column.Item().PaddingTop(20);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken3).Padding(8).Text("PËRSHKRIMI").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(8).AlignRight().Text("SHUMA (€)").Bold().FontColor(Colors.White);
                });

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10)
                    .Text(string.IsNullOrWhiteSpace(invoice.Description) ? "Shërbim / Faturë e përgjithshme" : invoice.Description);

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(10).AlignRight()
                    .Text($"€{invoice.Amount:N2}").SemiBold();
            });

            column.Item().PaddingTop(12).AlignRight().Container().Width(200).Column(totalCol =>
            {
                totalCol.Item().Row(r =>
                {
                    r.RelativeItem().Text("Nëntotali:").FontSize(10).FontColor(Colors.Grey.Darken1);
                    r.RelativeItem().AlignRight().Text($"€{invoice.Amount:N2}").FontSize(10);
                });

                totalCol.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                totalCol.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text("TOTALI:").FontSize(13).Bold().FontColor(Colors.Blue.Darken3);
                    r.RelativeItem().AlignRight().Text($"€{invoice.Amount:N2}").FontSize(13).ExtraBold().FontColor(Colors.Blue.Darken3);
                });
            });

            if (!string.IsNullOrWhiteSpace(invoice.Notes))
            {
                column.Item().PaddingTop(24).Container()
                    .Background(Colors.Grey.Lighten4)
                    .Padding(12)
                    .Column(noteCol =>
                    {
                        noteCol.Item().Text("Shënime shtesë:").FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                        noteCol.Item().PaddingTop(3).Text(invoice.Notes).FontSize(9).FontColor(Colors.Grey.Darken3);
                    });
            }
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            col.Item().PaddingTop(8).Row(row =>
            {
                row.RelativeItem().Text("Ky dokument është gjeneruar automatikisht nga sistemi.")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Faqja ");
                    text.CurrentPageNumber();
                    text.Span(" nga ");
                    text.TotalPages();
                });
            });
        });
    }
}

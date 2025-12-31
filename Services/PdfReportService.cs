using ExpenseManagementAPI.DTOs;
using ExpenseManagementAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ExpenseManagementAPI.Services
{
    public interface IPdfReportService
    {
        byte[] GenerateMonthlyReportPdf(MonthlyReportDTO report);
    }

    public class PdfReportService : IPdfReportService
    {
              public PdfReportService()
        {
            // Set QuestPDF license to Community (free for revenue < $1M USD)
            QuestPDF.Settings.License = LicenseType.Community;
        }


        public byte[] GenerateMonthlyReportPdf(MonthlyReportDTO report)
        {
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text($"Monthly Expense Report: {report.MonthYear}").SemiBold().FontSize(20).AlignCenter();
                    page.Content().Column(col =>
                    {
                        col.Spacing(5);

                        col.Item().Text($"User: {report.UserFullName}").FontSize(14);
                        col.Item().Text($"Total Expenses: {report.TotalExpenses:C}").FontSize(14);
                        col.Item().Text($"Total Transactions: {report.TotalExpenseCount}").FontSize(14);

                        // Category breakdown table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Category");
                                header.Cell().Text("Amount");
                                header.Cell().Text("Transactions");
                            });

                            foreach (var cat in report.CategoryBreakdown)
                            {
                                table.Cell().Text(cat.CategoryName);
                                table.Cell().Text(cat.Amount.ToString("C"));
                                table.Cell().Text(cat.ExpenseCount.ToString());
                            }
                        });

                        // Top 10 Expenses
                        col.Item().Text("Top 10 Expenses").FontSize(16).SemiBold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Date");
                                header.Cell().Text("Category");
                                header.Cell().Text("Amount");
                                header.Cell().Text("Description");
                            });

                            foreach (var exp in report.TopExpenses)
                            {
                                table.Cell().Text(exp.ExpenseDate.ToString("yyyy-MM-dd"));
                                table.Cell().Text(exp.CategoryName);
                                table.Cell().Text(exp.Amount.ToString("C"));
                                table.Cell().Text(exp.Description ?? "-");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generated on: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"));
                    });
                });
            }).GeneratePdf();

            return pdfBytes;
        }
    }
}

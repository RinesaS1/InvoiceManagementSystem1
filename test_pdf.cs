using System;
using System.IO;
using System.Text;
using InvoiceManagementSystem.API.Models;
using InvoiceManagementSystem.API.Services;

var invoice = new Invoice
{
    Id = 1,
    InvoiceNumber = "INV-2026-TEST",
    Amount = 250.00m,
    Description = "Test Service Payment",
    IssueDate = DateTime.UtcNow,
    DueDate = DateTime.UtcNow.AddDays(15),
    Status = InvoiceStatus.Paid,
    Customer = new User { FullName = "Test Client", Email = "client@example.com" },
    Notes = "Te dhena testuese"
};

var service = new PdfService();
var bytes = service.GenerateInvoicePdf(invoice);
File.WriteAllBytes("test_output.pdf", bytes);

var content = Encoding.UTF8.GetString(bytes);
bool hasPaid = content.Contains("PAGUAR");
bool hasUnpaid = content.Contains("PAPAGUAR");
bool hasInvNum = content.Contains("INV-2026-TEST");
Console.WriteLine($"STATUS_IN_PDF: hasPaid={hasPaid}, hasUnpaid={hasUnpaid}, hasInvNum={hasInvNum}");

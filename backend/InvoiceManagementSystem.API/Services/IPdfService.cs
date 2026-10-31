using InvoiceManagementSystem.API.Models;

namespace InvoiceManagementSystem.API.Services;

public interface IPdfService
{
    byte[] GenerateInvoicePdf(Invoice invoice);
}


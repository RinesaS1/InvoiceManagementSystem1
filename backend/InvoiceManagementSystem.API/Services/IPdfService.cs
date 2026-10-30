using InvoiceManagementSystem.API.Models;

namespace InvoiceManagementSystem.API.Services;

/// <summary>
/// IPdfService — Kontrata e shërbimit për gjenerimin e skedarëve PDF.
/// Mban logjikën e dizajnit dhe formatimit të dokumentit të ndarë nga kontrolleri.
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Gjeneron një dokument PDF profesional për faturën e dhënë.
    /// Të dhënat vijnë direkt nga baza e të dhënave, jo nga frontendi.
    /// </summary>
    byte[] GenerateInvoicePdf(Invoice invoice);
}


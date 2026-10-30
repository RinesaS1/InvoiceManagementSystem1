namespace InvoiceManagementSystem.API.Models.Enums;

/// <summary>
/// InvoiceStatus — Statusi i faturës.
/// Unpaid: Fatura nuk është paguar ende.
/// Paid:   Fatura është paguar dhe konfirmuar.
/// </summary>
public enum InvoiceStatus
{
    Unpaid = 0,
    Paid = 1
}

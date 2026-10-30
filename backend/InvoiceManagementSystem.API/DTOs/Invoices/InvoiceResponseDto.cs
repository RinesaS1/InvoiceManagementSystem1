using InvoiceManagementSystem.API.DTOs.Payments;
using InvoiceManagementSystem.API.Models.Enums;

namespace InvoiceManagementSystem.API.DTOs.Invoices;

/// <summary>
/// InvoiceResponseDto — Të dhënat e faturës që kthehen te klienti (Admin ose Customer).
/// Shmang ekspozimin e drejtpërdrejtë të entitetit të bazës së të dhënave
/// dhe pasuron përgjigjen me emrin dhe emailin e klientit, si dhe historikun e pagesave.
/// </summary>
public class InvoiceResponseDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? Notes { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Lista e të gjitha paraqitjeve të pagesave për këtë faturë.</summary>
    public ICollection<PaymentResponseDto> Payments { get; set; } = new List<PaymentResponseDto>();
}


using InvoiceManagementSystem.API.Models.Enums;

namespace InvoiceManagementSystem.API.DTOs.Payments;

/// <summary>
/// PaymentResponseDto — Të dhënat e paraqitjes së pagesës që kthehen te klienti ose administratori.
/// </summary>
public class PaymentResponseDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal InvoiceAmount { get; set; }
    public DateTime SubmittedAt { get; set; }
    public PaymentStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedById { get; set; }
    public string? VerifiedByName { get; set; }
}


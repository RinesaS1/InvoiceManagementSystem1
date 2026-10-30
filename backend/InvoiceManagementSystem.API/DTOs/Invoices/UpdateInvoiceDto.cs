using System.ComponentModel.DataAnnotations;
using InvoiceManagementSystem.API.Models.Enums;

namespace InvoiceManagementSystem.API.DTOs.Invoices;

/// <summary>
/// UpdateInvoiceDto — Të dhënat për përditësimin e një fature nga Admin.
/// </summary>
public class UpdateInvoiceDto : IValidatableObject
{
    [Required(ErrorMessage = "Numri i faturës është i detyrueshëm.")]
    [MaxLength(50, ErrorMessage = "Numri i faturës nuk mund të kalojë 50 karaktere.")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Përshkrimi nuk mund të kalojë 500 karaktere.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Shuma është e detyrueshme.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Shuma duhet të jetë më e madhe se 0.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Data e lëshimit është e detyrueshme.")]
    public DateTime IssueDate { get; set; }

    [Required(ErrorMessage = "Data e skadencës është e detyrueshme.")]
    public DateTime DueDate { get; set; }

    [MaxLength(1000, ErrorMessage = "Shënimet nuk mund të kalojnë 1000 karaktere.")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Statusi është i detyrueshëm.")]
    public InvoiceStatus Status { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DueDate < IssueDate)
        {
            yield return new ValidationResult(
                "Data e skadencës nuk mund të jetë më e hershme se data e lëshimit.",
                [nameof(DueDate)]);
        }
    }
}


using System.ComponentModel.DataAnnotations;

namespace InvoiceManagementSystem.API.DTOs.Payments;

/// <summary>
/// RejectPaymentDto — Të dhënat që dërgon Admin gjatë refuzimit të një pagese.
/// Arsyeja e refuzimit është e detyrueshme në mënyrë që klienti të njoftohet qartë.
/// </summary>
public class RejectPaymentDto
{
    [Required(ErrorMessage = "Arsyeja e refuzimit është e detyrueshme.")]
    [MaxLength(500, ErrorMessage = "Arsyeja e refuzimit nuk mund të kalojë 500 karaktere.")]
    public string RejectionReason { get; set; } = string.Empty;
}


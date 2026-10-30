using System.ComponentModel.DataAnnotations;
using InvoiceManagementSystem.API.Models.Enums;

namespace InvoiceManagementSystem.API.Models;

/// <summary>
/// Payment — Paraqitja e pagesës nga Klienti.
///
/// Fluksi i pagesës:
///   1. Klienti paraqet pagesën → Status = Pending
///   2a. Admin e konfirmon     → Status = Confirmed, Fattura bëhet Paid
///   2b. Admin e refuzon       → Status = Rejected, RejectionReason plotësohet
///   3. (nëse refuzuar) Klienti mund të bëjë paraqitje të re për të njëjtën faturë
///
/// Prandaj një Invoice mund të ketë SHUMË Payments.
/// </summary>
public class Payment : BaseEntity
{
    // ─── RELACIONI ME FATURËN ─────────────────────────────────────────────────

    /// <summary>FK → Invoice. Fattura për të cilën bëhet kjo pagesë.</summary>
    [Required]
    public int InvoiceId { get; set; }

    // ─── DETAJET E PARAQITJES ─────────────────────────────────────────────────

    /// <summary>
    /// Data dhe ora kur Klienti paraqiti pagesën.
    /// (Trashëgon CreatedAt nga BaseEntity, por e shtojmë edhe SubmittedAt
    /// si alias semantik të qartë.)
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Statusi aktual i pagesës: Pending, Confirmed ose Rejected.</summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // ─── VERIFIKIMI NGA ADMIN ─────────────────────────────────────────────────

    /// <summary>
    /// Arsyeja e refuzimit (plotësohet vetëm nga Admin kur Status = Rejected).
    /// </summary>
    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    /// <summary>Data dhe ora kur Admin verifikoi (konfirmoi ose refuzoi) pagesën.</summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// ID e Adminit që bëri verifikimin.
    /// FK → ApplicationUser (Admin).
    /// </summary>
    public string? VerifiedById { get; set; }

    // ─── NAVIGIM / NAVIGATION PROPERTIES ─────────────────────────────────────

    /// <summary>Fattura e lidhur me këtë pagesë.</summary>
    public Invoice Invoice { get; set; } = null!;

    /// <summary>Admini që verifikoi pagesën.</summary>
    public ApplicationUser? VerifiedBy { get; set; }
}

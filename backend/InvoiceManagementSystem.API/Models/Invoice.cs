using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using InvoiceManagementSystem.API.Models.Enums;

namespace InvoiceManagementSystem.API.Models;

/// <summary>
/// Invoice — Fatura.
/// Çdo faturë i takon një klienti (Customer) dhe mund të ketë
/// shumë paraqitje pagese (Payments), veçanërisht nëse pagesa
/// e mëparshme u refuzua.
/// </summary>
public class Invoice : BaseEntity
{
    // ─── IDENTIFIKIMI ────────────────────────────────────────────────────────

    /// <summary>
    /// Numri unik i faturës (p.sh. "INV-2024-0001").
    /// Gjenerohet nga sistemi dhe nuk mund të përsëritet.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    // ─── RELACIONI ME KLIENTIN ────────────────────────────────────────────────

    /// <summary>FK → ApplicationUser. Klienti të cilit i takon kjo faturë.</summary>
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    // ─── DETAJET E FATURËS ───────────────────────────────────────────────────

    /// <summary>Përshkrimi i faturës (p.sh. "Shërbim mujor Nëntor 2024").</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Shuma e faturës.
    /// DUHET të jetë decimal — jo float/double, për saktësi monetare.
    /// Precizion: 18 shifra totale, 2 shifra pas presjes.
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Shuma duhet të jetë pozitive.")]
    public decimal Amount { get; set; }

    /// <summary>Data e lëshimit të faturës.</summary>
    [Required]
    public DateTime IssueDate { get; set; }

    /// <summary>Data e skadencës — afati i fundit i pagesës.</summary>
    [Required]
    public DateTime DueDate { get; set; }

    /// <summary>Shënime shtesë nga Admin (opsionale).</summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Statusi aktual i faturës: Unpaid ose Paid.</summary>
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;

    // ─── NAVIGIM / NAVIGATION PROPERTIES ─────────────────────────────────────

    /// <summary>Klienti i lidhur me këtë faturë.</summary>
    public ApplicationUser Customer { get; set; } = null!;

    /// <summary>
    /// Lista e paraqitjeve të pagesës për këtë faturë.
    /// Mund të ketë shumë nëse pagesat e mëparshme janë refuzuar.
    /// </summary>
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

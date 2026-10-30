using Microsoft.AspNetCore.Identity;

namespace InvoiceManagementSystem.API.Models;

/// <summary>
/// ApplicationUser — Përdoruesi i aplikacionit.
/// Trashëgon nga IdentityUser i cili siguron fushat standarde:
/// Id, UserName, Email, PasswordHash, PhoneNumber, etj.
///
/// Rolet e mundshme:
///   - "Admin"    → menaxhon faturat, konfirmon pagesat
///   - "Customer" → sheh faturat e veta, paraqet pagesa
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>Emri i plotë i përdoruesit (p.sh. "Rinesa Selmonaj").</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Data e regjistrimit në sistem.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Data e modifikimit të fundit të profilit.</summary>
    public DateTime? UpdatedAt { get; set; }

    // ─── NAVIGIM / NAVIGATION PROPERTIES ─────────────────────────────────────

    /// <summary>
    /// Lista e faturave që i përkasin këtij klienti.
    /// Një klient mund të ketë shumë fatura (One-to-Many).
    /// </summary>
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

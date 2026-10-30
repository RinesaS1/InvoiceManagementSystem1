using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.API.Models;

namespace InvoiceManagementSystem.API.Data;

/// <summary>
/// AppDbContext — Pika kryesore e komunikimit me bazën e të dhënave SQLite.
///
/// Trashëgon nga IdentityDbContext<ApplicationUser> (jo DbContext i thjeshtë),
/// kjo mundëson që tabelat e ASP.NET Core Identity (AspNetUsers, AspNetRoles,
/// AspNetUserRoles, etj.) të krijohen në të njëjtën bazë të dhënash.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ─── TABELAT / DbSets ────────────────────────────────────────────────────

    /// <summary>Tabela e faturave.</summary>
    public DbSet<Invoice> Invoices => Set<Invoice>();

    /// <summary>Tabela e paraqitjeve të pagesave.</summary>
    public DbSet<Payment> Payments => Set<Payment>();

    // ─── KONFIGURIMI I MODELEVE ──────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // DUHET të thirret base.OnModelCreating() kur trashëgojmë nga
        // IdentityDbContext — ky konfiguron tabelat e Identity-t.
        base.OnModelCreating(modelBuilder);

        // ── Invoice ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Invoice>(entity =>
        {
            // Indeks unik mbi InvoiceNumber — dy fatura nuk mund të kenë
            // të njëjtin numër.
            entity.HasIndex(i => i.InvoiceNumber)
                  .IsUnique();

            // Saktësi monetare: 18 shifra totale, 2 shifra decimale.
            // SQLite e ruan si TEXT/REAL, por EF Core e kthen si decimal.
            entity.Property(i => i.Amount)
                  .HasColumnType("decimal(18,2)");

            // ── Relacion: ApplicationUser → Invoice (One-to-Many) ───────────
            // Një Customer mund të ketë shumë fatura.
            // Nëse fshihet Customer-i, Restrict — ndalon fshirjen nëse ka fatura,
            // kjo mbron integritetin e të dhënave.
            entity.HasOne(i => i.Customer)
                  .WithMany(u => u.Invoices)
                  .HasForeignKey(i => i.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Payment ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Payment>(entity =>
        {
            // ── Relacion: Invoice → Payment (One-to-Many) ───────────────────
            // Një faturë mund të ketë shumë paraqitje pagese (nëse refuzohen).
            // Cascade: nëse fshihet fattura, fshihen edhe pagesat e saj.
            entity.HasOne(p => p.Invoice)
                  .WithMany(i => i.Payments)
                  .HasForeignKey(p => p.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);

            // ── Relacion: ApplicationUser → Payment (verifikuesi) ───────────
            // Admini që verifikoi pagesën (opsionale — mund të jetë null
            // kur pagesa është Pending).
            // Restrict: nuk lejon fshirjen e Adminit nëse ka verifikime.
            entity.HasOne(p => p.VerifiedBy)
                  .WithMany()
                  .HasForeignKey(p => p.VerifiedById)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);
        });
    }
}

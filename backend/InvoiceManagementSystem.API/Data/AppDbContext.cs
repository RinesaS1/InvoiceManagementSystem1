using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.API.Models;

namespace InvoiceManagementSystem.API.Data;

/// <summary>
/// Main database context. Inherits from IdentityDbContext so that ASP.NET Core Identity
/// tables (AspNetUsers, AspNetRoles, etc.) live in the same SQLite database.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Required: configures Identity tables
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Invoice>(entity =>
        {
            // InvoiceNumber must be unique across all invoices
            entity.HasIndex(i => i.InvoiceNumber)
                  .IsUnique();

            entity.Property(i => i.Amount)
                  .HasColumnType("decimal(18,2)");

            // One customer → many invoices; Restrict prevents deleting a customer who has invoices
            entity.HasOne(i => i.Customer)
                  .WithMany(u => u.Invoices)
                  .HasForeignKey(i => i.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            // One invoice → many payment submissions (e.g. after a rejection)
            // Cascade: deleting an invoice also deletes its payment records
            entity.HasOne(p => p.Invoice)
                  .WithMany(i => i.Payments)
                  .HasForeignKey(p => p.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);

            // The admin who verified the payment (null until verified)
            // Restrict: cannot delete an admin who has verified payments
            entity.HasOne(p => p.VerifiedBy)
                  .WithMany()
                  .HasForeignKey(p => p.VerifiedById)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);
        });
    }
}

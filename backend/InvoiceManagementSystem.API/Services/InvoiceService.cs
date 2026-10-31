using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.API.Data;
using InvoiceManagementSystem.API.DTOs.Invoices;
using InvoiceManagementSystem.API.DTOs.Payments;
using InvoiceManagementSystem.API.Models;
using InvoiceManagementSystem.API.Models.Enums;

namespace InvoiceManagementSystem.API.Services;

/// <summary>
/// InvoiceService — Implementimi i logjikës së biznesit për faturat.
/// Zbaton rregullat e sigurisë, verifikimin e pronësisë, dhe kufizimet e modifikimit.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InvoiceService(
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ─── MARRJA E FATURAVE (LISTË) ──────────────────────────────────────────

    public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesAsync(
        string currentUserId,
        bool isAdmin,
        string? search = null,
        InvoiceStatus? status = null)
    {
        var query = _context.Invoices
            .Include(i => i.Customer)
            .AsNoTracking()
            .AsQueryable();

        // Security: non-admin users can only see their own invoices
        if (!isAdmin)
        {
            query = query.Where(i => i.CustomerId == currentUserId);
        }

        // Filtri sipas statusit (Unpaid, Paid)
        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        // Kërkimi me tekst (numër fature ose të dhëna të klientit)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(term) ||
                (i.Description != null && i.Description.ToLower().Contains(term)) ||
                (i.Customer.FullName != null && i.Customer.FullName.ToLower().Contains(term)) ||
                (i.Customer.Email != null && i.Customer.Email.ToLower().Contains(term)));
        }

        var invoices = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return invoices.Select(MapToDto);
    }

    // ─── DETAJET E NJË FATURE ───────────────────────────────────────────────

    public async Task<InvoiceResponseDto?> GetInvoiceByIdAsync(
        int id,
        string currentUserId,
        bool isAdmin)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Payments)
                .ThenInclude(p => p.VerifiedBy)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return null;

        // Security: customer can only view their own invoice; return null to avoid revealing other invoices
        if (!isAdmin && invoice.CustomerId != currentUserId)
        {
            return null;
        }

        return MapToDto(invoice);
    }

    // ─── ENTITETI PËR PDF OSE LOGJIKË TË BRENDSHME ──────────────────────────

    public async Task<Invoice?> GetInvoiceEntityAsync(
        int id,
        string currentUserId,
        bool isAdmin)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return null;

        // Security: customer can only download their own invoice
        if (!isAdmin && invoice.CustomerId != currentUserId)
        {
            return null;
        }

        return invoice;
    }

    // ─── KRIJIMI I FATURËS (ADMIN) ──────────────────────────────────────────

    public async Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto dto)
    {
        // 1. Verifiko nëse InvoiceNumber është unik
        var numberExists = await _context.Invoices
            .AnyAsync(i => i.InvoiceNumber == dto.InvoiceNumber);

        if (numberExists)
        {
            throw new InvalidOperationException($"Numri i faturës '{dto.InvoiceNumber}' ekziston tashmë në sistem.");
        }

        // 2. Verifiko nëse klienti ekziston
        var customer = await _userManager.FindByIdAsync(dto.CustomerId);
        if (customer == null)
        {
            throw new InvalidOperationException("Klienti i specifikuar nuk u gjet.");
        }

        // 3. Validimi i datave dhe shumës
        if (dto.DueDate < dto.IssueDate)
        {
            throw new InvalidOperationException("Data e skadencës nuk mund të jetë më e hershme se data e lëshimit.");
        }

        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException("Shuma e faturës duhet të jetë më e madhe se zero.");
        }

        var invoice = new Invoice
        {
            InvoiceNumber = dto.InvoiceNumber.Trim(),
            CustomerId = dto.CustomerId,
            Description = dto.Description?.Trim(),
            Amount = dto.Amount,
            IssueDate = dto.IssueDate,
            DueDate = dto.DueDate,
            Notes = dto.Notes?.Trim(),
            Status = InvoiceStatus.Unpaid, // Fatura e re është gjithmonë Unpaid
            CreatedAt = DateTime.UtcNow
        };

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Ngarko të dhënat e klientit për përgjigjen
        invoice.Customer = customer;

        return MapToDto(invoice);
    }

    // ─── PËRDITËSIMI I FATURËS (ADMIN) ──────────────────────────────────────

    public async Task<InvoiceResponseDto> UpdateInvoiceAsync(int id, UpdateInvoiceDto dto)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
        {
            throw new KeyNotFoundException($"Fatura me ID {id} nuk u gjet.");
        }

        // 1. Verifiko nëse numri i ri i faturës është unik (përveç faturës aktuale)
        var numberExists = await _context.Invoices
            .AnyAsync(i => i.InvoiceNumber == dto.InvoiceNumber && i.Id != id);

        if (numberExists)
        {
            throw new InvalidOperationException($"Numri i faturës '{dto.InvoiceNumber}' po përdoret nga një faturë tjetër.");
        }

        // 2. Validimi i datave dhe shumës
        if (dto.DueDate < dto.IssueDate)
        {
            throw new InvalidOperationException("Data e skadencës nuk mund të jetë më e hershme se data e lëshimit.");
        }

        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException("Shuma e faturës duhet të jetë më e madhe se zero.");
        }

        // Përditëso fushat
        invoice.InvoiceNumber = dto.InvoiceNumber.Trim();
        invoice.Description = dto.Description?.Trim();
        invoice.Amount = dto.Amount;
        invoice.IssueDate = dto.IssueDate;
        invoice.DueDate = dto.DueDate;
        invoice.Notes = dto.Notes?.Trim();
        invoice.Status = dto.Status;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(invoice);
    }

    // ─── FSHIRJA E FATURËS (ADMIN) ──────────────────────────────────────────

    public async Task DeleteInvoiceAsync(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
        {
            throw new KeyNotFoundException($"Fatura me ID {id} nuk u gjet.");
        }

        // RREGULL BIZNESI: Fshi vetëm kur është e përshtatshme
        // 1. Nuk mund të fshihet nëse është e paguar
        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("Një faturë e shlyer (Paid) nuk mund të fshihet për të ruajtur integritetin financiar.");
        }

        // 2. Nuk mund të fshihet nëse ka pagesë të konfirmuar
        if (invoice.Payments.Any(p => p.Status == PaymentStatus.Confirmed))
        {
            throw new InvalidOperationException("Fatura nuk mund të fshihet sepse ka pagesa të konfirmuara në sistem.");
        }

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
    }

    // ─── MAPPING HELPER ─────────────────────────────────────────────────────

    private static InvoiceResponseDto MapToDto(Invoice invoice) => new()
    {
        Id = invoice.Id,
        InvoiceNumber = invoice.InvoiceNumber,
        CustomerId = invoice.CustomerId,
        CustomerName = invoice.Customer?.FullName ?? string.Empty,
        CustomerEmail = invoice.Customer?.Email ?? string.Empty,
        Description = invoice.Description,
        Amount = invoice.Amount,
        IssueDate = invoice.IssueDate,
        DueDate = invoice.DueDate,
        Notes = invoice.Notes,
        Status = invoice.Status,
        CreatedAt = invoice.CreatedAt,
        UpdatedAt = invoice.UpdatedAt,
        Payments = invoice.Payments?.Select(p => new PaymentResponseDto
        {
            Id = p.Id,
            InvoiceId = p.InvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer?.FullName ?? string.Empty,
            InvoiceAmount = invoice.Amount,
            SubmittedAt = p.SubmittedAt,
            Status = p.Status,
            RejectionReason = p.RejectionReason,
            VerifiedAt = p.VerifiedAt,
            VerifiedById = p.VerifiedById,
            VerifiedByName = p.VerifiedBy?.FullName
        }).OrderByDescending(p => p.SubmittedAt).ToList() ?? new List<PaymentResponseDto>()
    };
}

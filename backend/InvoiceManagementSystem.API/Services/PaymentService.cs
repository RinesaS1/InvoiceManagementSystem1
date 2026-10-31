using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.API.Data;
using InvoiceManagementSystem.API.DTOs.Payments;
using InvoiceManagementSystem.API.Models;
using InvoiceManagementSystem.API.Models.Enums;

namespace InvoiceManagementSystem.API.Services;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentService(
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<PaymentResponseDto> RequestPaymentAsync(int invoiceId, string currentUserId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null)
        {
            throw new KeyNotFoundException($"Fatura me ID {invoiceId} nuk u gjet.");
        }

        if (invoice.CustomerId != currentUserId)
        {
            throw new UnauthorizedAccessException("Nuk keni autorizim të paraqitni pagesë për faturën e një klienti tjetër.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new InvalidOperationException("Kjo faturë është shlyer (Paid) tashmë.");
        }

        var hasPendingPayment = await _context.Payments
            .AnyAsync(p => p.InvoiceId == invoiceId && p.Status == PaymentStatus.Pending);

        if (hasPendingPayment)
        {
            throw new InvalidOperationException("Ekziston tashmë një paraqitje pagese në pritje (Pending) për këtë faturë. Ju lutemi prisni verifikimin nga administratori.");
        }

        var payment = new Payment
        {
            InvoiceId = invoiceId,
            SubmittedAt = DateTime.UtcNow,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        payment.Invoice = invoice;

        return MapToDto(payment);
    }

    public async Task<IEnumerable<PaymentResponseDto>> GetPendingPaymentsAsync()
    {
        var payments = await _context.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i.Customer)
            .Include(p => p.VerifiedBy)
            .Where(p => p.Status == PaymentStatus.Pending)
            .OrderBy(p => p.SubmittedAt)
            .AsNoTracking()
            .ToListAsync();

        return payments.Select(MapToDto);
    }

    public async Task<PaymentResponseDto> VerifyPaymentAsync(int paymentId, string adminId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Customer)
                .Include(p => p.VerifiedBy)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                throw new KeyNotFoundException($"Paraqitja e pagesës me ID {paymentId} nuk u gjet.");
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                throw new InvalidOperationException($"Kjo kërkesë pagese nuk mund të konfirmohet sepse është tashmë me statusin '{payment.Status}'.");
            }

            var admin = await _userManager.FindByIdAsync(adminId);

            payment.Status = PaymentStatus.Confirmed;
            payment.VerifiedAt = DateTime.UtcNow;
            payment.VerifiedById = adminId;
            payment.VerifiedBy = admin;
            payment.UpdatedAt = DateTime.UtcNow;

            payment.Invoice.Status = InvoiceStatus.Paid;
            payment.Invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToDto(payment);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PaymentResponseDto> RejectPaymentAsync(int paymentId, string rejectionReason, string adminId)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            throw new InvalidOperationException("Arsyeja e refuzimit është e detyrueshme.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Customer)
                .Include(p => p.VerifiedBy)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                throw new KeyNotFoundException($"Paraqitja e pagesës me ID {paymentId} nuk u gjet.");
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                throw new InvalidOperationException($"Kjo kërkesë pagese nuk mund të refuzohet sepse është tashmë me statusin '{payment.Status}'.");
            }

            var admin = await _userManager.FindByIdAsync(adminId);

            payment.Status = PaymentStatus.Rejected;
            payment.RejectionReason = rejectionReason.Trim();
            payment.VerifiedAt = DateTime.UtcNow;
            payment.VerifiedById = adminId;
            payment.VerifiedBy = admin;
            payment.UpdatedAt = DateTime.UtcNow;

            payment.Invoice.Status = InvoiceStatus.Unpaid;
            payment.Invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToDto(payment);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static PaymentResponseDto MapToDto(Payment payment) => new()
    {
        Id = payment.Id,
        InvoiceId = payment.InvoiceId,
        InvoiceNumber = payment.Invoice?.InvoiceNumber ?? string.Empty,
        CustomerId = payment.Invoice?.CustomerId ?? string.Empty,
        CustomerName = payment.Invoice?.Customer?.FullName ?? string.Empty,
        InvoiceAmount = payment.Invoice?.Amount ?? 0,
        SubmittedAt = payment.SubmittedAt,
        Status = payment.Status,
        RejectionReason = payment.RejectionReason,
        VerifiedAt = payment.VerifiedAt,
        VerifiedById = payment.VerifiedById,
        VerifiedByName = payment.VerifiedBy?.FullName
    };
}


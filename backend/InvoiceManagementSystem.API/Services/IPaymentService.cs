using InvoiceManagementSystem.API.DTOs.Payments;

namespace InvoiceManagementSystem.API.Services;

public interface IPaymentService
{
    Task<PaymentResponseDto> RequestPaymentAsync(int invoiceId, string currentUserId);
    Task<IEnumerable<PaymentResponseDto>> GetPendingPaymentsAsync();
    Task<PaymentResponseDto> VerifyPaymentAsync(int paymentId, string adminId);
    Task<PaymentResponseDto> RejectPaymentAsync(int paymentId, string rejectionReason, string adminId);
}


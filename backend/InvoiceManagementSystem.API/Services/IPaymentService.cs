using InvoiceManagementSystem.API.DTOs.Payments;

namespace InvoiceManagementSystem.API.Services;

/// <summary>
/// IPaymentService — Kontrata e shërbimit për rrjedhën e verifikimit të pagesave.
/// Nuk proceson pagesa reale bankare, por vetëm njoftimet dhe verifikimet manuale nga Admin.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Klienti njofton se ka paguar një faturë të papaguar ("I Have Paid").
    /// Krijon një rekord pagese me statusin Pending.
    /// </summary>
    Task<PaymentResponseDto> RequestPaymentAsync(int invoiceId, string currentUserId);

    /// <summary>
    /// Merr të gjitha kërkesat e pagesave që janë në pritje (Pending) për shqyrtim nga Admin.
    /// </summary>
    Task<IEnumerable<PaymentResponseDto>> GetPendingPaymentsAsync();

    /// <summary>
    /// Admin konfirmon pagesën: statusi i pagesës bëhet Confirmed, fatura bëhet Paid.
    /// Përdor transaksion të bazës së të dhënave për konsistencë.
    /// </summary>
    Task<PaymentResponseDto> VerifyPaymentAsync(int paymentId, string adminId);

    /// <summary>
    /// Admin refuzon pagesën: statusi i pagesës bëhet Rejected me arsyen përkatëse,
    /// fatura mbetet Unpaid. Klienti mund të paraqesë kërkesë të re pas kësaj.
    /// Përdor transaksion të bazës së të dhënave për konsistencë.
    /// </summary>
    Task<PaymentResponseDto> RejectPaymentAsync(int paymentId, string rejectionReason, string adminId);
}


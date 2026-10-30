using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvoiceManagementSystem.API.DTOs;
using InvoiceManagementSystem.API.DTOs.Payments;
using InvoiceManagementSystem.API.Services;

namespace InvoiceManagementSystem.API.Controllers;

/// <summary>
/// PaymentsController — Menaxhon shqyrtimin dhe verifikimin e pagesave nga Admin.
/// Ky sistem nuk kryen pagesa direkte bankare, por verifikon paraqitjet manuale.
/// </summary>
[Route("api/payments")]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // ─── 1. LISTA E PAGESAVE NË PRITJE (GET /api/payments/pending) ───────────

    /// <summary>
    /// Merr të gjitha paraqitjet e pagesave që janë në pritje të shqyrtimit (Pending).
    /// Qasje: VETËM ADMIN.
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PaymentResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IEnumerable<PaymentResponseDto>>>> GetPendingPayments()
    {
        var pendingPayments = await _paymentService.GetPendingPaymentsAsync();
        return OkResponse(pendingPayments);
    }

    // ─── 2. KONFIRMIMI I PAGESËS (PATCH /api/payments/{id}/verify) ───────────

    /// <summary>
    /// Admin konfirmon pagesën e pranuar.
    /// - Statusi i pagesës bëhet Confirmed.
    /// - Fatura e lidhur bëhet Paid.
    /// - Regjistrohet data dhe administratori verifikues.
    /// Qasje: VETËM ADMIN.
    /// </summary>
    [HttpPatch("{id:int}/verify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> VerifyPayment(int id)
    {
        try
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var payment = await _paymentService.VerifyPaymentAsync(id, adminId);
            return OkResponse(payment, "Pagesa u konfirmua me sukses dhe fatura u shënua si e paguar (Paid).");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundResponse<PaymentResponseDto>(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse<PaymentResponseDto>(ex.Message);
        }
    }

    // ─── 3. REFUZIMI I PAGESËS (PATCH /api/payments/{id}/reject) ─────────────

    /// <summary>
    /// Admin refuzon pagesën duke dhënë një arsye të qartë.
    /// - Statusi i pagesës bëhet Rejected me RejectionReason.
    /// - Fatura e lidhur MBETET Unpaid.
    /// - Regjistrohet data dhe administratori verifikues.
    /// - Klienti do të ketë mundësi të paraqesë pagesë të re.
    /// Qasje: VETËM ADMIN.
    /// </summary>
    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> RejectPayment(
        int id,
        [FromBody] RejectPaymentDto dto)
    {
        try
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var payment = await _paymentService.RejectPaymentAsync(id, dto.RejectionReason, adminId);
            return OkResponse(payment, "Pagesa u refuzua. Fatura mbetet e papaguar (Unpaid).");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundResponse<PaymentResponseDto>(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse<PaymentResponseDto>(ex.Message);
        }
    }
}


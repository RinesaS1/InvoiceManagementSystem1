using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvoiceManagementSystem.API.DTOs;
using InvoiceManagementSystem.API.DTOs.Payments;
using InvoiceManagementSystem.API.Services;

namespace InvoiceManagementSystem.API.Controllers;

[Route("api/payments")]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

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


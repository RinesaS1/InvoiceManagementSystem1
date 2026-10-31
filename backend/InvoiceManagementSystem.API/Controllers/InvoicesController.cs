using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvoiceManagementSystem.API.DTOs;
using InvoiceManagementSystem.API.DTOs.Invoices;
using InvoiceManagementSystem.API.DTOs.Payments;
using InvoiceManagementSystem.API.Models.Enums;
using InvoiceManagementSystem.API.Services;

namespace InvoiceManagementSystem.API.Controllers;

[Route("api/invoices")]
public class InvoicesController : BaseApiController
{
    private readonly IInvoiceService _invoiceService;
    private readonly IPaymentService _paymentService;
    private readonly IPdfService _pdfService;

    public InvoicesController(
        IInvoiceService invoiceService,
        IPaymentService paymentService,
        IPdfService pdfService)
    {
        _invoiceService = invoiceService;
        _paymentService = paymentService;
        _pdfService = pdfService;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InvoiceResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IEnumerable<InvoiceResponseDto>>>> GetInvoices(
        [FromQuery] string? search,
        [FromQuery] InvoiceStatus? status)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var isAdmin = User.IsInRole("Admin");

        var invoices = await _invoiceService.GetInvoicesAsync(
            currentUserId,
            isAdmin,
            search,
            status);

        return OkResponse(invoices);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> GetInvoiceById(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var isAdmin = User.IsInRole("Admin");

        var invoice = await _invoiceService.GetInvoiceByIdAsync(id, currentUserId, isAdmin);

        if (invoice == null)
        {
            return NotFoundResponse<InvoiceResponseDto>($"Fatura me ID {id} nuk u gjet.");
        }

        return OkResponse(invoice);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> CreateInvoice([FromBody] CreateInvoiceDto dto)
    {
        try
        {
            var invoice = await _invoiceService.CreateInvoiceAsync(dto);
            return CreatedResponse(invoice, "Fatura u krijua me sukses.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse<InvoiceResponseDto>(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<InvoiceResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<InvoiceResponseDto>>> UpdateInvoice(
        int id,
        [FromBody] UpdateInvoiceDto dto)
    {
        try
        {
            var invoice = await _invoiceService.UpdateInvoiceAsync(id, dto);
            return OkResponse(invoice, "Fatura u përditësua me sukses.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundResponse<InvoiceResponseDto>(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse<InvoiceResponseDto>(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse>> DeleteInvoice(int id)
    {
        try
        {
            await _invoiceService.DeleteInvoiceAsync(id);
            return Ok(ApiResponse.OkNoData("Fatura u fshi me sukses."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("{id:int}/payment-request")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> RequestPayment(int id)
    {
        try
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var payment = await _paymentService.RequestPaymentAsync(id, currentUserId);
            return CreatedResponse(payment, "Njoftimi i pagesës u regjistrua me sukses dhe është në pritje të shqyrtimit nga administratori.");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFoundResponse<PaymentResponseDto>(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<PaymentResponseDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse<PaymentResponseDto>(ex.Message);
        }
    }

    [HttpGet("{id:int}/pdf")]
    [Authorize]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DownloadInvoicePdf(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var isAdmin = User.IsInRole("Admin");

        var invoice = await _invoiceService.GetInvoiceEntityAsync(id, currentUserId, isAdmin);
        if (invoice == null)
        {
            return NotFound(ApiResponse.Fail($"Fatura me ID {id} nuk u gjet."));
        }

        var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);
        var fileName = $"Fatura-{invoice.InvoiceNumber}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}


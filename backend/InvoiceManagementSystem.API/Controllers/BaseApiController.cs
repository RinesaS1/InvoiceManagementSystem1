using Microsoft.AspNetCore.Mvc;
using InvoiceManagementSystem.API.DTOs;

namespace InvoiceManagementSystem.API.Controllers;

/// <summary>
/// BaseApiController — Klasa bazë për të gjithë kontrollerët.
/// Siguron prefiksin /api/ dhe formatin standard të përgjigjes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>Kthen 200 OK me të dhëna.</summary>
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.Ok(data, message));

    /// <summary>Kthen 201 Created me të dhëna.</summary>
    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(T data, string? message = null)
        => StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(data, message));

    /// <summary>Kthen 400 Bad Request me mesazh gabimi.</summary>
    protected ActionResult<ApiResponse<T>> BadRequestResponse<T>(string message)
        => BadRequest(ApiResponse<T>.Fail(message));

    /// <summary>Kthen 404 Not Found me mesazh.</summary>
    protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(string message)
        => NotFound(ApiResponse<T>.Fail(message));
}

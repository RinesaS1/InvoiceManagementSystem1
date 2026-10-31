using Microsoft.AspNetCore.Mvc;
using InvoiceManagementSystem.API.DTOs;

namespace InvoiceManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.Ok(data, message));

    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(T data, string? message = null)
        => StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(data, message));

    protected ActionResult<ApiResponse<T>> BadRequestResponse<T>(string message)
        => BadRequest(ApiResponse<T>.Fail(message));

    protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(string message)
        => NotFound(ApiResponse<T>.Fail(message));
}

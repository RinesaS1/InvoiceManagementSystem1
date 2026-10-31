using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvoiceManagementSystem.API.DTOs;
using InvoiceManagementSystem.API.DTOs.Auth;
using InvoiceManagementSystem.API.Services;
using System.Security.Claims;

namespace InvoiceManagementSystem.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Regjistron një Customer të ri. Roli caktohet automatikisht si "Customer".</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return CreatedResponse(result, "Llogaria u krijua me sukses. Mirë se vini!");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequestResponse<AuthResponseDto>(ex.Message);
        }
    }

    /// <summary>Autentikon përdoruesin dhe kthen JWT token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return OkResponse(result, "Hyrja u krye me sukses.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail(ex.Message));
        }
    }

    /// <summary>Kthen informacionin e përdoruesit aktual nga JWT claims. Kërkon autentifikim.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<object>> Me()
    {
        var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email    = User.FindFirstValue(ClaimTypes.Email);
        var fullName = User.FindFirstValue(ClaimTypes.Name);
        var role     = User.FindFirstValue(ClaimTypes.Role);

        return OkResponse<object>(new
        {
            UserId   = userId,
            Email    = email,
            FullName = fullName,
            Role     = role
        });
    }

    /// <summary>Endpoint vetëm për Admin — demonstron autorizimin bazuar në rol.</summary>
    [HttpGet("admin-test")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<ApiResponse<object>> AdminTest()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        return OkResponse<object>(new
        {
            Message = $"Mirë se vini, Admin {email}! Ky endpoint kërkon rolin Admin."
        });
    }

    /// <summary>Endpoint vetëm për Customer — demonstron autorizimin bazuar në rol.</summary>
    [HttpGet("customer-test")]
    [Authorize(Roles = "Customer")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<ApiResponse<object>> CustomerTest()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        return OkResponse<object>(new
        {
            Message = $"Mirë se vini, Customer {email}! Ky endpoint kërkon rolin Customer."
        });
    }
}


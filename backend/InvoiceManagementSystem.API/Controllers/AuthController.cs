using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvoiceManagementSystem.API.DTOs;
using InvoiceManagementSystem.API.DTOs.Auth;
using InvoiceManagementSystem.API.Services;
using System.Security.Claims;

namespace InvoiceManagementSystem.API.Controllers;

/// <summary>
/// AuthController — Menaxhon regjistrimin, hyrjen dhe informacionin e sesionit.
///
/// Endpoint-et publike (pa autentifikim):
///   POST /api/auth/register  → regjistron Customer të ri
///   POST /api/auth/login     → kthen JWT token
///
/// Endpoint-et e mbrojtura:
///   GET  /api/auth/me        → informacioni i përdoruesit aktual (çdo rol)
///   GET  /api/auth/admin-test → vetëm Admin (demonstrim i autorizimit)
/// </summary>
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // ─── REGJISTRIM ──────────────────────────────────────────────────────────

    /// <summary>
    /// Regjistron një Customer të ri.
    /// Ky endpoint është PUBLIK — nuk kërkon token.
    /// Roli caktohet automatikisht si "Customer"; nuk mund të zgjidhet nga klienti.
    /// </summary>
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

    // ─── LOGIN ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Autentikon përdoruesin me email + fjalëkalim.
    /// Ky endpoint është PUBLIK — nuk kërkon token.
    /// Kthen JWT token që duhet dërguar me çdo kërkesë private.
    /// </summary>
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

    // ─── INFORMACIONI I SESIONIT ──────────────────────────────────────────────

    /// <summary>
    /// Kthen informacionin e përdoruesit aktual.
    /// Kërkon autentifikim — çdo rol (Admin ose Customer).
    /// Frontendit i shërben për të shfaqur emrin/rolin pa bërë kërkesë shtesë.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<object>> Me()
    {
        // Lexo claims nga JWT token-i i verifikuar
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

    // ─── TEST AUTORIZIMI ──────────────────────────────────────────────────────

    /// <summary>
    /// Endpoint vetëm për Admin — demonstron autorizimin bazuar në rol.
    /// Customer që mundohet të hyjë merr 403 Forbidden.
    /// </summary>
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

    /// <summary>
    /// Endpoint vetëm për Customer — demonstron autorizimin bazuar në rol.
    /// Admin që mundohet të hyjë merr 403 Forbidden.
    /// </summary>
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


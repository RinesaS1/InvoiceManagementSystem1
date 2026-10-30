using InvoiceManagementSystem.API.DTOs.Auth;

namespace InvoiceManagementSystem.API.Services;

/// <summary>
/// IAuthService — Kontrata e shërbimit të autentikimit.
/// AuthController varet nga kjo ndërfaqe, jo nga implementimi konkret.
/// Kjo mundëson testim të lehtë (mock) dhe ndarje të qartë të detyrave.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Regjistron një përdorues të ri si Customer.
    /// Nëse email ekziston ose validimi dështon, hedh exception.
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Autentikon përdoruesin me email + fjalëkalim.
    /// Kthen JWT token nëse kredencialet janë të sakta.
    /// Hedh exception nëse kredencialet janë të gabuara.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}


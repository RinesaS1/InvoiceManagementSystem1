using InvoiceManagementSystem.API.DTOs.Auth;

namespace InvoiceManagementSystem.API.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}


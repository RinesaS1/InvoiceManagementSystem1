using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using InvoiceManagementSystem.API.DTOs.Auth;
using InvoiceManagementSystem.API.Models;

namespace InvoiceManagementSystem.API.Services;

/// <summary>
/// AuthService — Implementimi i shërbimit të autentikimit.
///
/// Fluksi i regjistrimit:
///   1. Valido DTO (bëhet nga [ApiController] automatikisht)
///   2. Krijo ApplicationUser me UserManager (hashon fjalëkalimin)
///   3. Cakto rolin "Customer"
///   4. Gjenero JWT token
///   5. Kthe AuthResponseDto
///
/// Fluksi i loginit:
///   1. Gjej përdoruesin me email
///   2. Verifiko fjalëkalimin me UserManager (krahason hash)
///   3. Gjenero JWT token
///   4. Kthe AuthResponseDto
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    // ─── REGJISTRIM ──────────────────────────────────────────────────────────

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Verifiko nëse email-i ekziston tashmë
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            throw new InvalidOperationException("Një llogari me këtë email ekziston tashmë.");

        // Krijo ApplicationUser — Identity do të hashëjë fjalëkalimin
        var user = new ApplicationUser
        {
            UserName = dto.Email,      // Identity kërkon UserName
            Email = dto.Email,
            FullName = dto.FullName,
            CreatedAt = DateTime.UtcNow
        };

        // CreateAsync: hashon fjalëkalimin dhe ruan në AspNetUsers
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new InvalidOperationException(
                $"Regjistrimi dështoi: {string.Join(", ", errors)}");
        }

        // Cakto gjithmonë rolin Customer — kurrë Admin përmes kësaj metode
        await _userManager.AddToRoleAsync(user, "Customer");

        return await BuildAuthResponseAsync(user);
    }

    // ─── LOGIN ───────────────────────────────────────────────────────────────

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // Kërko përdoruesin me email
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            throw new UnauthorizedAccessException("Kredencialet janë të gabuara.");

        // Verifiko fjalëkalimin (krahason me hash-in e ruajtur)
        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            throw new UnauthorizedAccessException("Kredencialet janë të gabuara.");

        return await BuildAuthResponseAsync(user);
    }

    // ─── GJENERIMI I JWT TOKEN ────────────────────────────────────────────────

    private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user)
    {
        // Merr rolet e përdoruesit (Admin ose Customer)
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Customer";

        var token = GenerateJwtToken(user, role, out var expiresAt);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Role = role,
            ExpiresAt = expiresAt
        };
    }

    private string GenerateJwtToken(
        ApplicationUser user,
        string role,
        out DateTime expiresAt)
    {
        // Lexo konfigurimin e JWT nga appsettings
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key mungon në konfigurim.");
        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer mungon në konfigurim.");
        var audience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience mungon në konfigurim.");
        var expiresInMinutes = int.Parse(
            _configuration["Jwt:ExpiresInMinutes"] ?? "60");

        // Çelësi simetrik për nënshkrimin e token-it (HMAC-SHA256)
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        // Claims — informacioni i ngulitur në token
        // Klienti mund t'i lexojë (nuk janë sekrete), por nuk mund t'i falsifikojë
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID unike e token-it
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, role),        // ← [Authorize(Roles="Admin")] e lexon këtë
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}


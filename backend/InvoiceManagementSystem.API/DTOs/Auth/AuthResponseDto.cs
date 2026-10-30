namespace InvoiceManagementSystem.API.DTOs.Auth;

/// <summary>
/// AuthResponseDto — Përgjigja e suksesshme e autentikimit (regjistrim ose login).
///
/// Klienti e ruan token-in (zakonisht në localStorage ose memory)
/// dhe e dërgon me çdo kërkesë private si:
///   Authorization: Bearer {Token}
/// </summary>
public class AuthResponseDto
{
    /// <summary>JWT token-i — i vlefshëm për ExpiresAt.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>ID e përdoruesit (GUID string nga Identity).</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Email-i i përdoruesit.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Emri i plotë.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Roli: "Admin" ose "Customer".</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Data dhe ora e skadencës së token-it (UTC).</summary>
    public DateTime ExpiresAt { get; set; }
}


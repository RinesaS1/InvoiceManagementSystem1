using System.ComponentModel.DataAnnotations;

namespace InvoiceManagementSystem.API.DTOs.Auth;

/// <summary>
/// LoginDto — Kredencialet e hyrjes.
/// Klienti dërgon email dhe fjalëkalim, sistemi kthen një JWT token.
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "Email-i është i detyrueshëm.")]
    [EmailAddress(ErrorMessage = "Formati i email-it është i pavlefshëm.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fjalëkalimi është i detyrueshëm.")]
    public string Password { get; set; } = string.Empty;
}


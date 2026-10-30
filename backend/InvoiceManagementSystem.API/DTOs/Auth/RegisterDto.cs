using System.ComponentModel.DataAnnotations;

namespace InvoiceManagementSystem.API.DTOs.Auth;

/// <summary>
/// RegisterDto — Të dhënat që klienti dërgon për t'u regjistruar.
///
/// SIGURI: Roli nuk pranohet nga klienti — çdo regjistrim publik
/// krijon automatikisht një llogari Customer. Admin krijohet vetëm
/// nga seed-i i konfigurimit.
/// </summary>
public class RegisterDto
{
    /// <summary>Emri i plotë i përdoruesit (p.sh. "Rinesa Selmonaj").</summary>
    [Required(ErrorMessage = "Emri i plotë është i detyrueshëm.")]
    [MaxLength(100, ErrorMessage = "Emri nuk mund të kalojë 100 karaktere.")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Email-i — përdoret si username për login.</summary>
    [Required(ErrorMessage = "Email-i është i detyrueshëm.")]
    [EmailAddress(ErrorMessage = "Formati i email-it është i pavlefshëm.")]
    [MaxLength(256, ErrorMessage = "Email-i nuk mund të kalojë 256 karaktere.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Fjalëkalimi — do të hashëtohet para se të ruhet.</summary>
    [Required(ErrorMessage = "Fjalëkalimi është i detyrueshëm.")]
    [MinLength(8, ErrorMessage = "Fjalëkalimi duhet të ketë të paktën 8 karaktere.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Konfirmimi i fjalëkalimit — duhet të jetë identik me Password.</summary>
    [Required(ErrorMessage = "Konfirmimi i fjalëkalimit është i detyrueshëm.")]
    [Compare(nameof(Password), ErrorMessage = "Fjalëkalimet nuk përputhen.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}


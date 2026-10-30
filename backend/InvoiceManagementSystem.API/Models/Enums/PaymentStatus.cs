namespace InvoiceManagementSystem.API.Models.Enums;

/// <summary>
/// PaymentStatus — Statusi i pagesës.
/// Pending:   Pagesa është paraqitur dhe pret verifikimin nga Admin.
/// Confirmed: Admin e ka konfirmuar pagesën — fattura konsiderohet e shlyer.
/// Rejected:  Admin e ka refuzuar pagesën (p.sh. shumë e gabuar, provë e pavlefshme).
///            Klienti mund të bëjë paraqitje të re.
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Confirmed = 1,
    Rejected = 2
}

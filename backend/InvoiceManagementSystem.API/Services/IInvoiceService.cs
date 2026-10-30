using InvoiceManagementSystem.API.DTOs.Invoices;
using InvoiceManagementSystem.API.Models;
using InvoiceManagementSystem.API.Models.Enums;

namespace InvoiceManagementSystem.API.Services;

/// <summary>
/// IInvoiceService — Kontrata e shërbimit për menaxhimin e faturave.
/// Përcakton rregullat e biznesit për Admin dhe Customer.
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Merr faturat. Nëse përdoruesi është Customer, kthen VETËM faturat e tij.
    /// Nëse është Admin, kthen të gjitha faturat.
    /// Mbështet kërkimin dhe filtrimin sipas statusit.
    /// </summary>
    Task<IEnumerable<InvoiceResponseDto>> GetInvoicesAsync(
        string currentUserId,
        bool isAdmin,
        string? search = null,
        InvoiceStatus? status = null);

    /// <summary>
    /// Merr detajet e një fature sipas ID-së.
    /// Verifikon pronësinë: Customer mund të shohë vetëm faturën e tij.
    /// </summary>
    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(
        int id,
        string currentUserId,
        bool isAdmin);

    /// <summary>
    /// Merr entitetin e faturës me të dhënat e klientit nga baza e të dhënave për gjenerim PDF-i.
    /// Verifikon autorizimin dhe pronësinë.
    /// </summary>
    Task<Invoice?> GetInvoiceEntityAsync(
        int id,
        string currentUserId,
        bool isAdmin);

    /// <summary>
    /// Krijon një faturë të re (vetëm Admin).
    /// </summary>
    Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto dto);

    /// <summary>
    /// Përditëson një faturë ekzistuese (vetëm Admin).
    /// </summary>
    Task<InvoiceResponseDto> UpdateInvoiceAsync(int id, UpdateInvoiceDto dto);

    /// <summary>
    /// Fshin një faturë vetëm kur është e përshtatshme (p.sh. e papaguar, pa pagesa të konfirmuara).
    /// </summary>
    Task DeleteInvoiceAsync(int id);
}


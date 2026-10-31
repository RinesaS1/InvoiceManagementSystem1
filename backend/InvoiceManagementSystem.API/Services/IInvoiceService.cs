using InvoiceManagementSystem.API.DTOs.Invoices;
using InvoiceManagementSystem.API.Models;
using InvoiceManagementSystem.API.Models.Enums;

namespace InvoiceManagementSystem.API.Services;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceResponseDto>> GetInvoicesAsync(
        string currentUserId,
        bool isAdmin,
        string? search = null,
        InvoiceStatus? status = null);

    Task<InvoiceResponseDto?> GetInvoiceByIdAsync(
        int id,
        string currentUserId,
        bool isAdmin);

    Task<Invoice?> GetInvoiceEntityAsync(
        int id,
        string currentUserId,
        bool isAdmin);

    Task<InvoiceResponseDto> CreateInvoiceAsync(CreateInvoiceDto dto);

    Task<InvoiceResponseDto> UpdateInvoiceAsync(int id, UpdateInvoiceDto dto);

    Task DeleteInvoiceAsync(int id);
}


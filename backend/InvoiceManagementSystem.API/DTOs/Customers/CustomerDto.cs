namespace InvoiceManagementSystem.API.DTOs.Customers;

/// <summary>
/// CustomerDto — Të dhënat e klientit për panelin e administratorit.
/// </summary>
public class CustomerDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int InvoiceCount { get; set; }
}


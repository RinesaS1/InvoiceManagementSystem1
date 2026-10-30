namespace InvoiceManagementSystem.API.Models;

/// <summary>
/// Klasa bazë — çdo model i bazës së të dhënave do të trashëgojë nga kjo klasë.
/// Kjo siguron që çdo entitet ka Id, datën e krijimit dhe datën e modifikimit.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

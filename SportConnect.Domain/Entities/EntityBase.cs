namespace SportConnect.Domain.Entities;

/// <summary>
/// Base abstracta para todas las entidades persistidas.
/// Demuestra herencia y centraliza Id + auditoría temporal.
/// </summary>
public abstract class EntityBase
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

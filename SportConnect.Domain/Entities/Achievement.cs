namespace SportConnect.Domain.Entities;

/// <summary>
/// Catálogo de logros disponibles. Cada Achievement tiene una "rule key"
/// que identifica qué Strategy del lado de Services se encarga de evaluarlo.
/// </summary>
public class Achievement : EntityBase
{
    public string Code { get; set; } = string.Empty;       // ej. "MASTER_CREATOR"
    public string Name { get; set; } = string.Empty;       // ej. "Master Creator"
    public string Description { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;
    public int Threshold { get; set; }                     // ej. 20

    public ICollection<UserAchievement> UnlockedBy { get; set; } = new List<UserAchievement>();
}

public class UserAchievement
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int AchievementId { get; set; }
    public Achievement Achievement { get; set; } = null!;

    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
}

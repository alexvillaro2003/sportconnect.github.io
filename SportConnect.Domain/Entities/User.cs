using SportConnect.Domain.Enums;

namespace SportConnect.Domain.Entities;

public class User : EntityBase
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string? Nationality { get; set; }
    public string? Bio { get; set; }
    public byte[]? ProfilePicture { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    // Navegación
    public ICollection<UserLanguage> Languages { get; set; } = new List<UserLanguage>();
    public ICollection<Activity> CreatedActivities { get; set; } = new List<Activity>();
    public ICollection<ActivityParticipant> Participations { get; set; } = new List<ActivityParticipant>();
    public ICollection<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

using SportConnect.Domain.Enums;

namespace SportConnect.Domain.Entities;

public class Activity : EntityBase
{
    public int CreatorId { get; set; }
    public User Creator { get; set; } = null!;

    public int SportId { get; set; }
    public Sport Sport { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }
    public int MaxPlayers { get; set; }
    public ActivityVisibility Visibility { get; set; } = ActivityVisibility.Public;

    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public ICollection<ActivityParticipant> Participants { get; set; } = new List<ActivityParticipant>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();

    /// <summary>
    /// El chat sigue activo desde la creación hasta medianoche del día de la actividad.
    /// Lógica de dominio pura, sin dependencia de infraestructura.
    /// </summary>
    public bool IsChatActive(DateTime nowUtc)
    {
        var endOfDay = ScheduledAt.Date.AddDays(1);
        return nowUtc < endOfDay;
    }

    public bool IsFull => Participants.Count(p => p.Status == ParticipantStatus.Joined) >= MaxPlayers;
}

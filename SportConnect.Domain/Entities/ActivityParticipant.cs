using SportConnect.Domain.Enums;

namespace SportConnect.Domain.Entities;

public class ActivityParticipant : EntityBase
{
    public int ActivityId { get; set; }
    public Activity Activity { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ParticipantStatus Status { get; set; } = ParticipantStatus.Joined;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

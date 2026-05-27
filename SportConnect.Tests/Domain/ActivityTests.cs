using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using Xunit;

namespace SportConnect.Tests.Domain;

public class ActivityTests
{
    [Fact]
    public void IsChatActive_BeforeMidnightOfActivityDay_ReturnsTrue()
    {
        var activity = new Activity { ScheduledAt = new DateTime(2026, 6, 1, 18, 0, 0, DateTimeKind.Utc) };

        // El día de la actividad, antes de medianoche
        var now = new DateTime(2026, 6, 1, 23, 30, 0, DateTimeKind.Utc);

        Assert.True(activity.IsChatActive(now));
    }

    [Fact]
    public void IsChatActive_AfterMidnightOfActivityDay_ReturnsFalse()
    {
        var activity = new Activity { ScheduledAt = new DateTime(2026, 6, 1, 18, 0, 0, DateTimeKind.Utc) };

        // Día siguiente a las 00:01
        var now = new DateTime(2026, 6, 2, 0, 1, 0, DateTimeKind.Utc);

        Assert.False(activity.IsChatActive(now));
    }

    [Fact]
    public void IsFull_WhenJoinedCountEqualsMaxPlayers_ReturnsTrue()
    {
        var activity = new Activity
        {
            MaxPlayers = 2,
            Participants = new List<ActivityParticipant>
            {
                new() { Status = ParticipantStatus.Joined },
                new() { Status = ParticipantStatus.Joined }
            }
        };

        Assert.True(activity.IsFull);
    }

    [Fact]
    public void IsFull_PendingParticipantsDontCount()
    {
        var activity = new Activity
        {
            MaxPlayers = 2,
            Participants = new List<ActivityParticipant>
            {
                new() { Status = ParticipantStatus.Joined },
                new() { Status = ParticipantStatus.Pending }
            }
        };

        Assert.False(activity.IsFull);
    }
}

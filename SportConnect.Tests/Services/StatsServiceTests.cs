using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Implementations;
using Xunit;

namespace SportConnect.Tests.Services;

public class StatsServiceTests
{
    [Fact]
    public async Task ComputeAsync_NoActivities_ReturnsZeros()
    {
        var db = TestDbFactory.CreateInMemory();
        var service = new StatsService(db);

        var user = new User { Username = "alice", Email = "alice@test.com", PasswordHash = "x" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var stats = await service.ComputeAsync(user.Id);

        Assert.Equal(0, stats.TotalActivitiesCreated);
        Assert.Equal(0, stats.TotalActivitiesJoined);
        Assert.Null(stats.MostPlayedSport);
    }

    [Fact]
    public async Task ComputeAsync_WithCreatedAndJoined_ReturnsCorrectCounts()
    {
        var db = TestDbFactory.CreateInMemory();
        var service = new StatsService(db);

        var creator = new User { Username = "alice", Email = "alice@test.com", PasswordHash = "x" };
        var joiner  = new User { Username = "bob",   Email = "bob@test.com",   PasswordHash = "x" };
        db.Users.AddRange(creator, joiner);

        var sport = new Sport { Name = "Football" };
        db.Sports.Add(sport);
        await db.SaveChangesAsync();

        var activity = new Activity
        {
            CreatorId   = creator.Id,
            SportId     = sport.Id,
            ScheduledAt = DateTime.Now.AddDays(1),
            MaxPlayers  = 10,
            Visibility  = ActivityVisibility.Public,
            Location    = "Park"
        };
        db.Activities.Add(activity);
        await db.SaveChangesAsync();

        db.ActivityParticipants.Add(new ActivityParticipant
        {
            ActivityId = activity.Id,
            UserId     = joiner.Id,
            Status     = ParticipantStatus.Joined
        });
        await db.SaveChangesAsync();

        var creatorStats = await service.ComputeAsync(creator.Id);
        Assert.Equal(1, creatorStats.TotalActivitiesCreated);
        Assert.Equal(0, creatorStats.TotalActivitiesJoined);

        var joinerStats = await service.ComputeAsync(joiner.Id);
        Assert.Equal(0, joinerStats.TotalActivitiesCreated);
        Assert.Equal(1, joinerStats.TotalActivitiesJoined);
        Assert.Equal("Football", joinerStats.MostPlayedSport);
    }

    [Fact]
    public async Task ComputeAsync_MultiSport_MostPlayedSportIsCorrect()
    {
        var db = TestDbFactory.CreateInMemory();
        var service = new StatsService(db);

        var user = new User { Username = "alice", Email = "alice@test.com", PasswordHash = "x" };
        db.Users.Add(user);

        var football = new Sport { Name = "Football" };
        var tennis   = new Sport { Name = "Tennis" };
        db.Sports.AddRange(football, tennis);

        var creator = new User { Username = "creator", Email = "c@test.com", PasswordHash = "x" };
        db.Users.Add(creator);
        await db.SaveChangesAsync();

        Activity MakeActivity(int sportId) => new()
        {
            CreatorId   = creator.Id,
            SportId     = sportId,
            ScheduledAt = DateTime.Now.AddDays(1),
            MaxPlayers  = 10,
            Visibility  = ActivityVisibility.Public,
            Location    = "Park"
        };

        var f1 = MakeActivity(football.Id);
        var f2 = MakeActivity(football.Id);
        var t1 = MakeActivity(tennis.Id);
        db.Activities.AddRange(f1, f2, t1);
        await db.SaveChangesAsync();

        db.ActivityParticipants.AddRange(
            new ActivityParticipant { ActivityId = f1.Id, UserId = user.Id, Status = ParticipantStatus.Joined },
            new ActivityParticipant { ActivityId = f2.Id, UserId = user.Id, Status = ParticipantStatus.Joined },
            new ActivityParticipant { ActivityId = t1.Id, UserId = user.Id, Status = ParticipantStatus.Joined }
        );
        await db.SaveChangesAsync();

        var stats = await service.ComputeAsync(user.Id);

        Assert.Equal(3, stats.TotalActivitiesJoined);
        Assert.Equal("Football", stats.MostPlayedSport);
        Assert.Equal(2, stats.PlaysPerSport["Football"]);
        Assert.Equal(1, stats.PlaysPerSport["Tennis"]);
    }
}

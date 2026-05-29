using SportConnect.Data;
using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Achievements;
using SportConnect.Services.Implementations;
using Xunit;

namespace SportConnect.Tests.Services;

public class AchievementServiceTests
{
    private static async Task<(AppDbContext db, AchievementService service, User user, Achievement firstSteps)>
        SetupAsync()
    {
        var db = TestDbFactory.CreateInMemory();

        var user = new User { Username = "alice", Email = "alice@test.com", PasswordHash = "x" };
        db.Users.Add(user);

        var firstSteps = new Achievement
        {
            Code        = "FIRST_STEPS",
            Name        = "First Steps",
            Description = "Join your first activity",
            IconName    = "star",
            Threshold   = 1
        };
        db.Achievements.Add(firstSteps);

        var sport = new Sport { Name = "Football" };
        db.Sports.Add(sport);
        await db.SaveChangesAsync();

        var activity = new Activity
        {
            CreatorId   = user.Id,
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
            UserId     = user.Id,
            Status     = ParticipantStatus.Joined
        });
        await db.SaveChangesAsync();

        var rules   = new IAchievementRule[] { new FirstStepsRule() };
        var service = new AchievementService(db, rules);

        return (db, service, user, firstSteps);
    }

    [Fact]
    public async Task EvaluateForUser_FirstJoin_UnlocksFirstSteps()
    {
        var (db, service, user, firstSteps) = await SetupAsync();

        var unlocked = await service.EvaluateForUserAsync(user.Id);

        Assert.Single(unlocked);
        Assert.Equal("FIRST_STEPS", unlocked[0].Code);

        var inDb = await service.GetUnlockedAsync(user.Id);
        Assert.Single(inDb);
    }

    [Fact]
    public async Task EvaluateForUser_AlreadyUnlocked_DoesNotDuplicate()
    {
        var (db, service, user, _) = await SetupAsync();

        await service.EvaluateForUserAsync(user.Id);
        await service.EvaluateForUserAsync(user.Id);

        var unlocked = await service.GetUnlockedAsync(user.Id);
        Assert.Single(unlocked);
    }

    [Fact]
    public async Task EvaluateForUser_RuleNotMet_DoesNotUnlock()
    {
        var db = TestDbFactory.CreateInMemory();

        var user = new User { Username = "alice", Email = "alice@test.com", PasswordHash = "x" };
        db.Users.Add(user);
        db.Achievements.Add(new Achievement
        {
            Code = "FIRST_STEPS", Name = "First Steps",
            Description = "Join your first activity", IconName = "star", Threshold = 1
        });
        await db.SaveChangesAsync();

        var rules   = new IAchievementRule[] { new FirstStepsRule() };
        var service = new AchievementService(db, rules);

        var unlocked = await service.EvaluateForUserAsync(user.Id);

        Assert.Empty(unlocked);
        Assert.Empty(await service.GetUnlockedAsync(user.Id));
    }
}

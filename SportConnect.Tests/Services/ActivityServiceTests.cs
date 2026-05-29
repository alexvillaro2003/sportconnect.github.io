using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Implementations;
using Xunit;

namespace SportConnect.Tests.Services;

public class ActivityServiceTests
{
    // ── helpers ────────────────────────────────────────────────────────────────

    private static async Task<(ActivityService service, User creator, Activity activity)>
        SetupAsync()
    {
        var db = TestDbFactory.CreateInMemory();
        var service = new ActivityService(db);

        var creator = new User
        {
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = "hash",
            Role = UserRole.User
        };
        db.Users.Add(creator);
        await db.SaveChangesAsync();

        var sport = new Sport { Name = "Football" };
        db.Sports.Add(sport);
        await db.SaveChangesAsync();

        var activity = await service.CreateAsync(
            creator.Id,
            sport.Id,
            DateTime.Now.AddDays(1),
            4,
            ActivityVisibility.Public,
            "Park",
            null);

        return (service, creator, activity);
    }

    // ── UpdateAsync tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ByCreator_UpdatesFields()
    {
        var (service, creator, activity) = await SetupAsync();

        await service.UpdateAsync(
            activity.Id, creator.Id,
            activity.SportId, DateTime.Now.AddDays(2),
            6, ActivityVisibility.RequestBased, "Stadium", "New desc");

        var updated = await service.GetByIdAsync(activity.Id);
        Assert.NotNull(updated);
        Assert.Equal(6, updated!.MaxPlayers);
        Assert.Equal("Stadium", updated.Location);
        Assert.Equal("New desc", updated.Description);
        Assert.Equal(ActivityVisibility.RequestBased, updated.Visibility);
    }

    [Fact]
    public async Task UpdateAsync_ByOtherUser_ThrowsUnauthorized()
    {
        var (service, _, activity) = await SetupAsync();
        var db = TestDbFactory.CreateInMemory();

        var otherUser = new User
        {
            Username = "bob",
            Email = "bob@example.com",
            PasswordHash = "hash",
            Role = UserRole.User
        };
        // We need the other user in the same db as the activity
        // Rebuild everything in one db to share context
        var db2 = TestDbFactory.CreateInMemory();
        var svc2 = new ActivityService(db2);

        var creator2 = new User { Username = "alice2", Email = "a2@example.com", PasswordHash = "h", Role = UserRole.User };
        var other2 = new User { Username = "bob2", Email = "b2@example.com", PasswordHash = "h", Role = UserRole.User };
        db2.Users.AddRange(creator2, other2);
        var sport2 = new Sport { Name = "Tennis" };
        db2.Sports.Add(sport2);
        await db2.SaveChangesAsync();

        var act2 = await svc2.CreateAsync(creator2.Id, sport2.Id, DateTime.Now.AddDays(1), 4, ActivityVisibility.Public, "Court", null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc2.UpdateAsync(act2.Id, other2.Id, sport2.Id, DateTime.Now.AddDays(2), 4, ActivityVisibility.Public, "Court", null));
    }

    [Fact]
    public async Task UpdateAsync_ByAdmin_Succeeds()
    {
        var db = TestDbFactory.CreateInMemory();
        var service = new ActivityService(db);

        var creator = new User { Username = "alice", Email = "alice@ex.com", PasswordHash = "h", Role = UserRole.User };
        var admin = new User { Username = "admin", Email = "admin@ex.com", PasswordHash = "h", Role = UserRole.Admin };
        db.Users.AddRange(creator, admin);
        var sport = new Sport { Name = "Basketball" };
        db.Sports.Add(sport);
        await db.SaveChangesAsync();

        var activity = await service.CreateAsync(creator.Id, sport.Id, DateTime.Now.AddDays(1), 4, ActivityVisibility.Public, "Gym", null);

        await service.UpdateAsync(activity.Id, admin.Id, sport.Id, DateTime.Now.AddDays(3), 8, ActivityVisibility.Public, "Arena", "Admin edit");

        var updated = await service.GetByIdAsync(activity.Id);
        Assert.Equal("Arena", updated!.Location);
        Assert.Equal(8, updated.MaxPlayers);
    }

    [Fact]
    public async Task UpdateAsync_WithMaxPlayersBelowCurrentParticipants_Throws()
    {
        var db = TestDbFactory.CreateInMemory();
        var service = new ActivityService(db);

        var creator = new User { Username = "alice", Email = "alice@ex.com", PasswordHash = "h", Role = UserRole.User };
        var second = new User { Username = "bob", Email = "bob@ex.com", PasswordHash = "h", Role = UserRole.User };
        db.Users.AddRange(creator, second);
        var sport = new Sport { Name = "Volleyball" };
        db.Sports.Add(sport);
        await db.SaveChangesAsync();

        var activity = await service.CreateAsync(creator.Id, sport.Id, DateTime.Now.AddDays(1), 4, ActivityVisibility.Public, "Beach", null);
        await service.JoinAsync(activity.Id, second.Id);

        // 2 participants joined (creator + second); trying to set maxPlayers to 1 should throw
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateAsync(activity.Id, creator.Id, sport.Id, DateTime.Now.AddDays(2), 1, ActivityVisibility.Public, "Beach", null));
    }

    [Fact]
    public async Task UpdateAsync_WithPastDate_Throws()
    {
        var (service, creator, activity) = await SetupAsync();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.UpdateAsync(activity.Id, creator.Id, activity.SportId, DateTime.Now.AddDays(-1), 4, ActivityVisibility.Public, "Park", null));
    }
}

using Microsoft.EntityFrameworkCore;
using SportConnect.Data;
using SportConnect.Domain.Enums;

namespace SportConnect.Services.Achievements;

public sealed class MasterCreatorRule : IAchievementRule
{
    public string AchievementCode => "MASTER_CREATOR";

    public async Task<bool> IsUnlockedAsync(int userId, AppDbContext db)
    {
        var count = await db.Activities.CountAsync(a => a.CreatorId == userId);
        return count >= 20;
    }
}

public sealed class DedicatedPlayerRule : IAchievementRule
{
    public string AchievementCode => "DEDICATED_PLAYER";

    public async Task<bool> IsUnlockedAsync(int userId, AppDbContext db)
    {
        var groups = await db.ActivityParticipants
            .Where(p => p.UserId == userId && p.Status == ParticipantStatus.Joined)
            .GroupBy(p => p.Activity.SportId)
            .Select(g => g.Count())
            .ToListAsync();

        return groups.Count > 0 && groups.Max() >= 20;
    }
}

public sealed class FirstStepsRule : IAchievementRule
{
    public string AchievementCode => "FIRST_STEPS";

    public Task<bool> IsUnlockedAsync(int userId, AppDbContext db) =>
        db.ActivityParticipants.AnyAsync(p =>
            p.UserId == userId && p.Status == ParticipantStatus.Joined);
}

public sealed class SocialButterflyRule : IAchievementRule
{
    public string AchievementCode => "SOCIAL_BUTTERFLY";

    public async Task<bool> IsUnlockedAsync(int userId, AppDbContext db)
    {
        var count = await db.ActivityParticipants
            .CountAsync(p => p.UserId == userId && p.Status == ParticipantStatus.Joined);
        return count >= 10;
    }
}

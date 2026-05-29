using Microsoft.EntityFrameworkCore;
using SportConnect.Data;
using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Interfaces;

namespace SportConnect.Services.Implementations;

public class ActivityService : IActivityService
{
    private readonly AppDbContext _db;

    public ActivityService(AppDbContext db) => _db = db;

    public async Task<Activity> CreateAsync(
        int creatorId, int sportId, DateTime scheduledAt, int maxPlayers,
        ActivityVisibility visibility, string location, string? description)
    {
        if (maxPlayers < 2) throw new ArgumentException("MaxPlayers must be at least 2.");
        if (scheduledAt < DateTime.Now) throw new ArgumentException("Activity must be scheduled in the future.");
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location is required.");

        var activity = new Activity
        {
            CreatorId = creatorId,
            SportId = sportId,
            ScheduledAt = scheduledAt,
            MaxPlayers = maxPlayers,
            Visibility = visibility,
            Location = location.Trim(),
            Description = description?.Trim()
        };

        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();

        // El creador queda automáticamente unido
        _db.ActivityParticipants.Add(new ActivityParticipant
        {
            ActivityId = activity.Id,
            UserId = creatorId,
            Status = ParticipantStatus.Joined
        });
        await _db.SaveChangesAsync();

        return activity;
    }

    public async Task<IReadOnlyList<Sport>> GetAllSportsAsync() =>
        await _db.Sports.OrderBy(s => s.Name).ToListAsync();

    public async Task<IReadOnlyList<Activity>> GetFeedAsync(int? sportId = null)
    {
        var query = _db.Activities
            .AsNoTracking()
            .Include(a => a.Creator)
            .Include(a => a.Sport)
            .Include(a => a.Participants)
            .Where(a => a.ScheduledAt >= DateTime.Now);

        if (sportId.HasValue) query = query.Where(a => a.SportId == sportId.Value);

        return await query.OrderBy(a => a.ScheduledAt).ToListAsync();
    }

    public Task<Activity?> GetByIdAsync(int id) =>
        _db.Activities
           .AsNoTracking()
           .Include(a => a.Creator)
           .Include(a => a.Sport)
           .Include(a => a.Participants).ThenInclude(p => p.User)
           .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IReadOnlyList<Activity>> GetUserActivitiesAsync(int userId) =>
        await _db.Activities
            .AsNoTracking()
            .Include(a => a.Sport)
            .Include(a => a.Creator)
            .Include(a => a.Participants)
            .Where(a => a.Participants.Any(p => p.UserId == userId && p.Status == ParticipantStatus.Joined))
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync();

    public async Task<bool> JoinAsync(int activityId, int userId)
    {
        var activity = await _db.Activities
            .Include(a => a.Participants)
            .FirstOrDefaultAsync(a => a.Id == activityId)
            ?? throw new InvalidOperationException("Activity not found.");

        if (activity.IsFull) return false;
        if (activity.Participants.Any(p => p.UserId == userId && p.Status == ParticipantStatus.Joined)) return false;

        var status = activity.Visibility == ActivityVisibility.Public
            ? ParticipantStatus.Joined
            : ParticipantStatus.Pending;

        _db.ActivityParticipants.Add(new ActivityParticipant
        {
            ActivityId = activityId,
            UserId = userId,
            Status = status
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task LeaveAsync(int activityId, int userId)
    {
        var participant = await _db.ActivityParticipants
            .FirstOrDefaultAsync(p => p.ActivityId == activityId && p.UserId == userId);
        if (participant is null) return;

        _db.ActivityParticipants.Remove(participant);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        int activityId, int requestingUserId,
        int sportId, DateTime scheduledAt, int maxPlayers,
        ActivityVisibility visibility, string location, string? description)
    {
        var activity = await _db.Activities
            .Include(a => a.Participants)
            .FirstOrDefaultAsync(a => a.Id == activityId)
            ?? throw new InvalidOperationException("Activity not found.");

        var requester = await _db.Users.FindAsync(requestingUserId)
            ?? throw new InvalidOperationException("User not found.");

        if (activity.CreatorId != requestingUserId && requester.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("Only the creator or an admin can edit this activity.");

        if (maxPlayers < 2) throw new ArgumentException("MaxPlayers must be at least 2.");
        if (scheduledAt < DateTime.Now) throw new ArgumentException("Activity must be scheduled in the future.");
        if (string.IsNullOrWhiteSpace(location)) throw new ArgumentException("Location is required.");

        var joinedCount = activity.Participants.Count(p => p.Status == ParticipantStatus.Joined);
        if (maxPlayers < joinedCount)
            throw new ArgumentException($"MaxPlayers cannot be less than the current number of participants ({joinedCount}).");

        activity.SportId = sportId;
        activity.ScheduledAt = scheduledAt;
        activity.MaxPlayers = maxPlayers;
        activity.Visibility = visibility;
        activity.Location = location.Trim();
        activity.Description = description?.Trim();

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int activityId, int requestingUserId)
    {
        var activity = await _db.Activities.FindAsync(activityId)
            ?? throw new InvalidOperationException("Activity not found.");

        var requester = await _db.Users.FindAsync(requestingUserId)
            ?? throw new InvalidOperationException("User not found.");

        if (activity.CreatorId != requestingUserId && requester.Role != UserRole.Admin)
            throw new UnauthorizedAccessException("Only the creator or an admin can delete this activity.");

        _db.Activities.Remove(activity);
        await _db.SaveChangesAsync();
    }
}

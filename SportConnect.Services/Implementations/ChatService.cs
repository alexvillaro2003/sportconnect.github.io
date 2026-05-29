using Microsoft.EntityFrameworkCore;
using SportConnect.Data;
using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Interfaces;

namespace SportConnect.Services.Implementations;

public class ChatService : IChatService
{
    private readonly AppDbContext _db;

    public ChatService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Message>> GetMessagesAsync(int activityId)
    {
        return await _db.Messages
            .Include(m => m.User)
            .Where(m => m.ActivityId == activityId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<Message> SendMessageAsync(int activityId, int userId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message cannot be empty.", nameof(content));

        if (!await IsChatActiveAsync(activityId))
            throw new InvalidOperationException("Chat is no longer active for this activity.");

        // Solo participantes pueden escribir
        var isParticipant = await _db.ActivityParticipants.AnyAsync(p =>
            p.ActivityId == activityId && p.UserId == userId && p.Status == ParticipantStatus.Joined);

        if (!isParticipant)
            throw new UnauthorizedAccessException("Only joined participants can send messages.");

        var message = new Message
        {
            ActivityId = activityId,
            UserId = userId,
            Content = content.Trim(),
            SentAt = DateTime.Now
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        // Recargar con User para que el VM lo muestre directamente
        await _db.Entry(message).Reference(m => m.User).LoadAsync();
        return message;
    }

    public async Task<bool> IsChatActiveAsync(int activityId)
    {
        var activity = await _db.Activities.FindAsync(activityId);
        return activity is not null && activity.IsChatActive(DateTime.UtcNow);
    }
}

public class StatsService : IStatsService
{
    private readonly AppDbContext _db;

    public StatsService(AppDbContext db) => _db = db;

    public async Task<UserStats> ComputeAsync(int userId)
    {
        var created = await _db.Activities.CountAsync(a => a.CreatorId == userId);

        var joined = await _db.ActivityParticipants
            .Include(p => p.Activity).ThenInclude(a => a.Sport)
            .Where(p => p.UserId == userId && p.Status == ParticipantStatus.Joined)
            .Select(p => p.Activity.Sport.Name)
            .ToListAsync();

        var perSport = joined
            .GroupBy(s => s)
            .ToDictionary(g => g.Key, g => g.Count());

        var mostPlayed = perSport
            .OrderByDescending(kv => kv.Value)
            .Select(kv => kv.Key)
            .FirstOrDefault();

        return new UserStats(
            TotalActivitiesCreated: created,
            TotalActivitiesJoined: joined.Count,
            MostPlayedSport: mostPlayed,
            PlaysPerSport: perSport
        );
    }
}

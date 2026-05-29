using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;

namespace SportConnect.Services.Interfaces;

public interface IActivityService
{
    Task<IReadOnlyList<Sport>> GetAllSportsAsync();

    Task<Activity> CreateAsync(
        int creatorId,
        int sportId,
        DateTime scheduledAt,
        int maxPlayers,
        ActivityVisibility visibility,
        string location,
        string? description);

    Task<IReadOnlyList<Activity>> GetFeedAsync(int? sportId = null);
    Task<Activity?> GetByIdAsync(int id);
    Task<IReadOnlyList<Activity>> GetUserActivitiesAsync(int userId);

    Task UpdateAsync(
        int activityId,
        int requestingUserId,
        int sportId,
        DateTime scheduledAt,
        int maxPlayers,
        ActivityVisibility visibility,
        string location,
        string? description);

    Task<bool> JoinAsync(int activityId, int userId);
    Task LeaveAsync(int activityId, int userId);
    Task DeleteAsync(int activityId, int requestingUserId);
}

using SportConnect.Domain.Entities;

namespace SportConnect.Services.Interfaces;

public interface IChatService
{
    Task<IReadOnlyList<Message>> GetMessagesAsync(int activityId);
    Task<Message> SendMessageAsync(int activityId, int userId, string content);
    Task<bool> IsChatActiveAsync(int activityId);
}

public interface IAchievementService
{
    /// <summary>
    /// Reevalúa todos los achievements del usuario tras un evento (creación, unirse, etc.).
    /// Inserta los recién desbloqueados en UserAchievements.
    /// </summary>
    Task<IReadOnlyList<Achievement>> EvaluateForUserAsync(int userId);

    Task<IReadOnlyList<Achievement>> GetUnlockedAsync(int userId);
    Task<IReadOnlyList<Achievement>> GetAllAsync();
}

public interface IStatsService
{
    Task<UserStats> ComputeAsync(int userId);
}

public record UserStats(
    int TotalActivitiesCreated,
    int TotalActivitiesJoined,
    string? MostPlayedSport,
    IReadOnlyDictionary<string, int> PlaysPerSport
);

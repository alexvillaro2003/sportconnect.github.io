using Microsoft.EntityFrameworkCore;
using SportConnect.Data;
using SportConnect.Domain.Entities;
using SportConnect.Services.Achievements;
using SportConnect.Services.Interfaces;

namespace SportConnect.Services.Implementations;

/// <summary>
/// Orquesta las reglas (Strategy pattern). Para añadir un nuevo achievement:
/// 1. Insertar fila en el catálogo (ver DatabaseInitializer).
/// 2. Crear una clase que implemente IAchievementRule.
/// 3. Registrarla en el contenedor DI. El servicio la recogerá automáticamente.
/// </summary>
public class AchievementService : IAchievementService
{
    private readonly AppDbContext _db;
    private readonly IEnumerable<IAchievementRule> _rules;

    public AchievementService(AppDbContext db, IEnumerable<IAchievementRule> rules)
    {
        _db = db;
        _rules = rules;
    }

    public async Task<IReadOnlyList<Achievement>> EvaluateForUserAsync(int userId)
    {
        var newlyUnlocked = new List<Achievement>();

        var alreadyUnlockedCodes = await _db.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.Achievement.Code)
            .ToListAsync();

        foreach (var rule in _rules)
        {
            if (alreadyUnlockedCodes.Contains(rule.AchievementCode)) continue;

            if (!await rule.IsUnlockedAsync(userId, _db)) continue;

            var achievement = await _db.Achievements
                .FirstOrDefaultAsync(a => a.Code == rule.AchievementCode);
            if (achievement is null) continue;

            _db.UserAchievements.Add(new UserAchievement
            {
                UserId = userId,
                AchievementId = achievement.Id,
                UnlockedAt = DateTime.UtcNow
            });
            newlyUnlocked.Add(achievement);
        }

        if (newlyUnlocked.Count > 0)
            await _db.SaveChangesAsync();

        return newlyUnlocked;
    }

    public async Task<IReadOnlyList<Achievement>> GetUnlockedAsync(int userId) =>
        await _db.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Include(ua => ua.Achievement)
            .Select(ua => ua.Achievement)
            .ToListAsync();

    public async Task<IReadOnlyList<Achievement>> GetAllAsync() =>
        await _db.Achievements.OrderBy(a => a.Name).ToListAsync();
}

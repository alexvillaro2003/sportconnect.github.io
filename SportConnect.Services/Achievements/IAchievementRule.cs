using SportConnect.Data;

namespace SportConnect.Services.Achievements;

/// <summary>
/// Estrategia de evaluación de un achievement concreto.
/// Cada implementación corresponde 1-a-1 con un Achievement.Code del catálogo.
/// </summary>
public interface IAchievementRule
{
    /// <summary>Code que enlaza la regla con la fila correspondiente en Achievements.</summary>
    string AchievementCode { get; }

    /// <summary>True si el usuario cumple la condición ahora mismo.</summary>
    Task<bool> IsUnlockedAsync(int userId, AppDbContext db);
}

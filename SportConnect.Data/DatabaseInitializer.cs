using Microsoft.EntityFrameworkCore;
using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;

namespace SportConnect.Data;

/// <summary>
/// Inicializa la BD: crea el esquema (EnsureCreated) y siembra catálogos
/// estáticos (deportes, idiomas, achievements) si están vacíos.
/// </summary>
public static class DatabaseInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        SeedSports(context);
        SeedLanguages(context);
        SeedAchievements(context);
        SeedAdminUser(context);

        context.SaveChanges();
    }

    private static void SeedSports(AppDbContext context)
    {
        if (context.Sports.Any()) return;

        context.Sports.AddRange(
            new Sport { Name = "Football", IsOutdoor = true, IconName = "football" },
            new Sport { Name = "Basketball", IsOutdoor = false, IconName = "basketball" },
            new Sport { Name = "Tennis", IsOutdoor = true, IconName = "tennis" },
            new Sport { Name = "Padel", IsOutdoor = true, IconName = "padel" },
            new Sport { Name = "Volleyball", IsOutdoor = false, IconName = "volleyball" },
            new Sport { Name = "Running", IsOutdoor = true, IconName = "running" },
            new Sport { Name = "Cycling", IsOutdoor = true, IconName = "cycling" },
            new Sport { Name = "Swimming", IsOutdoor = false, IconName = "swimming" },
            new Sport { Name = "Yoga", IsOutdoor = false, IconName = "yoga" },
            new Sport { Name = "Climbing", IsOutdoor = false, IconName = "climbing" }
        );
    }

    private static void SeedLanguages(AppDbContext context)
    {
        if (context.Languages.Any()) return;

        context.Languages.AddRange(
            new Language { Name = "English",    Code = "en" },
            new Language { Name = "Spanish",    Code = "es" },
            new Language { Name = "Dutch",      Code = "nl" },
            new Language { Name = "French",     Code = "fr" },
            new Language { Name = "German",     Code = "de" },
            new Language { Name = "Italian",    Code = "it" },
            new Language { Name = "Portuguese", Code = "pt" }
        );
    }

    private static void SeedAdminUser(AppDbContext context)
    {
        if (context.Users.Any(u => u.Role == UserRole.Admin)) return;

        context.Users.Add(new User
        {
            Username     = "admin",
            Email        = "admin@sportconnect.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 12),
            Role         = UserRole.Admin
        });
    }

    private static void SeedAchievements(AppDbContext context)
    {
        if (context.Achievements.Any()) return;

        context.Achievements.AddRange(
            new Achievement
            {
                Code = "MASTER_CREATOR",
                Name = "Master Creator",
                Description = "Create 20 or more activities.",
                IconName = "trophy",
                Threshold = 20
            },
            new Achievement
            {
                Code = "DEDICATED_PLAYER",
                Name = "Dedicated Player",
                Description = "Play the same sport 20 or more times.",
                IconName = "medal",
                Threshold = 20
            },
            new Achievement
            {
                Code = "FIRST_STEPS",
                Name = "First Steps",
                Description = "Join your first activity.",
                IconName = "star",
                Threshold = 1
            },
            new Achievement
            {
                Code = "SOCIAL_BUTTERFLY",
                Name = "Social Butterfly",
                Description = "Join 10 different activities.",
                IconName = "butterfly",
                Threshold = 10
            }
        );
    }
}

using Microsoft.EntityFrameworkCore;
using SportConnect.Data;
using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Interfaces;

namespace SportConnect.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(int id) =>
        _db.Users
           .Include(u => u.Languages).ThenInclude(ul => ul.Language)
           .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IReadOnlyList<User>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<User>();

        var q = query.Trim().ToLower();
        return await _db.Users
            .Where(u => u.Username.ToLower().Contains(q) || u.Email.ToLower().Contains(q))
            .OrderBy(u => u.Username)
            .Take(50)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<User>> GetAllAsync() =>
        await _db.Users.OrderBy(u => u.Username).ToListAsync();

    public async Task UpdateProfileAsync(int userId, string? bio, string? nationality, byte[]? profilePicture)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        user.Bio = bio;
        user.Nationality = nationality;
        if (profilePicture is not null) user.ProfilePicture = profilePicture;

        await _db.SaveChangesAsync();
    }

    public async Task UpdateRoleAsync(int userId, UserRole newRole)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");
        user.Role = newRole;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }

    public async Task SetLanguagesAsync(int userId, IEnumerable<int> languageIds)
    {
        var user = await _db.Users
            .Include(u => u.Languages)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found.");

        user.Languages.Clear();
        foreach (var langId in languageIds.Distinct())
        {
            user.Languages.Add(new UserLanguage { UserId = userId, LanguageId = langId });
        }

        await _db.SaveChangesAsync();
    }
}

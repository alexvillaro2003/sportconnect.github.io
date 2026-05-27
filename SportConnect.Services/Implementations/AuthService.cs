using Microsoft.EntityFrameworkCore;
using SportConnect.Data;
using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Interfaces;

namespace SportConnect.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;

    public AuthService(AppDbContext db) => _db = db;

    public async Task<User> RegisterAsync(string username, string email, string plainPassword)
    {
        ValidateRegistration(username, email, plainPassword);

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedUsername = username.Trim();

        var exists = await _db.Users.AnyAsync(u =>
            u.Username == normalizedUsername || u.Email == normalizedEmail);

        if (exists)
            throw new InvalidOperationException("Username or email is already in use.");

        var user = new User
        {
            Username = normalizedUsername,
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12),
            Role = UserRole.User
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<User?> LoginAsync(string usernameOrEmail, string plainPassword)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(plainPassword))
            return null;

        var input = usernameOrEmail.Trim();
        var inputLower = input.ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Username == input || u.Email == inputLower);

        if (user is null) return null;

        var valid = BCrypt.Net.BCrypt.Verify(plainPassword, user.PasswordHash);
        return valid ? user : null;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            throw new ArgumentException("New password must be at least 8 characters.", nameof(newPassword));

        var user = await _db.Users.FindAsync(userId);
        if (user is null) return false;

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
        await _db.SaveChangesAsync();
        return true;
    }

    private static void ValidateRegistration(string username, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters.", nameof(username));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("A valid email is required.", nameof(email));

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters.", nameof(password));
    }
}

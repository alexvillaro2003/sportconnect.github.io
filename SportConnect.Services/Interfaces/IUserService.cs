using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;

namespace SportConnect.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetByIdAsync(int id);
    Task<IReadOnlyList<User>> SearchAsync(string query);
    Task<IReadOnlyList<User>> GetAllAsync();
    Task UpdateProfileAsync(int userId, string? bio, string? nationality, byte[]? profilePicture);
    Task UpdateRoleAsync(int userId, UserRole newRole);
    Task DeleteAccountAsync(int userId);
    Task SetLanguagesAsync(int userId, IEnumerable<int> languageIds);
}

using SportConnect.Domain.Entities;

namespace SportConnect.Services.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Registra un nuevo usuario. Devuelve el usuario creado o lanza si email/username ya existen.
    /// </summary>
    Task<User> RegisterAsync(string username, string email, string plainPassword);

    /// <summary>
    /// Verifica credenciales. Devuelve el usuario si son correctas, null en caso contrario.
    /// </summary>
    Task<User?> LoginAsync(string usernameOrEmail, string plainPassword);

    /// <summary>
    /// Cambia la contraseña verificando la antigua. Devuelve true si tuvo éxito.
    /// </summary>
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
}

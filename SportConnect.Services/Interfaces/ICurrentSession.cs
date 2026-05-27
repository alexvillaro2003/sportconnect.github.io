using SportConnect.Domain.Entities;

namespace SportConnect.Services.Interfaces;

/// <summary>
/// Lleva la cuenta del usuario actualmente autenticado.
/// Singleton en el contenedor DI; los ViewModels lo inyectan.
/// </summary>
public interface ICurrentSession
{
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }

    void SetUser(User user);
    void Clear();
}

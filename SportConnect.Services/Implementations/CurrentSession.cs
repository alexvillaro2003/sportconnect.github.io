using SportConnect.Domain.Entities;
using SportConnect.Domain.Enums;
using SportConnect.Services.Interfaces;

namespace SportConnect.Services.Implementations;

public class CurrentSession : ICurrentSession
{
    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    public void SetUser(User user) => CurrentUser = user;
    public void Clear() => CurrentUser = null;
}

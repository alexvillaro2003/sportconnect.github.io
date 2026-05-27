using SportConnect.Services.Implementations;
using Xunit;

namespace SportConnect.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_WithValidInput_CreatesUserWithHashedPassword()
    {
        using var db = TestDbFactory.CreateInMemory();
        var service = new AuthService(db);

        var user = await service.RegisterAsync("alice", "alice@example.com", "supersecret123");

        Assert.NotEqual(0, user.Id);
        Assert.Equal("alice", user.Username);
        Assert.Equal("alice@example.com", user.Email);
        Assert.NotEqual("supersecret123", user.PasswordHash); // está hasheada
        Assert.True(BCrypt.Net.BCrypt.Verify("supersecret123", user.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_Throws()
    {
        using var db = TestDbFactory.CreateInMemory();
        var service = new AuthService(db);

        await service.RegisterAsync("alice", "alice@example.com", "supersecret123");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RegisterAsync("bob", "alice@example.com", "anotherpass456"));
    }

    [Theory]
    [InlineData("ab", "a@b.com", "validpassword")]      // username demasiado corto
    [InlineData("alice", "not-an-email", "validpassword")] // email inválido
    [InlineData("alice", "a@b.com", "short")]            // password demasiado corta
    public async Task RegisterAsync_WithInvalidInput_Throws(string username, string email, string password)
    {
        using var db = TestDbFactory.CreateInMemory();
        var service = new AuthService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.RegisterAsync(username, email, password));
    }

    [Fact]
    public async Task LoginAsync_WithCorrectCredentials_ReturnsUser()
    {
        using var db = TestDbFactory.CreateInMemory();
        var service = new AuthService(db);
        await service.RegisterAsync("alice", "alice@example.com", "supersecret123");

        var byUsername = await service.LoginAsync("alice", "supersecret123");
        var byEmail    = await service.LoginAsync("alice@example.com", "supersecret123");

        Assert.NotNull(byUsername);
        Assert.NotNull(byEmail);
        Assert.Equal("alice", byUsername!.Username);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull()
    {
        using var db = TestDbFactory.CreateInMemory();
        var service = new AuthService(db);
        await service.RegisterAsync("alice", "alice@example.com", "supersecret123");

        var result = await service.LoginAsync("alice", "wrongpassword");
        Assert.Null(result);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidOldPassword_UpdatesHash()
    {
        using var db = TestDbFactory.CreateInMemory();
        var service = new AuthService(db);
        var user = await service.RegisterAsync("alice", "alice@example.com", "supersecret123");

        var ok = await service.ChangePasswordAsync(user.Id, "supersecret123", "brandnewpass456");

        Assert.True(ok);
        var login = await service.LoginAsync("alice", "brandnewpass456");
        Assert.NotNull(login);
    }
}

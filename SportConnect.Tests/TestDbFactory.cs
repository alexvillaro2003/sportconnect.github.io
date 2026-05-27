using Microsoft.EntityFrameworkCore;
using SportConnect.Data;

namespace SportConnect.Tests;

/// <summary>
/// Helper para crear un AppDbContext con BD en memoria, aislado por test.
/// </summary>
internal static class TestDbFactory
{
    public static AppDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}

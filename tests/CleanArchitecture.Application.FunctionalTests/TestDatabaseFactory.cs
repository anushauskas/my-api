using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Application.FunctionalTests;

public static class TestDatabaseFactory
{
    public static async Task<ITestDatabase> CreateAsync(IConfiguration configuration)
    {
        // Testcontainers requires Docker. To use a local SQL Server database instead,
        // switch to `SqlTestDatabase` and update appsettings.json.
        var database = new SqlTestDatabase();

        await database.InitialiseAsync(configuration);

        return database;
    }
}

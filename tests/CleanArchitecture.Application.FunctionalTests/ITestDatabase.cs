using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Application.FunctionalTests;

public interface ITestDatabase : IAsyncDisposable
{
    Task InitialiseAsync(IConfiguration configuration);

    DbConnection GetConnection();

    Task ResetAsync();
}

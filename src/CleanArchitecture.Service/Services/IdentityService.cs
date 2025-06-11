using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Service.Services;
internal class IdentityService : IIdentityService
{
    public Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        return Task.FromResult(true);
    }

    public Task<string?> GetUserNameAsync(string userId)
    {
        return Task.FromResult<string?>("Username");
    }

    public Task<bool> IsInRoleAsync(string userId, string role)
    {
        return Task.FromResult(true);
    }
}

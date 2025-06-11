using System.Security.Claims;
using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace CleanArchitecture.Web.Services;

public class IdentityService : IIdentityService
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        IHttpContextAccessor httpContext,
        IAuthorizationService authorizationService)
    {
        _httpContext = httpContext;
        _authorizationService = authorizationService;
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = _httpContext.HttpContext?.User;
        if (user == null)
        {
            return false;
        }

        var result = await _authorizationService.AuthorizeAsync(user, policyName);

        return result.Succeeded;
    }

    public Task<string?> GetUserNameAsync(string userId)
    {
        return Task.FromResult(_httpContext.HttpContext?.User.Identity?.Name);
    }

    public Task<bool> IsInRoleAsync(string userId, string role)
    {
        var identity = _httpContext.HttpContext?.User.Identity as ClaimsIdentity;
        if (identity == null)
        {
            return Task.FromResult(false);
        }

        var isInRole = identity.Claims.Where(c => c.Type == identity.RoleClaimType).Any(c => c.Value == role);

        return Task.FromResult(isInRole);
    }
}

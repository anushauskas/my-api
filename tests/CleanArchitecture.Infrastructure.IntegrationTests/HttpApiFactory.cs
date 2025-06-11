using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using CleanArchitecture.Infrastructure.Configuration.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CleanArchitecture.Infrastructure.IntegrationTests;
internal class HttpApiFactory : WebApplicationFactory<Program>
{
    private bool _addAuthenticationHeader = false;
    private TokenManagementOptions _tokenManagementOptions = new();
    private string[] _scopes = Array.Empty<string>();
    private string _symmetricKey;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        _tokenManagementOptions = configuration.GetSection("TokenManagement").Get<TokenManagementOptions>()
            ?? new TokenManagementOptions { };

        builder
            .UseConfiguration(configuration);
    }

    protected override void ConfigureClient(HttpClient client)
    {
        if (_addAuthenticationHeader)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Testing", GetToken());
        }

        base.ConfigureClient(client);
    }

    public HttpApiFactory WithAuthenticationHeader()
    {
        _addAuthenticationHeader = true;
        return this;
    }

    public HttpApiFactory WithScopes(params string[] scopes)
    {
        _scopes = scopes;
        return this;
    }

    public HttpApiFactory WithSymmetricKey(string symmetricKey)
    {
        _symmetricKey = symmetricKey;
        return this;
    }

    private string GetToken()
    {
        var bytes = Convert.FromBase64String(ConsumeGivenSymmetricKey() ?? _tokenManagementOptions.SymmetricKey);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(bytes), SecurityAlgorithms.HmacSha256);

        var jwtHeader = new JwtHeader(credentials);

        var claims = ConsumeGivenClaims();

        var jwtPayload = new JwtPayload(
            _tokenManagementOptions.Issuer,
            _tokenManagementOptions.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow
        );

        var token = new JwtSecurityToken(
            jwtHeader,
            jwtPayload
        );

        var handler = new JwtSecurityTokenHandler();

        return handler.WriteToken(token);
    }

    private IEnumerable<Claim> ConsumeGivenClaims()
    {
        var ret = _scopes.Select(s => new Claim("scopes", s));
        _scopes = Array.Empty<string>();
        return ret.Union(new[] { new Claim(ClaimTypes.NameIdentifier, "Authenticated Client") });
    }

    private string ConsumeGivenSymmetricKey()
    {
        var ret = _symmetricKey;
        _symmetricKey = null;
        return ret;
    }

}

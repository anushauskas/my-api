using CleanArchitecture.Infrastructure.Interfaces;

namespace CleanArchitecture.Infrastructure.Configuration.Options;
internal class OAuth2TokenClientOptions : IOptionsSection
{
    public string SectionName => "OAuth2TokenClient";

    public string OAuth2TokenUrl { get; init; } = string.Empty;
    public string OAuth2ClientId { get; init; } = string.Empty;
    public string OAuth2ClientSecret { get; init; } = string.Empty;

    public bool Validate(string? sectionParentPath = null)
    {
        return !string.IsNullOrWhiteSpace(OAuth2TokenUrl);
    }
}

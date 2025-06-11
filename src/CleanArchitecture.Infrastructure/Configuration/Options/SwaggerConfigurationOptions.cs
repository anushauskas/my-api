using CleanArchitecture.Infrastructure.Interfaces;

namespace CleanArchitecture.Infrastructure.Configuration.Options;
public class SwaggerConfigurationOptions : IOptionsSection
{
    public string SectionName => "Swagger";

    public bool Enabled { get; set; }
    public bool UiEnabled { get; set; }
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = string.Empty;
    public string OAuthClientId { get; set; } = string.Empty;
    public string AuthorizationUrl { get; set; } = string.Empty;

    public bool Validate(string? sectionParentPath = null)
    {
        return true;
    }
}

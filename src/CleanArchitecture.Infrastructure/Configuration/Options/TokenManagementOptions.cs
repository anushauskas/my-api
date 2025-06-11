using CleanArchitecture.Infrastructure.Interfaces;

namespace CleanArchitecture.Infrastructure.Configuration.Options;
public class TokenManagementOptions : IOptionsSection
{
    public string SectionName => "TokenManagement";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SymmetricKey { get; init; } = string.Empty;
    public string Authority { get; init; } = string.Empty;

    public bool Validate(string? sectionParentPath = null)
    {
        return true;
    }
}

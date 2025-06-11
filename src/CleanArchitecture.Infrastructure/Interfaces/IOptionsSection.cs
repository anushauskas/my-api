namespace CleanArchitecture.Infrastructure.Interfaces;
public interface IOptionsSection
{
    string SectionName { get; }
    bool Validate(string? sectionParentPath = null);
}

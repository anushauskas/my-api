namespace CleanArchitecture.Infrastructure.Configuration.Options;
internal class TodoItemsApiClientOptions : IHttpClientOptionsSection
{
    public string SectionName => "TodoItemsApiClient";

    public string BaseUrl { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; }

    public bool Validate(string? sectionParentPath = null)
    {
        return !string.IsNullOrWhiteSpace(BaseUrl) && Timeout != TimeSpan.Zero;
    }
}

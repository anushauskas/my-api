using CleanArchitecture.Infrastructure.Interfaces;

namespace CleanArchitecture.Infrastructure.Configuration;
internal interface IHttpClientOptionsSection : IOptionsSection
{
    string BaseUrl { get; set; }
    TimeSpan Timeout { get; set; }
}

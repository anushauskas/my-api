using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.HttpHandlers;
internal class HttpErrorLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpErrorLoggingHandler> _logger;

    public HttpErrorLoggingHandler(ILogger<HttpErrorLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Error {@StatusCode} on HTTP request {@Method} {@RequestUri} {@Response}",
                    response.StatusCode, request.Method.Method, request.RequestUri, responseString
                );
            }

            return response;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                "Error on HTTP request {@Method} {@RequestUri}",
                request.Method.Method, request.RequestUri
            );

            throw;
        }
    }
}

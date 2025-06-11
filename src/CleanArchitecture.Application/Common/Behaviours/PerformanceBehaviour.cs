using System.Diagnostics;
using CleanArchitecture.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Common.Behaviours;

public class PerformanceBehaviour<TRequest> : IPipelineBehavior<TRequest> where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly IInstrumentationService _instrumentationService;
    private readonly TagList _tagList;
    private readonly string _requestName;

    public PerformanceBehaviour(
        ILogger<TRequest> logger,
        IUser user,
        IIdentityService identityService,
        IInstrumentationService instrumentationService)
    {
        _timer = new Stopwatch();

        _logger = logger;
        _user = user;
        _identityService = identityService;
        _instrumentationService = instrumentationService;
        _requestName = typeof(TRequest).Name;
        _tagList = new TagList()
        {
            {
                "request",
                _requestName
            }
        };
    }

    public async Task Handle(TRequest request, Func<Task> next, CancellationToken cancellationToken)
    {
        using var activity = _instrumentationService.ActivitySource.StartActivity(_requestName);
        activity?.AddTag("request", request);
        _timer.Start();
        try
        {
            await next();
        }
        catch (Exception ex) when (activity != null)
        {
            // https://github.com/open-telemetry/semantic-conventions/blob/main/docs/exceptions/exceptions-spans.md
            activity?.AddEvent(new ActivityEvent("exception", default,
                new ActivityTagsCollection(new TagList()
                {
                    { "exception.escaped", true },
                    { "exception.message", ex.Message },
                    { "exception.stacktrace", ex.StackTrace },
                    { "exception.type", ex.GetType().FullName }
                })));
            activity?.SetTag("otel.status_code", "ERROR");
            activity?.SetTag("otel.status_description", ex.Message);
            throw;
        }
        finally
        {
            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            _instrumentationService.RecordRequestDuration(elapsedMilliseconds, _tagList);

            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;
                var userId = _user.Id ?? string.Empty;
                var userName = string.Empty;

                if (!string.IsNullOrEmpty(userId))
                {
                    userName = await _identityService.GetUserNameAsync(userId);
                }

                _logger.LogWarning("CleanArchitecture Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                    requestName, elapsedMilliseconds, userId, userName, request);
            }
        }
    }
}

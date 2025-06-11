namespace CleanArchitecture.Infrastructure.Telemetry;
internal class OpenTelemetryOptions
{
    public Uri Endpoint { get; init; } = new Uri("http://localhost:4317");
    public string ServiceName { get; init; } = "CleanArchitecture";
    public string ServiceNamespace { get; init; } = "ap.core";
    public TimeSpan MetricsPushInterval { get; init; }
}

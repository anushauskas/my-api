using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Infrastructure.Telemetry;
internal class InstrumentationService : IInstrumentationService, IDisposable
{
    internal const string MeterName = "ApplicationMetrics";
    internal const string RequestDurationHistogramName = "application.request.duration";
    internal const string ActivitySourceName = "CleanArchitecture";

    private readonly Meter _meter;
    private readonly Histogram<double> _requestDurationHistogram;


    public InstrumentationService()
    {
        _meter = new Meter(MeterName);
        // please follow naming conventions
        // https://opentelemetry.io/docs/specs/otel/metrics/semantic_conventions/
        // https://prometheus.io/docs/practices/naming/
        _requestDurationHistogram = _meter.CreateHistogram<double>(RequestDurationHistogramName, unit: "s", description: "Request duration");

        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        ActivitySource = new ActivitySource(ActivitySourceName, version);
    }

    public ActivitySource ActivitySource { get; }

    public void Dispose()
    {
        ActivitySource.Dispose();
        _meter.Dispose();
    }

    public void RecordRequestDuration(double miliseconds, TagList tagList)
    {
        _requestDurationHistogram.Record(miliseconds / 1000, tagList);
    }
}

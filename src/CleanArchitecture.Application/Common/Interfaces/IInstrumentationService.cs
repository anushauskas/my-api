using System.Diagnostics;

namespace CleanArchitecture.Application.Common.Interfaces;
public interface IInstrumentationService
{
    void RecordRequestDuration(double miliseconds, TagList tagList = new TagList());
    public ActivitySource ActivitySource { get; }
}

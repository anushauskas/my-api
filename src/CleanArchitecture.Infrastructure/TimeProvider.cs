namespace CleanArchitecture.Infrastructure;
public class TimeProvider
{
    internal DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
}

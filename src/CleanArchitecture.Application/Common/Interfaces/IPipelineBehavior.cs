namespace CleanArchitecture.Application.Common.Interfaces
{
    public interface IPipelineBehavior<TRequest> 
        where TRequest : notnull
    {
        Task Handle(TRequest request, Func<Task> next, CancellationToken cancellationToken);
    }
}

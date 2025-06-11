namespace CleanArchitecture.Application.Common.Interfaces;

public interface IRequestHandler<TRequest>
{
    Task Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IRequest<TResponse> 
{
}

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : notnull
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

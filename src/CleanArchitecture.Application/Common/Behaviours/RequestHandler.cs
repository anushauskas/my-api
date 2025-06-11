using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Application.Common.Behaviours
{
    public class RequestHandler<TRequest, TResponse> where TRequest : notnull
    {
        private readonly IEnumerable<IPipelineBehavior<TRequest>> _behaviors;
        private readonly IRequestHandler<TRequest, TResponse> _handler;

        public RequestHandler(
            IEnumerable<IPipelineBehavior<TRequest>> behaviors,
            IRequestHandler<TRequest, TResponse> handler)
        {
            _behaviors = behaviors;
            _handler = handler;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            TResponse response = default!;
            Func<TRequest, CancellationToken, Task> fun = async (req, ct) => response = await _handler.Handle(req, ct);

            var handlerChain = _behaviors.Reverse().Aggregate(
                fun,
                (next, behavior) => (req, ct) => behavior.Handle(req, () => next(req, ct), ct));

            await handlerChain(request, cancellationToken);
            return response;
        }
    }

    public class RequestHandler<TRequest> where TRequest : notnull
    {
        private readonly IEnumerable<IPipelineBehavior<TRequest>> _behaviors;
        private readonly IRequestHandler<TRequest> _handler;

        public RequestHandler(
            IEnumerable<IPipelineBehavior<TRequest>> behaviors,
            IRequestHandler<TRequest> handler)
        {
            _behaviors = behaviors;
            _handler = handler;
        }

        public Task Handle(TRequest request, CancellationToken cancellationToken)
        {
            var handlerChain = _behaviors.Reverse().Aggregate(
                _handler.Handle,
                (next, behavior) => (req, ct) => behavior.Handle(req, () => next(req, ct), ct));

            return handlerChain(request, cancellationToken);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using static MediatorLite.INotificationHandle;

namespace MediatorLite
{
    namespace MediatorLight
    {
        public class Mediator : IMediator
        {
            private readonly IServiceProvider _serviceProvider;

            public Mediator(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            // ------------------------------
            //  MÉTODO SEND → Para Requests
            // ------------------------------
            public async Task<TResponse> Send<TResponse>(
                IRequest<TResponse> request,
                CancellationToken cancellationToken = default)
            {
                // Descobre o tipo do handler correto
                var handlerType = typeof(IRequestHandler<,>)
                    .MakeGenericType(request.GetType(), typeof(TResponse));

                // Resolve o handler do container de DI
                dynamic? handler = _serviceProvider.GetService(handlerType);

                if (handler is null)
                    throw new InvalidOperationException(
                        $"Handler não encontrado para o tipo {request.GetType().Name}");

                // Executa o método Handle() do handler encontrado
                return await handler.Handle((dynamic)request, cancellationToken);
            }

            // -----------------------------------
            //  MÉTODO PUBLISH → Para Notifications
            // -----------------------------------
            public async Task Publish(
                INotification notification,
                CancellationToken cancellationToken = default)
            {
                // Descobre o tipo base do handler de notificação
                var handlerType = typeof(INotificationHandler<>)
                    .MakeGenericType(notification.GetType());

                // Resolve todos os handlers para essa notificação
                var handlers = _serviceProvider.GetServices(handlerType);

                if (handlers is null || !handlers.Any())
                    return; // nenhum handler para essa notificação

                // Executa todos os handlers da notificação
                foreach (dynamic handler in handlers)
                {
                    await handler.Handle((dynamic)notification, cancellationToken);
                }
            }
        }
    }
}

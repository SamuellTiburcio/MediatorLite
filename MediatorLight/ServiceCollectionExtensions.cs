using System.Reflection;
using MediatorLite.MediatorLight;
using Microsoft.Extensions.DependencyInjection;
using static MediatorLite.INotificationHandle;

namespace MediatorLite
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registra o Mediator e todos os handlers encontrados nos assemblies especificados.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="assemblies">Assemblies onde procurar handlers</param>
        public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                // Se não passou assemblies, pega o assembly que chamou este método
                assemblies = [Assembly.GetCallingAssembly()];
            }

            // Registrar Mediator
            services.AddSingleton<IMediator, Mediator>();

            // Registrar todos os handlers
            foreach (var assembly in assemblies)
            {
                RegisterHandlers(services, assembly);
            }

            return services;
        }

        private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var interfaces = type.GetInterfaces();

                foreach (var @interface in interfaces)
                {
                    if (!@interface.IsGenericType) continue;

                    var genericDef = @interface.GetGenericTypeDefinition();

                    if (genericDef == typeof(IRequestHandler<,>) || genericDef == typeof(INotificationHandler<>))
                    {
                        services.AddTransient(@interface, type);
                    }
                    else if (genericDef == typeof(IPipelineBehavior<,>))
                    {
                        // Pipelines (pré/pós-processamento)
                        services.AddTransient(@interface, type);
                    }
                }
            }
        }
    }

    // Pipeline interface opcional
    public interface IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        TResponse Handle(TRequest request, Func<TRequest, TResponse> next);
    }
}
using Cleo.Core.Domain.Ports;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Cleo.Infrastructure.Messaging;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Cleo.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all infrastructure services required by the Cleo system.
    /// </summary>
    public static IServiceCollection AddCleoInfrastructure(this IServiceCollection services, Uri julesBaseUrl)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(julesBaseUrl);

        // Security & Persistence
        services.AddSingleton<IVault, NativeVault>();
        services.AddSingleton<IFileSystem, PhysicalFileSystem>();
        services.AddSingleton<ISessionReader, RegistrySessionReader>();
        services.AddSingleton<ISessionWriter, RegistrySessionWriter>();
        
        // Messaging
        services.AddSingleton<IDispatcher, MediatRDispatcher>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        // Jules Client & Mappers
        services.AddSingleton<ISessionStatusMapper, DefaultSessionStatusMapper>();
        services.AddSingleton<IJulesActivityMapper, PlanningActivityMapper>();
        services.AddSingleton<IJulesActivityMapper, ResultActivityMapper>();
        services.AddSingleton<IJulesActivityMapper, ProgressActivityMapper>();
        services.AddSingleton<IJulesActivityMapper, FailureActivityMapper>();
        services.AddSingleton<IJulesActivityMapper, MessageActivityMapper>();
        services.AddSingleton<IJulesActivityMapper, UnknownActivityMapper>();

        services.AddTransient<JulesAuthHandler>();
        services.AddTransient<JulesLoggingHandler>();

        void ConfigureJulesClient(HttpClient client)
        {
            client.BaseAddress = julesBaseUrl;
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        services.AddHttpClient<IJulesSessionClient, RestJulesSessionClient>(ConfigureJulesClient)
            .AddHttpMessageHandler<JulesAuthHandler>()
            .AddHttpMessageHandler<JulesLoggingHandler>();

        services.AddHttpClient<IJulesSourceClient, RestJulesSourceClient>(ConfigureJulesClient)
            .AddHttpMessageHandler<JulesAuthHandler>()
            .AddHttpMessageHandler<JulesLoggingHandler>();

        services.AddHttpClient<IJulesActivityClient, RestJulesActivityClient>(ConfigureJulesClient)
            .AddHttpMessageHandler<JulesAuthHandler>()
            .AddHttpMessageHandler<JulesLoggingHandler>();

        return services;
    }
}

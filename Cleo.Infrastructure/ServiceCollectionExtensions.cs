using Cleo.Core.Domain.Ports;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Cleo.Infrastructure.Common; // New Namespace
using Cleo.Infrastructure.Messaging;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis; // Added
using System.Runtime.InteropServices;

namespace Cleo.Infrastructure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all infrastructure services required by the Cleo system.
    /// </summary>
    [SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Platform is checked via IPlatformProvider.")]
    public static IServiceCollection AddCleoInfrastructure(this IServiceCollection services, Uri julesBaseUrl, IPlatformProvider? platformProvider = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(julesBaseUrl);

        // 1. Determine Platform 🌍
        platformProvider ??= new DefaultPlatformProvider();

        // 2. Security & Persistence
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var identityPath = Path.Combine(appData, "Cleo", "identity.dat");

        if (platformProvider.IsWindows())
        {
            services.AddSingleton<IEncryptionStrategy, DpapiEncryptionStrategy>();
        }
        else
        {
            services.AddSingleton<IEncryptionStrategy, AesGcmEncryptionStrategy>();
        }

        services.AddSingleton<IVault>(sp => new NativeVault(identityPath, sp.GetRequiredService<IEncryptionStrategy>()));
        
        services.AddSingleton<IFileSystem, PhysicalFileSystem>();
        services.AddSingleton<IRegistryPathProvider, DefaultRegistryPathProvider>();
        services.AddSingleton<IRegistryTaskMapper, RegistryTaskMapper>();
        services.AddSingleton<IRegistrySerializer, JsonRegistrySerializer>();
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

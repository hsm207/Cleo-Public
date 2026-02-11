using Cleo.Core.Domain.Ports;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Cleo.Infrastructure.Common;
using Cleo.Infrastructure.Messaging;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Infrastructure.Persistence.Mappers;
using Cleo.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
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
        services.AddSingleton<ICredentialStore>(sp => (NativeVault)sp.GetRequiredService<IVault>());
        
        services.AddSingleton<IFileSystem, PhysicalFileSystem>();
        services.AddSingleton<IRegistryPathProvider, DefaultRegistryPathProvider>();
        services.AddSingleton<IRegistryTaskMapper, RegistryTaskMapper>();
        services.AddSingleton<IRegistrySerializer, JsonRegistrySerializer>();
        services.AddSingleton<ISessionReader, RegistrySessionReader>();
        services.AddSingleton<ISessionWriter, RegistrySessionWriter>();

        // High-Fidelity Activity Persistence Plugins (South Boundary) 🔌💎
        services.AddSingleton<ArtifactMapperFactory>();
        services.AddSingleton<IArtifactPersistenceMapper, BashOutputMapper>();
        services.AddSingleton<IArtifactPersistenceMapper, ChangeSetMapper>();
        services.AddSingleton<IArtifactPersistenceMapper, MediaMapper>();

        services.AddSingleton<ActivityMapperFactory>();
        services.AddSingleton<IActivityPersistenceMapper, Persistence.Mappers.PlanningActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, Persistence.Mappers.MessageActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, Persistence.Mappers.ApprovalActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, Persistence.Mappers.ProgressActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, Persistence.Mappers.CompletionActivityMapper>();
        services.AddSingleton<IActivityPersistenceMapper, Persistence.Mappers.FailureActivityMapper>();
        
        // Messaging
        services.AddSingleton<IDispatcher, MediatRDispatcher>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        // Jules Client & Mappers (North Boundary)
        services.AddSingleton<ISessionStatusMapper, DefaultSessionStatusMapper>();
        services.AddSingleton<Clients.Jules.Mapping.IJulesActivityMapper, Clients.Jules.Mapping.PlanningActivityMapper>();
        services.AddSingleton<Clients.Jules.Mapping.IJulesActivityMapper, Clients.Jules.Mapping.ApprovalActivityMapper>();
        services.AddSingleton<Clients.Jules.Mapping.IJulesActivityMapper, Clients.Jules.Mapping.ProgressActivityMapper>();
        services.AddSingleton<Clients.Jules.Mapping.IJulesActivityMapper, Clients.Jules.Mapping.CompletionActivityMapper>();
        services.AddSingleton<Clients.Jules.Mapping.IJulesActivityMapper, Clients.Jules.Mapping.FailureActivityMapper>();
        services.AddSingleton<Clients.Jules.Mapping.IJulesActivityMapper, Clients.Jules.Mapping.MessageActivityMapper>();
        services.AddSingleton<Clients.Jules.Mapping.IJulesActivityMapper, Clients.Jules.Mapping.UnknownActivityMapper>();

        services.AddTransient<JulesAuthHandler>();
        services.AddTransient<JulesLoggingHandler>();

        void ConfigureJulesClient(HttpClient client)
        {
            client.BaseAddress = julesBaseUrl;
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        // Jules Clients (Specialized & SRP-Compliant)
        services.AddHttpClient<IJulesSessionClient, RestSessionLifecycleClient>(ConfigureJulesClient)
            .AddHttpMessageHandler<JulesAuthHandler>()
            .AddHttpMessageHandler<JulesLoggingHandler>();

        services.AddHttpClient<ISessionMessenger, RestSessionMessenger>(ConfigureJulesClient)
            .AddHttpMessageHandler<JulesAuthHandler>()
            .AddHttpMessageHandler<JulesLoggingHandler>();

        services.AddHttpClient<IPulseMonitor, RestPulseMonitor>(ConfigureJulesClient)
            .AddHttpMessageHandler<JulesAuthHandler>()
            .AddHttpMessageHandler<JulesLoggingHandler>();

        services.AddHttpClient<ISessionController, RestSessionController>(ConfigureJulesClient)
            .AddHttpMessageHandler<JulesAuthHandler>()
            .AddHttpMessageHandler<JulesLoggingHandler>();

        services.AddHttpClient<IJulesSourceClient, RestJulesSourceClient>(ConfigureJulesClient)
            .AddHttpMessageHandler<JulesAuthHandler>()
            .AddHttpMessageHandler<JulesLoggingHandler>();

        services.AddScoped<ISourceCatalog>(sp => (RestJulesSourceClient)sp.GetRequiredService<IJulesSourceClient>());

        services.AddHttpClient<IJulesActivityClient, RestJulesActivityClient>(ConfigureJulesClient)
            .AddHttpMessageHandler<JulesAuthHandler>()
            .AddHttpMessageHandler<JulesLoggingHandler>();

        services.AddScoped<ISessionArchivist>(sp => (RestJulesActivityClient)sp.GetRequiredService<IJulesActivityClient>());

        // Use Cases
        services.AddScoped<Cleo.Core.UseCases.InitiateSession.InitiateSessionUseCase>();
        services.AddScoped<Cleo.Core.UseCases.RefreshPulse.IRefreshPulseUseCase, Cleo.Core.UseCases.RefreshPulse.RefreshPulseUseCase>();
        services.AddScoped<Cleo.Core.UseCases.BrowseHistory.IBrowseHistoryUseCase, Cleo.Core.UseCases.BrowseHistory.BrowseHistoryUseCase>();
        services.AddScoped<Cleo.Core.UseCases.ApprovePlan.IApprovePlanUseCase, Cleo.Core.UseCases.ApprovePlan.ApprovePlanUseCase>();
        services.AddScoped<Cleo.Core.UseCases.AuthenticateUser.IAuthenticateUserUseCase, Cleo.Core.UseCases.AuthenticateUser.AuthenticateUserUseCase>();

        return services;
    }
}

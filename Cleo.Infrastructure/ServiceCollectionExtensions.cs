using Cleo.Core.Domain.Ports;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Cleo.Infrastructure.Common;
using Cleo.Infrastructure.Messaging;
using Cleo.Infrastructure.Persistence;
using Cleo.Infrastructure.Persistence.Internal;
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
        
        // Messaging
        services.AddSingleton<IDispatcher, MediatRDispatcher>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        // Jules Client & Mappers
        services.AddSingleton<ISessionStatusMapper, DefaultSessionStatusMapper>();
        services.AddSingleton<IJulesActivityMapper, PlanningActivityMapper>();
        services.AddSingleton<IJulesActivityMapper, ResultActivityMapper>();
        services.AddSingleton<IJulesActivityMapper, ExecutionActivityMapper>();
        services.AddSingleton<IJulesActivityMapper, ProgressActivityMapper>();
        services.AddSingleton<IJulesActivityMapper, CompletionActivityMapper>();
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

        // Jules Clients
        services.AddHttpClient<IJulesSessionClient, RestJulesSessionClient>(ConfigureJulesClient)
            .AddHttpMessageHandler<JulesAuthHandler>()
            .AddHttpMessageHandler<JulesLoggingHandler>();

        services.AddScoped<ISessionMessenger>(sp => (RestJulesSessionClient)sp.GetRequiredService<IJulesSessionClient>());
        services.AddScoped<IPulseMonitor>(sp => (RestJulesSessionClient)sp.GetRequiredService<IJulesSessionClient>());

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

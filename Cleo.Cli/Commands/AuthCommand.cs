using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.AuthenticateUser;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class AuthCommand
{
    private readonly IAuthenticateUserUseCase _authenticateUseCase;
    private readonly IVault _vault;
    private readonly ILogger<AuthCommand> _logger;

    public AuthCommand(IAuthenticateUserUseCase authenticateUseCase, IVault vault, ILogger<AuthCommand> logger)
    {
        _authenticateUseCase = authenticateUseCase;
        _vault = vault;
        _logger = logger;
    }

    public Command Build()
    {
        var authCommand = new Command("auth", "Manage your Identity in the Vault 🔐");

        var loginCommand = new Command("login", "Authenticate with your Jules API Key 🔑");
        var keyArgument = new Argument<string>("key", "Your Jules API Key");
        loginCommand.AddArgument(keyArgument);
        loginCommand.SetHandler(async (key) => await ExecuteLoginAsync(key), keyArgument);

        var logoutCommand = new Command("logout", "Clear local identity and credentials 🗑️");
        logoutCommand.SetHandler(async () => await ExecuteLogoutAsync());

        authCommand.AddCommand(loginCommand);
        authCommand.AddCommand(logoutCommand);

        return authCommand;
    }

    private async Task ExecuteLoginAsync(string key)
    {
        try
        {
            var request = new AuthenticateUserRequest(key);
            var response = await _authenticateUseCase.ExecuteAsync(request).ConfigureAwait(false);

            if (response.Success)
            {
                Console.WriteLine($"✅ {response.Message}");
            }
            else
            {
                Console.WriteLine($"❌ Error: {response.Message}");
            }
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to login.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }

    private async Task ExecuteLogoutAsync()
    {
        await _vault.ClearAsync(CancellationToken.None).ConfigureAwait(false);
        Console.WriteLine("🗑️ Credentials cleared. See you later! 👋🥀");
    }
}

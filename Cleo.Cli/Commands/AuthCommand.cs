using System.CommandLine;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.AuthenticateUser;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal sealed class AuthCommand
{
    private readonly IAuthenticateUserUseCase _useCase;
    private readonly ICredentialStore _credentialStore;
    private readonly ILogger<AuthCommand> _logger;

    public AuthCommand(IAuthenticateUserUseCase useCase, ICredentialStore credentialStore, ILogger<AuthCommand> logger)
    {
        _useCase = useCase;
        _credentialStore = credentialStore;
        _logger = logger;
    }

    public Command Build()
    {
        var authCommand = new Command("auth", "Manage your Jules authentication 🔐");

        var loginCommand = new Command("login", "Login to Jules with your API key.");
        loginCommand.SetHandler(async () => await ExecuteLoginAsync());
        authCommand.AddCommand(loginCommand);
        
        var logoutCommand = new Command("logout", "Clear your stored credentials.");
        logoutCommand.SetHandler(async () => await ExecuteLogoutAsync());
        authCommand.AddCommand(logoutCommand);

        return authCommand;
    }

    private async Task ExecuteLoginAsync()
    {
        Console.WriteLine("🔐 Login to Jules");
        Console.WriteLine("Please get your API key from the Jules console.");
        Console.Write("Enter your API Key: ");
        
        var apiKey = ReadMaskedLine();
        
        try
        {
            var request = new AuthenticateUserRequest(apiKey);
            var response = await _useCase.ExecuteAsync(request).ConfigureAwait(false);
            
            if (response.Success)
            {
                Console.WriteLine($"✅ {response.Message}");
            }
            else
            {
                Console.WriteLine($"❌ {response.Message}");
            }
        }
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to store API key.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Something went wrong: {ex.Message}");
        }
        #pragma warning restore CA1031
    }

    private async Task ExecuteLogoutAsync()
    {
        await _credentialStore.ClearIdentityAsync().ConfigureAwait(false);
        Console.WriteLine("🗑️ Credentials cleared. See you later! 👋🥀");
    }

    private static string ReadMaskedLine()
    {
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine() ?? string.Empty;
        }

        var password = string.Empty;
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
        }
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }
}

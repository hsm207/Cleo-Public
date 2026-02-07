using System.CommandLine;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal static class AuthCommand
{
    public static Command Create(IServiceProvider serviceProvider)
    {
        var authCommand = new Command("auth", "Manage your Jules authentication 🔐");

        var loginCommand = new Command("login", "Login to Jules with your API key.");
        
        loginCommand.SetHandler(async () =>
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("AuthCommand");
            var vault = serviceProvider.GetRequiredService<IVault>();

            Console.WriteLine("🔐 Login to Jules");
            Console.WriteLine("Please get your API key from the Jules console.");
            Console.Write("Enter your API Key: ");
            
            // Mask the input (simple version)
            var apiKey = ReadMaskedLine();
            
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("❌ API Key cannot be empty, babe.");
                return;
            }

            try
            {
                var identity = new Identity(new ApiKey(apiKey));
                await vault.StoreAsync(identity).ConfigureAwait(false);
                
                Console.WriteLine("✅ API Key saved securely in your vault! We're ready to go! 🚀💖");
            }
            #pragma warning disable CA1031
            catch (Exception ex)
            {
                #pragma warning disable CA1848
                logger.LogError(ex, "❌ Failed to store API key.");
                #pragma warning restore CA1848
                Console.WriteLine($"💔 Something went wrong: {ex.Message}");
            }
            #pragma warning restore CA1031
        });

        authCommand.AddCommand(loginCommand);
        
        var logoutCommand = new Command("logout", "Clear your stored credentials.");
        logoutCommand.SetHandler(async () => 
        {
            var vault = serviceProvider.GetRequiredService<IVault>();
            await vault.ClearAsync().ConfigureAwait(false);
            Console.WriteLine("🗑️ Credentials cleared. See you later! 👋🥀");
        });
        
        authCommand.AddCommand(logoutCommand);

        return authCommand;
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

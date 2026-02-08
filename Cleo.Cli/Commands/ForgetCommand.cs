using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ForgetSession;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class ForgetCommand
{
    private readonly IForgetSessionUseCase _useCase;
    private readonly ILogger<ForgetCommand> _logger;

    public ForgetCommand(IForgetSessionUseCase useCase, ILogger<ForgetCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("forget", "Remove a session from the local registry 🧹");

        var handleArgument = new Argument<string>("handle", "The session handle (ID) to forget.");
        command.AddArgument(handleArgument);

        command.SetHandler(async (handle) => await ExecuteAsync(handle), handleArgument);

        return command;
    }

    private async Task ExecuteAsync(string handle)
    {
        try
        {
            var sessionId = new SessionId(handle);
            var request = new ForgetSessionRequest(sessionId);
            await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            Console.WriteLine($"🧹 Session {handle} removed from registry.");
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "Failed to forget session.");
            #pragma warning restore CA1848
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

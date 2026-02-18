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
        var command = new Command("forget", "Forget a session from the local Session Registry 🧹");

        var sessionIdArgument = new Argument<string>("sessionId", "The session ID (e.g., sessions/123).");
        command.AddArgument(sessionIdArgument);

        command.SetHandler(async (sessionId) => await ExecuteAsync(sessionId), sessionIdArgument);

        return command;
    }

    private async Task ExecuteAsync(string sessionId)
    {
        try
        {
            var id = new SessionId(sessionId);
            var request = new ForgetSessionRequest(id);
            await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            Console.WriteLine($"🧹 Session {sessionId} removed from registry.");
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

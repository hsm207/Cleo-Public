using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.Correspond;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class TalkCommand
{
    private static readonly string[] MessageAliases = { "--message", "-m" };

    private readonly ICorrespondUseCase _useCase;
    private readonly ILogger<TalkCommand> _logger;

    public TalkCommand(ICorrespondUseCase useCase, ILogger<TalkCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("talk", "Send a message to Jules 💬");

        var sessionIdArgument = new Argument<string>("sessionId", "The session ID (e.g., sessions/123).");
        command.AddArgument(sessionIdArgument);

        var messageOption = new Option<string>(MessageAliases, "The message or guidance to send.")
        {
            IsRequired = true
        };
        command.AddOption(messageOption);

        command.SetHandler(async (sessionId, message) => await ExecuteAsync(sessionId, message), sessionIdArgument, messageOption);

        return command;
    }

    private async Task ExecuteAsync(string sessionId, string message)
    {
        try
        {
            var request = new CorrespondRequest(new SessionId(sessionId), message);
            await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            Console.WriteLine($"✅ Message sent! Jules is thinking... 🤔");
        }
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to send message.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Error: {ex.Message}");
        }
    }
}

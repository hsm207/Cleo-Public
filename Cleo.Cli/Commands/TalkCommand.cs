using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.Correspond;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class TalkCommand
{
    private readonly ICorrespondUseCase _useCase;
    private readonly ILogger<TalkCommand> _logger;

    private static readonly string[] MessageAliases = { "--message", "-m", "--prompt", "-p" };

    public TalkCommand(ICorrespondUseCase useCase, ILogger<TalkCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("talk", "Send a message or prompt to Jules 💬");

        var handleArgument = new Argument<string>("handle", "The session handle (ID).");
        var messageOption = new Option<string>(MessageAliases, "The message or prompt to send.") { IsRequired = true };

        command.AddArgument(handleArgument);
        command.AddOption(messageOption);

        command.SetHandler(async (handle, message) => await ExecuteAsync(handle, message), handleArgument, messageOption);

        return command;
    }

    private async Task ExecuteAsync(string handle, string message)
    {
        try
        {
            var sessionId = new SessionId(handle);
            
            Console.WriteLine($"💬 Sending message to {handle}...");
            var request = new CorrespondRequest(sessionId, message);
            await _useCase.ExecuteAsync(request);

            Console.WriteLine($"✅ Message sent! Jules is thinking... 🤔");
        }
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to send message.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Something went wrong: {ex.Message}");
        }
        #pragma warning restore CA1031
    }
}

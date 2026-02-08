using System.CommandLine;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.AbandonSession;
using Microsoft.Extensions.Logging;

namespace Cleo.Cli.Commands;

internal sealed class DeleteCommand
{
    private readonly IAbandonSessionUseCase _useCase;
    private readonly ILogger<DeleteCommand> _logger;

    public DeleteCommand(IAbandonSessionUseCase useCase, ILogger<DeleteCommand> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    public Command Build()
    {
        var command = new Command("delete", "Remove a task from the local registry 🗑️");

        var handleArgument = new Argument<string>("handle", "The session handle (ID) to remove.");
        command.AddArgument(handleArgument);

        command.SetHandler(async (handle) => await ExecuteAsync(handle), handleArgument);

        return command;
    }

    private async Task ExecuteAsync(string handle)
    {
        try
        {
            var sessionId = new SessionId(handle);
            var request = new AbandonSessionRequest(sessionId);
            await _useCase.ExecuteAsync(request).ConfigureAwait(false);

            Console.WriteLine($"🗑️ Session {handle} removed from registry. Goodbye, sweet prince! 🥀");
        }
        #pragma warning disable CA1031
        catch (Exception ex)
        {
            #pragma warning disable CA1848
            _logger.LogError(ex, "❌ Failed to delete session.");
            #pragma warning restore CA1848
            Console.WriteLine($"💔 Something went wrong: {ex.Message}");
        }
        #pragma warning restore CA1031
    }
}

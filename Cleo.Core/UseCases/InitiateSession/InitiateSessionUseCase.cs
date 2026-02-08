using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.InitiateSession;

public record InitiateSessionRequest(
    string TaskDescription,
    string RepoContext,
    string StartingBranch,
    string? UserProvidedTitle = null
);

public record InitiateSessionResponse(
    SessionId Id,
    Uri? DashboardUri,
    bool IsPrAutomated
);

/// <summary>
/// The orchestrator for launching a new engineering mission.
/// Implements session initiation policies.
/// </summary>
public class InitiateSessionUseCase : IUseCase<InitiateSessionRequest, InitiateSessionResponse>
{
    private readonly IJulesSessionClient _julesClient;
    private readonly ISessionWriter _sessionWriter;

    public InitiateSessionUseCase(IJulesSessionClient julesClient, ISessionWriter sessionWriter)
    {
        _julesClient = julesClient ?? throw new ArgumentNullException(nameof(julesClient));
        _sessionWriter = sessionWriter ?? throw new ArgumentNullException(nameof(sessionWriter));
    }

    public async Task<InitiateSessionResponse> ExecuteAsync(InitiateSessionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // 1. Apply Business Policy: Every session must result in a PR and require explicit developer approval.
        var mode = AutomationMode.AutoCreatePullRequest;
        const bool RequireApproval = true;

        // 2. Apply Business Policy: Title Generation / Fallback
        var title = request.UserProvidedTitle ?? TruncateTaskToTitle(request.TaskDescription);

        var options = new SessionCreationOptions(mode, title, RequireApproval);

        // 3. Coordinate with Infrastructure via Ports
        var taskDescription = new TaskDescription(request.TaskDescription);
        var sourceContext = new SourceContext(request.RepoContext, request.StartingBranch);

        var session = await _julesClient.CreateSessionAsync(
            taskDescription, 
            sourceContext, 
            options, 
            cancellationToken).ConfigureAwait(false);

        // 4. Persistence (Task Registry)
        await _sessionWriter.SaveAsync(session, cancellationToken).ConfigureAwait(false);

        // 5. Return the Response Model
        return new InitiateSessionResponse(
            session.Id,
            session.DashboardUri,
            options.Mode == AutomationMode.AutoCreatePullRequest
        );
    }

    private static string TruncateTaskToTitle(string task)
    {
        const int MaxLength = 50;
        if (task.Length <= MaxLength) return task;
        return task[..MaxLength].Trim() + "...";
    }
}

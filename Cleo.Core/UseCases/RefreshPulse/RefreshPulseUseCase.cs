using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.Services;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.RefreshPulse;

public class RefreshPulseUseCase : IRefreshPulseUseCase
{
    private readonly IPulseMonitor _pulseMonitor;
    private readonly IJulesActivityClient _activityClient;
    private readonly ISessionReader _sessionReader;
    private readonly ISessionWriter _sessionWriter;
    private readonly IPrResolver _prResolver;

    public RefreshPulseUseCase(
        IPulseMonitor pulseMonitor, 
        IJulesActivityClient activityClient,
        ISessionReader sessionReader, 
        ISessionWriter sessionWriter,
        IPrResolver prResolver)
    {
        _pulseMonitor = pulseMonitor;
        _activityClient = activityClient;
        _sessionReader = sessionReader;
        _sessionWriter = sessionWriter;
        _prResolver = prResolver;
    }

    public async Task<RefreshPulseResponse> ExecuteAsync(RefreshPulseRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await _sessionReader.RecallAsync(request.Id, cancellationToken).ConfigureAwait(false);
        
        try
        {
            // 1. Happy Path: Get the latest authoritative state and History from the remote system 💓📜
            // We use a dummy TaskDescription if the session is new to the registry.
            // It will be corrected once we fetch the remote session.
            var task = session?.Task ?? (TaskDescription)"Unknown Task (Recovered)";
            var remoteSessionTask = _pulseMonitor.GetRemoteSessionAsync(request.Id, task, cancellationToken);
            var activitiesTask = _activityClient.GetActivitiesAsync(request.Id, cancellationToken);

            await Task.WhenAll(remoteSessionTask, activitiesTask).ConfigureAwait(false);

            var remoteSession = await remoteSessionTask.ConfigureAwait(false);
            var activities = await activitiesTask.ConfigureAwait(false);
            
            // 2. Mirror Reality: Synchronize the local session with the remote truth 🔄💎
            // If the session was missing, we use the remote one as our base!
            session ??= remoteSession;

            session.UpdatePulse(remoteSession.Pulse);
            
            // Resolve the Pull Request (Remote First).
            // If the resolver returns null (remote is missing), we must purge the local PR (Zombie Artifact).
            var resolvedPr = _prResolver.Resolve(session.PullRequest, remoteSession.PullRequest);
            session.SetPullRequest(resolvedPr);

            foreach (var activity in activities)
            {
                // Simple synchronization: Add only if not already present
                // NOTE: Use local ID check for robustness
                if (session.SessionLog.All(a => a.Id != activity.Id))
                {
                    session.AddActivity(activity);
                }
            }

            await _sessionWriter.RememberAsync(session, cancellationToken).ConfigureAwait(false);
            
            return new RefreshPulseResponse(
                request.Id,
                remoteSession.Pulse,
                session.State,
                session.LastActivity,
                session.PullRequest,
                HasUnsubmittedSolution: session.Solution != null && session.PullRequest == null);
            
        }
        catch (RemoteCollaboratorUnavailableException)
        {
            if (session == null)
            {
                throw new InvalidOperationException($"🔍 Handle {request.Id} not found in the registry and the remote system is unreachable. 🥀");
            }

            // 2. Connectivity Fallback: Business Policy logic lives here. 🧠✨
            return new RefreshPulseResponse(
                request.Id,
                session.Pulse,
                session.State,
                session.LastActivity,
                session.PullRequest,
                HasUnsubmittedSolution: session.Solution != null && session.PullRequest == null,
                IsCached: true,
                Warning: "⚠️ Remote system unreachable. Showing last known state from Task Registry.");
            
        }
    }
}

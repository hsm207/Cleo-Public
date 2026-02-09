using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.RefreshPulse;

public class RefreshPulseUseCase : IRefreshPulseUseCase
{
    private readonly IPulseMonitor _pulseMonitor;
    private readonly IJulesActivityClient _activityClient;
    private readonly ISessionReader _sessionReader;
    private readonly ISessionWriter _sessionWriter;

    public RefreshPulseUseCase(
        IPulseMonitor pulseMonitor, 
        IJulesActivityClient activityClient,
        ISessionReader sessionReader, 
        ISessionWriter sessionWriter)
    {
        _pulseMonitor = pulseMonitor;
        _activityClient = activityClient;
        _sessionReader = sessionReader;
        _sessionWriter = sessionWriter;
    }

    public async Task<RefreshPulseResponse> ExecuteAsync(RefreshPulseRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await _sessionReader.RecallAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            throw new InvalidOperationException($"🔍 Handle {request.Id} not found in the registry. 🥀");
        }

        try
        {
            // 1. Happy Path: Get the latest Pulse and History from the remote system 💓📜
            var pulseTask = _pulseMonitor.GetSessionPulseAsync(request.Id, cancellationToken);
            var activitiesTask = _activityClient.GetActivitiesAsync(request.Id, cancellationToken);

            await Task.WhenAll(pulseTask, activitiesTask).ConfigureAwait(false);

            var pulse = await pulseTask.ConfigureAwait(false);
            var activities = await activitiesTask.ConfigureAwait(false);
            
            // 2. Mirror Reality: Synchronize the local session with the remote truth 🔄💎
            session.UpdatePulse(pulse);
            
            foreach (var activity in activities)
            {
                // Simple synchronization: Add only if not already present
                if (session.SessionLog.All(a => a.Id != activity.Id))
                {
                    session.AddActivity(activity);
                }
            }

            await _sessionWriter.RememberAsync(session, cancellationToken).ConfigureAwait(false);
            
            return new RefreshPulseResponse(request.Id, pulse);
        }
        catch (RemoteCollaboratorUnavailableException)
        {
            // 2. Connectivity Fallback: Business Policy logic lives here. 🧠✨
            return new RefreshPulseResponse(
                request.Id,
                session.Pulse,
                IsCached: true,
                Warning: "⚠️ Remote system unreachable. Showing last known state from Task Registry."
            );
        }
    }
}

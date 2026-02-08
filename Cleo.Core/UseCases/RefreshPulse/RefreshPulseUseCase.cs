using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.UseCases.RefreshPulse;

public class RefreshPulseUseCase : IRefreshPulseUseCase
{
    private readonly IPulseMonitor _pulseMonitor;
    private readonly ISessionReader _sessionReader;
    private readonly ISessionWriter _sessionWriter;

    public RefreshPulseUseCase(IPulseMonitor pulseMonitor, ISessionReader sessionReader, ISessionWriter sessionWriter)
    {
        _pulseMonitor = pulseMonitor;
        _sessionReader = sessionReader;
        _sessionWriter = sessionWriter;
    }

    public async Task<RefreshPulseResponse> ExecuteAsync(RefreshPulseRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var session = await _sessionReader.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (session == null)
        {
            throw new InvalidOperationException($"🔍 Handle {request.Id} not found in the registry, babe. 🥀");
        }

        try
        {
            // 1. Happy Path: Get the latest Pulse from the monitor
            var pulse = await _pulseMonitor.GetSessionPulseAsync(request.Id, cancellationToken).ConfigureAwait(false);
            
            // Update and Save 📝
            session.UpdatePulse(pulse);
            await _sessionWriter.SaveAsync(session, cancellationToken).ConfigureAwait(false);
            
            return new RefreshPulseResponse(request.Id, pulse);
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Net.Sockets.SocketException)
        {
            // 2. Connectivity Fallback: Business Policy logic lives here, babe! 🧠✨
            return new RefreshPulseResponse(
                request.Id,
                session.Pulse,
                IsCached: true,
                Warning: "⚠️ Remote system unreachable. Showing last known state from Task Registry."
            );
        }
    }
}

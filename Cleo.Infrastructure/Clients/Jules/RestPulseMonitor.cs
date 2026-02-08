using System.Net.Http.Json;
using System.Net.Sockets;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using Cleo.Infrastructure.Clients.Jules.Internal;
using Cleo.Infrastructure.Clients.Jules.Mapping;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// Specialized client for monitoring the state and heartbeat of a remote mission.
/// </summary>
public sealed class RestPulseMonitor : IPulseMonitor
{
    private readonly HttpClient _httpClient;
    private readonly ISessionStatusMapper _statusMapper;

    public RestPulseMonitor(HttpClient httpClient, ISessionStatusMapper statusMapper)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _statusMapper = statusMapper ?? throw new ArgumentNullException(nameof(statusMapper));
    }

    public async Task<SessionPulse> GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            var response = await _httpClient.GetAsync(new Uri($"v1alpha/{id.Value}", UriKind.Relative), cancellationToken).ConfigureAwait(false);
            await response.EnsureSuccessWithDetailAsync(cancellationToken).ConfigureAwait(false);

            var dto = await response.Content.ReadFromJsonAsync<JulesSessionDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
            if (dto == null) throw new InvalidOperationException("Failed to retrieve session pulse.");

            return new SessionPulse(_statusMapper.Map(dto.State), $"Session is {dto.State}");
        }
        catch (Exception ex) when (ex is HttpRequestException or SocketException)
        {
            throw new RemoteCollaboratorUnavailableException("Failed to retrieve session pulse due to connectivity issues.", ex);
        }
    }
}

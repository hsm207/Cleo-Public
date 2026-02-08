using System.Net.Sockets;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Internal;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// Specialized client for directing and controlling the progress of a live mission.
/// </summary>
public sealed class RestSessionController : ISessionController
{
    private readonly HttpClient _httpClient;

    public RestSessionController(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task ApprovePlanAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            var response = await _httpClient.PostAsync(new Uri($"v1alpha/{id.Value}:approvePlan", UriKind.Relative), null, cancellationToken).ConfigureAwait(false);
            await response.EnsureSuccessWithDetailAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or SocketException)
        {
            throw new RemoteCollaboratorUnavailableException("Failed to approve plan due to connectivity issues.", ex);
        }
    }
}

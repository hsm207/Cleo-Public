using System.Net.Http.Json;
using System.Net.Sockets;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Requests;
using Cleo.Infrastructure.Clients.Jules.Internal;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// Specialized client for the conversational stream (Talk loop) between the developer and Jules.
/// </summary>
public sealed class RestSessionMessenger : ISessionMessenger
{
    private readonly HttpClient _httpClient;

    public RestSessionMessenger(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

#pragma warning disable CA1062 // Validate arguments of public methods (VIP Lounge Rules: We trust the caller)
    public async Task SendMessageAsync(SessionId id, string message, CancellationToken cancellationToken = default)
    {
        // VIP Lounge: Callers (CLI) must validate inputs.
        var request = new JulesSendMessageRequestDto(message);

        try
        {
            var response = await _httpClient.PostAsJsonAsync($"v1alpha/{id.Value}:sendMessage", request, cancellationToken).ConfigureAwait(false);
            await response.EnsureSuccessWithDetailAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or SocketException)
        {
            throw new RemoteCollaboratorUnavailableException("Failed to send message due to connectivity issues.", ex);
        }
    }
}

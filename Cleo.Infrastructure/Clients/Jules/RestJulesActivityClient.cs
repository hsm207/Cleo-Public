using System.Globalization;
using System.Net.Http.Json;
using System.Net.Sockets;
using Cleo.Core.Domain.Entities; // Needed?
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Internal;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Cleo.Infrastructure.Common;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A REST-based implementation of the Jules activity history client.
/// </summary>
public sealed class RestJulesActivityClient : IJulesActivityClient, ISessionArchivist
{
    private readonly HttpClient _httpClient;
    private readonly IJulesActivityMapper _mapper;

    public RestJulesActivityClient(HttpClient httpClient, IJulesActivityMapper mapper)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IReadOnlyCollection<SessionActivity>> GetActivitiesAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        try
        {
            var allActivities = new List<SessionActivity>();
            string? nextPageToken = null;

            do
            {
                var uri = $"v1alpha/{id.Value}/activities";
                if (nextPageToken != null)
                {
                    uri += $"?pageToken={nextPageToken}";
                }

                var response = await _httpClient.GetAsync(new Uri(uri, UriKind.Relative), cancellationToken).ConfigureAwait(false);
                await response.EnsureSuccessWithDetailAsync(cancellationToken).ConfigureAwait(false);

                var dto = await response.Content.ReadFromJsonAsync<JulesListActivitiesResponseDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
                if (dto?.Activities != null)
                {
                    foreach (var a in dto.Activities)
                    {
                        allActivities.Add(_mapper.Map(a));
                    }
                }

                nextPageToken = dto?.NextPageToken;

            } while (nextPageToken != null);

            return allActivities.AsReadOnly();
        }
        catch (Exception ex) when (ex is HttpRequestException or SocketException)
        {
            throw new RemoteCollaboratorUnavailableException("Failed to retrieve activities due to connectivity issues.", ex);
        }
    }

    async Task<IReadOnlyList<SessionActivity>> ISessionArchivist.GetHistoryAsync(SessionId id, CancellationToken cancellationToken)
    {
        var activities = await GetActivitiesAsync(id, cancellationToken).ConfigureAwait(false);
        return activities.ToList().AsReadOnly();
    }
}

using System.Globalization;
using System.Net.Http.Json;
using System.Net.Sockets;
using Cleo.Core.Domain.Entities;
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
public sealed class RestJulesActivityClient : IRemoteActivitySource, IJulesActivityClient
{
    private readonly HttpClient _httpClient;
    private readonly IJulesActivityMapper _mapper;

    public RestJulesActivityClient(HttpClient httpClient, IJulesActivityMapper mapper)
    {
        _httpClient = httpClient;
        _mapper = mapper;
    }

#pragma warning disable CA1062 // Validate arguments of public methods (VIP Lounge Rules: We trust the caller)
    public async Task<IReadOnlyCollection<SessionActivity>> FetchActivitiesAsync(SessionId id, RemoteActivityOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var allActivities = new List<SessionActivity>();
            var currentOptions = options;

            do
            {
                var uri = JulesQueryBuilder.BuildListActivitiesUri(id, currentOptions);

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

                currentOptions = options with { PageToken = dto?.NextPageToken };

            } while (!string.IsNullOrEmpty(currentOptions.PageToken));

            return allActivities.AsReadOnly();
        }
        catch (Exception ex) when (ex is HttpRequestException or SocketException)
        {
            throw new RemoteCollaboratorUnavailableException("Failed to retrieve activities due to connectivity issues.", ex);
        }
    }

    public Task<IReadOnlyCollection<SessionActivity>> GetActivitiesAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        return FetchActivitiesAsync(id, new RemoteActivityOptions(null, null, null, null), cancellationToken);
    }
}

using System.Globalization;
using System.Net.Http.Json;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Internal;
using Cleo.Infrastructure.Clients.Jules.Mapping;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A REST-based implementation of the Jules activity history client.
/// </summary>
public sealed class RestJulesActivityClient : IJulesActivityClient, ISessionArchivist
{
    private readonly HttpClient _httpClient;
    private readonly IEnumerable<IJulesActivityMapper> _mappers;

    public RestJulesActivityClient(HttpClient httpClient, IEnumerable<IJulesActivityMapper> mappers)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
    }

    public async Task<IReadOnlyCollection<SessionActivity>> GetActivitiesAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

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
                // TODO: Refactor this inline lambda to use a factory or better error handling
                var mapped = new List<SessionActivity>();
                foreach (var a in dto.Activities)
                {
                    var mapper = _mappers.FirstOrDefault(m => m.CanMap(a));
                    if (mapper != null)
                    {
                        mapped.Add(mapper.Map(a));
                    }
                    else
                    {
                        mapped.Add(new MessageActivity(
                            a.Metadata.Name,
                            a.Metadata.Id,
                            DateTimeOffset.Parse(a.Metadata.CreateTime, CultureInfo.InvariantCulture),
                            ActivityOriginator.System,
                            $"Unknown activity type '{a.Metadata.Name}' received."));
                    }
                }
                
                allActivities.AddRange(mapped);
            }

            nextPageToken = dto?.NextPageToken;

        } while (nextPageToken != null);

        return allActivities.AsReadOnly();
    }

    async Task<IReadOnlyList<SessionActivity>> ISessionArchivist.GetHistoryAsync(SessionId id, CancellationToken cancellationToken)
    {
        var activities = await GetActivitiesAsync(id, cancellationToken).ConfigureAwait(false);
        return activities.ToList().AsReadOnly();
    }
}

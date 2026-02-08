using System.Net.Http.Json;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;
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

        var response = await _httpClient.GetFromJsonAsync<ListActivitiesResponse>($"v1alpha/{id.Value}/activities", cancellationToken).ConfigureAwait(false);
        if (response?.Activities == null) return Array.Empty<SessionActivity>();

        return response.Activities
            .Select(dto => _mappers.FirstOrDefault(m => m.CanMap(dto))?.Map(dto) 
                ?? new MessageActivity(dto.Id, dto.CreateTime, ActivityOriginator.System, $"Unknown activity type '{dto.Name}' received."))
            .ToList()
            .AsReadOnly();
    }

    async Task<IReadOnlyList<SessionActivity>> ISessionArchivist.GetHistoryAsync(SessionId id, CancellationToken cancellationToken)
    {
        var activities = await GetActivitiesAsync(id, cancellationToken).ConfigureAwait(false);
        return activities.ToList().AsReadOnly();
    }
}

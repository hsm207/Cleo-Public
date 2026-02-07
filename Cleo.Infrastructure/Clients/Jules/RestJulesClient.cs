using System.Net.Http.Json;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using Cleo.Infrastructure.Clients.Jules.Mapping;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A S.O.L.I.D., high-performance REST implementation of the Jules API client.
/// </summary>
public sealed class RestJulesClient : IJulesClient
{
    private readonly HttpClient _httpClient;
    private readonly IEnumerable<IJulesActivityMapper> _mappers;

    internal RestJulesClient(HttpClient httpClient, IEnumerable<IJulesActivityMapper> mappers)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
    }

    public async Task<Session> CreateSessionAsync(TaskDescription task, SourceContext source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(source);

        var request = new
        {
            prompt = (string)task,
            sourceContext = new
            {
                source = source.Repository,
                githubRepoContext = new
                {
                    startingBranch = source.StartingBranch
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync("v1alpha/sessions", request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<JulesSessionDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
        return JulesMapper.Map(dto!, task);
    }

    public async Task<SessionPulse> GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var dto = await _httpClient.GetFromJsonAsync<JulesSessionDto>($"v1alpha/{id.Value}", cancellationToken).ConfigureAwait(false);
        if (dto == null) throw new InvalidOperationException("Failed to retrieve session pulse.");

        return new SessionPulse(JulesMapper.MapStatus(dto.State), $"Session is {dto.State}");
    }

    public async Task SendMessageAsync(SessionId id, string feedback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(feedback);

        var request = new { messageText = feedback };
        var response = await _httpClient.PostAsJsonAsync($"v1alpha/{id.Value}:sendMessage", request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyCollection<SessionActivity>> GetActivitiesAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var response = await _httpClient.GetFromJsonAsync<ListActivitiesResponse>($"v1alpha/{id.Value}/activities", cancellationToken).ConfigureAwait(false);
        if (response?.Activities == null) return Array.Empty<SessionActivity>();

        return response.Activities
            .Select(dto => _mappers.FirstOrDefault(m => m.CanMap(dto))?.Map(dto) 
                ?? throw new InvalidOperationException($"No suitable mapping pattern found for activity {dto.Id}."))
            .ToList()
            .AsReadOnly();
    }
}

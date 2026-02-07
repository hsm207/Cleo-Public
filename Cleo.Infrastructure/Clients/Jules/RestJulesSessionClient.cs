using System.Net.Http.Json;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using Cleo.Infrastructure.Clients.Jules.Mapping;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A REST-based implementation of the Jules session lifecycle client.
/// </summary>
public sealed class RestJulesSessionClient : IJulesSessionClient
{
    private readonly HttpClient _httpClient;
    private readonly ISessionStatusMapper _statusMapper;

    public RestJulesSessionClient(HttpClient httpClient, ISessionStatusMapper statusMapper)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _statusMapper = statusMapper ?? throw new ArgumentNullException(nameof(statusMapper));
    }

    public async Task<Session> CreateSessionAsync(TaskDescription task, SourceContext source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(source);

        var request = new
        {
            prompt = (string)task,
            title = ((string)task).Length > 50 ? ((string)task)[..47] + "..." : (string)task,
            sourceContext = new
            {
                source = source.Repository,
                githubRepoContext = new
                {
                    startingBranch = source.StartingBranch
                }
            },
            requirePlanApproval = true
        };

        var response = await _httpClient.PostAsJsonAsync("v1alpha/sessions", request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<JulesSessionDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
        return JulesMapper.Map(dto!, task, _statusMapper);
    }

    public async Task<SessionPulse> GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var dto = await _httpClient.GetFromJsonAsync<JulesSessionDto>($"v1alpha/{id.Value}", cancellationToken).ConfigureAwait(false);
        if (dto == null) throw new InvalidOperationException("Failed to retrieve session pulse.");

        return new SessionPulse(_statusMapper.Map(dto.State), $"Session is {dto.State}");
    }

    public async Task SendMessageAsync(SessionId id, string feedback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(feedback);

        var request = new { messageText = feedback };
        var response = await _httpClient.PostAsJsonAsync($"v1alpha/{id.Value}:sendMessage", request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task ApprovePlanAsync(SessionId id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        var response = await _httpClient.PostAsync(new Uri($"v1alpha/{id.Value}:approvePlan", UriKind.Relative), null, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}

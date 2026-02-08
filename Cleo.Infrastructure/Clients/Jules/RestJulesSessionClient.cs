using System.Net.Http.Json;
using System.Net.Sockets;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using Cleo.Infrastructure.Clients.Jules.Internal;
using Cleo.Infrastructure.Clients.Jules.Mapping;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A REST-based implementation of the Jules session lifecycle client.
/// </summary>
public sealed class RestJulesSessionClient : IJulesSessionClient, ISessionMessenger, IPulseMonitor
{
    private readonly HttpClient _httpClient;
    private readonly ISessionStatusMapper _statusMapper;

    public RestJulesSessionClient(HttpClient httpClient, ISessionStatusMapper statusMapper)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _statusMapper = statusMapper ?? throw new ArgumentNullException(nameof(statusMapper));
    }

    public async Task<Session> CreateSessionAsync(
        TaskDescription task, 
        SourceContext source, 
        SessionCreationOptions options, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(options);

        var request = new
        {
            prompt = (string)task,
            title = options.Title,
            sourceContext = new
            {
                source = source.Repository,
                githubRepoContext = new
                {
                    startingBranch = source.StartingBranch
                }
            },
            requirePlanApproval = options.RequirePlanApproval,
            automationMode = options.Mode == AutomationMode.AutoCreatePullRequest ? "AUTO_CREATE_PR" : "NONE"
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("v1alpha/sessions", request, cancellationToken).ConfigureAwait(false);
            await response.EnsureSuccessWithDetailAsync(cancellationToken).ConfigureAwait(false);

            var dto = await response.Content.ReadFromJsonAsync<JulesSessionDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
            return JulesMapper.Map(dto!, task, _statusMapper);
        }
        catch (Exception ex) when (ex is HttpRequestException or SocketException)
        {
            throw new RemoteCollaboratorUnavailableException("Failed to initiate session due to connectivity issues.", ex);
        }
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

    public async Task SendMessageAsync(SessionId id, string feedback, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(feedback);

        var request = new { prompt = feedback };
        
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

    async Task<SessionPulse> IPulseMonitor.GetSessionPulseAsync(SessionId id, CancellationToken cancellationToken)
    {
        return await GetSessionPulseAsync(id, cancellationToken).ConfigureAwait(false);
    }

    async Task ISessionMessenger.SendMessageAsync(SessionId id, string message, CancellationToken cancellationToken)
    {
        await SendMessageAsync(id, message, cancellationToken).ConfigureAwait(false);
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

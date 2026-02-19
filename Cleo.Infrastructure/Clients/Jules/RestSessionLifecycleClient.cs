using System.Net.Http.Json;
using System.Net.Sockets;
using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Exceptions;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Dtos.Requests;
using Cleo.Infrastructure.Clients.Jules.Internal;
using Cleo.Infrastructure.Clients.Jules.Mapping;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// specialized client for the creation and initialization of remote Jules sessions.
/// </summary>
public sealed class RestSessionLifecycleClient : IJulesSessionClient
{
    private readonly HttpClient _httpClient;
    private readonly ISessionStatusMapper _statusMapper;

    public RestSessionLifecycleClient(HttpClient httpClient, ISessionStatusMapper statusMapper)
    {
        _httpClient = httpClient;
        _statusMapper = statusMapper;
    }

#pragma warning disable CA1062 // Validate arguments of public methods (VIP Lounge Rules: We trust the caller)
    public async Task<Session> CreateSessionAsync(
        TaskDescription task,
        SourceContext source,
        SessionCreationOptions options,
        CancellationToken cancellationToken = default)
    {
        var request = new JulesCreateSessionRequestDto(
            (string)task,
            new JulesSourceContextDto(
                source.Repository,
                new JulesGithubRepoContextDto(source.StartingBranch)),
            options.Title,
            options.RequirePlanApproval,
            options.Mode == AutomationMode.AutoCreatePr ? "AUTO_CREATE_PR" : "AUTOMATION_MODE_UNSPECIFIED"
        );

        try
        {
            var response = await _httpClient.PostAsJsonAsync("v1alpha/sessions", request, cancellationToken).ConfigureAwait(false);
            await response.EnsureSuccessWithDetailAsync(cancellationToken).ConfigureAwait(false);

            var dto = await response.Content.ReadFromJsonAsync<JulesSessionResponseDto>(cancellationToken: cancellationToken).ConfigureAwait(false);
            return JulesMapper.Map(dto!, _statusMapper);
        }
        catch (Exception ex) when (ex is HttpRequestException or SocketException)
        {
            throw new RemoteCollaboratorUnavailableException("Failed to initiate session due to connectivity issues.", ex);
        }
    }
}

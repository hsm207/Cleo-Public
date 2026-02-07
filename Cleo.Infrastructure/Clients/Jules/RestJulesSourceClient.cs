using System.Net.Http.Json;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A REST-based implementation of the Jules source discovery client.
/// </summary>
public sealed class RestJulesSourceClient : IJulesSourceClient
{
    private readonly HttpClient _httpClient;

    public RestJulesSourceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IReadOnlyCollection<SessionSource>> ListSourcesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<ListSourcesResponse>("v1alpha/sources", cancellationToken).ConfigureAwait(false);
        if (response?.Sources == null) return Array.Empty<SessionSource>();

        return response.Sources
            .Select(dto => new SessionSource(dto.Name, dto.GithubRepo?.Owner ?? "unknown", dto.GithubRepo?.Repo ?? "unknown"))
            .ToList()
            .AsReadOnly();
    }
}

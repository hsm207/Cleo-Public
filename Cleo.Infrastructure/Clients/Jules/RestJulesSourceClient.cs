using System.Net.Http.Json;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Internal;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A REST-based implementation of the Jules source discovery client.
/// </summary>
public sealed class RestJulesSourceClient : IJulesSourceClient, ISourceCatalog
{
    private readonly HttpClient _httpClient;

    public RestJulesSourceClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IReadOnlyCollection<SessionSource>> ListSourcesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(new Uri("v1alpha/sources", UriKind.Relative), cancellationToken).ConfigureAwait(false);
        await response.EnsureSuccessWithDetailAsync(cancellationToken).ConfigureAwait(false);

        var dto = await response.Content.ReadFromJsonAsync<ListSourcesResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (dto?.Sources == null) return Array.Empty<SessionSource>();

        return dto.Sources
            .Select(s => new SessionSource(s.Name, s.GithubRepo?.Owner ?? "unknown", s.GithubRepo?.Repo ?? "unknown"))
            .ToList()
            .AsReadOnly();
    }

    async Task<IReadOnlyList<SessionSource>> ISourceCatalog.GetAvailableSourcesAsync(CancellationToken cancellationToken)
    {
        var sources = await ListSourcesAsync(cancellationToken).ConfigureAwait(false);
        return sources.ToList().AsReadOnly();
    }
}

using System.Net.Http.Headers;
using Cleo.Core.Domain.Ports;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// A delegating handler that automatically retrieves the Jules API key from the vault
/// and adds it to the outgoing HTTP requests.
/// </summary>
internal sealed class JulesAuthHandler : DelegatingHandler
{
    private readonly IVault _vault;

    public JulesAuthHandler(IVault vault)
    {
        _vault = vault;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var identity = await _vault.RetrieveAsync(cancellationToken).ConfigureAwait(false);
        
        if (identity != null)
        {
            request.Headers.Add("x-goog-api-key", (string)identity.ApiKey);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}

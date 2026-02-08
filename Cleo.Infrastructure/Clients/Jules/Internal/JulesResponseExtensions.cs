using Cleo.Core.Domain.Exceptions;

namespace Cleo.Infrastructure.Clients.Jules.Internal;

internal static class JulesResponseExtensions
{
    public static async Task EnsureSuccessWithDetailAsync(this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new RemoteCollaboratorUnavailableException(
                $"Unable to communicate with Jules. (Status: {(int)response.StatusCode}) - Response: {errorBody}");
        }
    }
}

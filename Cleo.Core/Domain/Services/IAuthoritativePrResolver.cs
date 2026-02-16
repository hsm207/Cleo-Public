using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Services;

/// <summary>
/// Resolves the authoritative Pull Request from local and remote signals.
/// </summary>
public interface IAuthoritativePrResolver
{
    /// <summary>
    /// Resolves the authoritative Pull Request.
    /// </summary>
    /// <param name="local">The locally persisted PR.</param>
    /// <param name="remote">The remote PR.</param>
    /// <returns>The authoritative PR, or null if neither exists.</returns>
    PullRequest? Resolve(PullRequest? local, PullRequest? remote);
}

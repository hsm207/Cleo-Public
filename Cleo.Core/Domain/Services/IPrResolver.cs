using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Services;

/// <summary>
/// Resolves the Pull Request from local and remote signals.
/// </summary>
public interface IPrResolver
{
    /// <summary>
    /// Resolves the Pull Request.
    /// </summary>
    /// <param name="local">The locally persisted PR.</param>
    /// <param name="remote">The remote PR.</param>
    /// <returns>The resolved PR, or null if neither exists.</returns>
    PullRequest? Resolve(PullRequest? local, PullRequest? remote);
}

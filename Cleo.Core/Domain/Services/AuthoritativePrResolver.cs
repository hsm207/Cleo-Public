using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Services;

/// <summary>
/// Resolves the authoritative Pull Request from local and remote signals, prioritizing the remote source of truth.
/// </summary>
public class AuthoritativePrResolver : IAuthoritativePrResolver
{
    public PullRequest? Resolve(PullRequest? local, PullRequest? remote)
    {
        // Remote PR is the authoritative source of truth.
        if (remote != null)
        {
            return remote;
        }

        // Fallback to local PR if remote is missing (e.g., offline or partial sync).
        return local;
    }
}

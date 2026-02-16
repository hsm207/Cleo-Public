using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Services;

/// <summary>
/// Resolves the Pull Request by strictly enforcing the remote source of truth.
/// </summary>
public class RemoteFirstPrResolver : IPrResolver
{
    public PullRequest? Resolve(PullRequest? local, PullRequest? remote)
    {
        // Remote First: We strictly mirror the remote source of truth.
        // If remote is null, we return null, effectively purging any local "zombie" PR.
        return remote;
    }
}

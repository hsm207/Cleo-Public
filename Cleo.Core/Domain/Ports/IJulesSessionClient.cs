using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A portal for initializing new remote Jules sessions.
/// Acts as an Abstract Factory for remote resources.
/// </summary>
public interface IJulesSessionClient
{
    /// <summary>
    /// Launches a new remote session for a specific task and source.
    /// </summary>
    Task<Session> CreateSessionAsync(
        TaskDescription task,
        SourceContext source,
        SessionCreationOptions options,
        CancellationToken cancellationToken = default);
}

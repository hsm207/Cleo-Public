using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Core.Domain.Ports;

/// <summary>
/// A symmetric, action-oriented port for persisting and forgetting active Jules sessions.
/// Acts as the primary mechanism for managing Cleo's Workbench Memory. 🧠✨
/// </summary>
public interface ISessionWriter
{
    /// <summary>
    /// Records a session in the local registry, ensuring it is remembered across sessions.
    /// </summary>
    Task RememberAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a session from the local registry (Workbench Memory Abandonment).
    /// </summary>
    Task ForgetAsync(SessionId id, CancellationToken cancellationToken = default);
}

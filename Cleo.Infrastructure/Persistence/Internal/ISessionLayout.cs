using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence.Internal;

/// <summary>
/// Defines the physical layout strategy for a session's storage.
/// Responsible for resolving paths to session artifacts. 🗺️
/// </summary>
public interface ISessionLayout
{
    /// <summary>
    /// resolving the root directory for a specific session.
    /// </summary>
    string GetSessionDirectory(SessionId sessionId);

    /// <summary>
    /// resolving the path to the session metadata file (session.json).
    /// </summary>
    string GetMetadataPath(SessionId sessionId);

    /// <summary>
    /// resolving the path to the session history file (activities.jsonl).
    /// </summary>
    string GetHistoryPath(SessionId sessionId);
}

/// <summary>
/// Responsible for resolving the base storage path for sessions.
/// </summary>
public interface ISessionPathResolver
{
    /// <summary>
    /// Gets the root directory where all sessions are stored.
    /// </summary>
    string GetSessionsRoot();
}

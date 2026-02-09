using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence.Internal;

/// <summary>
/// A plugin interface for mapping specific domain activity types to/from opaque JSON payloads.
/// Enables the persistence boundary to be open for extension but closed for modification.
/// </summary>
internal interface IActivityPersistenceMapper
{
    /// <summary>
    /// The stable string discriminator used in the envelope (e.g., "PLANNING").
    /// </summary>
    string TypeKey { get; }

    /// <summary>
    /// Determines if this mapper can handle the given domain activity.
    /// </summary>
    bool CanHandle(SessionActivity activity);

    /// <summary>
    /// Serializes the rich domain activity into an opaque JSON string.
    /// </summary>
    string SerializePayload(SessionActivity activity);

    /// <summary>
    /// Hydrates a rich domain activity from an opaque JSON string.
    /// </summary>
    SessionActivity DeserializePayload(string id, DateTimeOffset timestamp, ActivityOriginator originator, string json);
}

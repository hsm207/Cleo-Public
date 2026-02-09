using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Infrastructure.Persistence.Internal;

/// <summary>
/// A plugin interface for mapping specific evidence artifacts to/from opaque JSON payloads.
/// </summary>
internal interface IArtifactPersistenceMapper
{
    string TypeKey { get; }
    bool CanHandle(Artifact artifact);
    string Serialize(Artifact artifact);
    Artifact Deserialize(string json);
}

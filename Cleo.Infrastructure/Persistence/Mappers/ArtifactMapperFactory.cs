using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

/// <summary>
/// A factory for orchestrating the polymorphic mapping of evidence artifacts.
/// </summary>
internal sealed class ArtifactMapperFactory
{
    private readonly IEnumerable<IArtifactPersistenceMapper> _mappers;

    public ArtifactMapperFactory(IEnumerable<IArtifactPersistenceMapper> mappers)
    {
        _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
    }

    public ArtifactEnvelope ToEnvelope(Artifact artifact)
    {
        var mapper = _mappers.FirstOrDefault(m => m.CanHandle(artifact))
            ?? throw new InvalidOperationException($"No persistence mapper found for artifact: {artifact.GetType().Name}");

        return new ArtifactEnvelope
        {
            Type = mapper.TypeKey,
            PayloadJson = mapper.Serialize(artifact)
        };
    }

    public Artifact FromEnvelope(ArtifactEnvelope envelope)
    {
        var mapper = _mappers.FirstOrDefault(m => m.TypeKey == envelope.Type)
            ?? throw new InvalidOperationException($"No persistence mapper found for stored artifact type: {envelope.Type}");

        return mapper.Deserialize(envelope.PayloadJson);
    }
}

public sealed class ArtifactEnvelope
{
    public string Type { get; init; } = default!;
    public string PayloadJson { get; init; } = default!;
}

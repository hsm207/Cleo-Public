namespace Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

/// <summary>
/// Envelope-level metadata for a Jules activity, sectioned off for ACL intuition.
/// </summary>
public sealed record JulesActivityMetadata(
    string Id,
    string Name,
    string? Description,
    string CreateTime,
    string Originator,
    IReadOnlyList<ArtifactDto>? Artifacts
);

namespace Cleo.Infrastructure.Persistence.Internal;

/// <summary>
/// A stable, high-level envelope for persisting session activities.
/// Following the Envelope Pattern to ensure OCP compliance.
/// </summary>
public sealed class ActivityEnvelopeDto
{
    /// <summary>
    /// The stable type discriminator (e.g., "PLANNING", "MESSAGE").
    /// Ensures persistence remains decoupled from C# class names.
    /// </summary>
    public string Type { get; init; } = default!;

    public string Id { get; init; } = default!;

    public DateTimeOffset Timestamp { get; init; }

    public string Originator { get; init; } = default!;

    /// <summary>
    /// The opaque, type-specific payload.
    /// Deferred to specialized mappers for serialization.
    /// </summary>
    public string PayloadJson { get; init; } = default!;
}

namespace Cleo.Core.Domain.Common;

/// <summary>
/// Represents a significant occurrence within the domain.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The exact moment this event occurred.
    /// </summary>
    DateTimeOffset OccurredOn { get; }
}

namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents the business evaluation of a session's deliverables.
/// </summary>
public enum DeliveryStatus
{
    Pending,
    Stalled,
    Unfulfilled,
    Delivered
}

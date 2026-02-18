namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents the unique identifier for a generated plan.
/// </summary>
public record PlanId
{
    public string Value { get; init; }

    public PlanId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Plan identifier cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static explicit operator PlanId(string value) => FromString(value);
    public static PlanId FromString(string value) => new(value);

    public static implicit operator string(PlanId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return id.Value;
    }

    public override string ToString() => Value;
}

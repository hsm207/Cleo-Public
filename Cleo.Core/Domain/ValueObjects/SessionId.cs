namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents the unique handle for a Jules session.
/// </summary>
public record SessionId
{
    public string Value { get; init; }

    public SessionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Session identifier cannot be empty.", nameof(value));
        }

        if (!value.StartsWith("sessions/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Session identifier must start with 'sessions/'.", nameof(value));
        }

        Value = value;
    }

    public static explicit operator SessionId(string value) => FromString(value);
    public static SessionId FromString(string value) => new(value);
    public static implicit operator string(SessionId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return id.Value;
    }

    public override string ToString() => Value;
}

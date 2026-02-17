namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents the secret token used to authenticate with the Jules API.
/// </summary>
public record ApiKey
{
    public string Value { get; init; }

    public ApiKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("API Key cannot be empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public static explicit operator ApiKey(string value) => FromString(value);
    public static ApiKey FromString(string value) => new(value);
    
    public static implicit operator string(ApiKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return key.Value;
    }

    public override string ToString() => Value;
}

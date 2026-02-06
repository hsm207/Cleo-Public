namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents the developer's intent or mission for a Jules session.
/// </summary>
public record TaskDescription
{
    public string Value { get; init; }

    public TaskDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Task description cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static implicit operator string(TaskDescription task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return task.Value;
    }

    public static implicit operator TaskDescription(string value) => FromString(value);

    public static TaskDescription FromString(string value) => new(value);
    public override string ToString() => Value;
}

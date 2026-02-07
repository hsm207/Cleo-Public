namespace Cleo.Core.Domain.ValueObjects;

public enum MessageSender
{
    User,
    Agent,
    System
}

/// <summary>
/// Represents a single message in the collaboration stream.
/// </summary>
public record ChatMessage(MessageSender Sender, string Content, DateTimeOffset Timestamp);

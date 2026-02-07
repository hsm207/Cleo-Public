using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class ChatMessageTests
{
    [Fact(DisplayName = "A ChatMessage should correctly store sender, content, and timestamp.")]
    public void ConstructorShouldSetValues()
    {
        var sender = MessageSender.Agent;
        var content = "I have a plan!";
        var timestamp = DateTimeOffset.UtcNow;
        var message = new ChatMessage(sender, content, timestamp);

        Assert.Equal(sender, message.Sender);
        Assert.Equal(content, message.Content);
        Assert.Equal(timestamp, message.Timestamp);
    }
}

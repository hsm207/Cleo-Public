using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Cleo.Core.Tests.Domain.Ports;

public class JulesClientTests
{
    private readonly SessionId _testId = new("sessions/123");
    private readonly TaskDescription _testTask = new("Fix bug");
    private readonly SourceContext _testSource = new("repo", "main");

    [Fact(DisplayName = "The IJulesClient port should define a clear contract for session lifecycle and collaboration.")]
    public async Task JulesClientPortShouldDefineStandardOperations()
    {
        var mockClient = new Mock<IJulesClient>();

        // Verify Create
        mockClient.Setup(c => c.CreateSessionAsync(_testTask, _testSource, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new Session(_testId, _testTask, _testSource, new SessionPulse(SessionStatus.StartingUp)));
        var session = await mockClient.Object.CreateSessionAsync(_testTask, _testSource);
        Assert.Equal(_testId, session.Id);

        // Verify GetPulse
        var pulse = new SessionPulse(SessionStatus.InProgress, "Thinking...");
        mockClient.Setup(c => c.GetSessionPulseAsync(_testId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(pulse);
        var retrievedPulse = await mockClient.Object.GetSessionPulseAsync(_testId);
        Assert.Equal(pulse, retrievedPulse);

        // Verify SendMessage
        await mockClient.Object.SendMessageAsync(_testId, "Hello");
        mockClient.Verify(c => c.SendMessageAsync(_testId, "Hello", It.IsAny<CancellationToken>()), Times.Once);

        // Verify GetSolution
        var patch = new SolutionPatch("diff", "sha");
        mockClient.Setup(c => c.GetLatestSolutionAsync(_testId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(patch);
        var retrievedPatch = await mockClient.Object.GetLatestSolutionAsync(_testId);
        Assert.Equal(patch, retrievedPatch);

        // Verify GetConversation
        mockClient.Setup(c => c.GetConversationAsync(_testId, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new[] { new ChatMessage(MessageSender.Agent, "Hi", DateTimeOffset.UtcNow) });
        var messages = await mockClient.Object.GetConversationAsync(_testId);
        Assert.Single(messages);
    }
}

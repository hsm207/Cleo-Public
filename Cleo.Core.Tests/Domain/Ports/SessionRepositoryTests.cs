using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.Ports;
using Cleo.Core.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Cleo.Core.Tests.Domain.Ports;

public class SessionRepositoryTests
{
    private readonly SessionId _testId = new("sessions/123");
    private readonly Session _testSession;

    public SessionRepositoryTests()
    {
        _testSession = new Session(
            _testId,
            new TaskDescription("Test"),
            new SourceContext("repo", "main"),
            new SessionPulse(SessionStatus.StartingUp));
    }

    [Fact(DisplayName = "The ISessionRepository port should define a clear contract for CRUD operations on sessions.")]
    public async Task SessionRepositoryPortShouldDefineStandardOperations()
    {
        var mockRepo = new Mock<ISessionRepository>();

        // Verify Save
        await mockRepo.Object.SaveAsync(_testSession, TestContext.Current.CancellationToken);
        mockRepo.Verify(r => r.SaveAsync(_testSession, It.IsAny<CancellationToken>()), Times.Once);

        // Verify GetById
        mockRepo.Setup(r => r.GetByIdAsync(_testId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testSession);
        var retrieved = await mockRepo.Object.GetByIdAsync(_testId, TestContext.Current.CancellationToken);
        Assert.Equal(_testSession, retrieved);

        // Verify List
        mockRepo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { _testSession });
        var list = await mockRepo.Object.ListAsync(TestContext.Current.CancellationToken);
        Assert.Single(list);

        // Verify Delete
        await mockRepo.Object.DeleteAsync(_testId, TestContext.Current.CancellationToken);
        mockRepo.Verify(r => r.DeleteAsync(_testId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

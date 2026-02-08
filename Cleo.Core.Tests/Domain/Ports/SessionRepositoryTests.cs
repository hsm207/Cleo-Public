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

    [Fact(DisplayName = "The ISessionReader port should define a clear contract for reading operations on sessions.")]
    public async Task SessionReaderPortShouldDefineStandardOperations()
    {
        var mockReader = new Mock<ISessionReader>();

        // Verify GetById
        mockReader.Setup(r => r.RecallAsync(_testId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testSession);
        var retrieved = await mockReader.Object.RecallAsync(_testId, TestContext.Current.CancellationToken);
        Assert.Equal(_testSession, retrieved);

        // Verify List
        mockReader.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { _testSession });
        var list = await mockReader.Object.ListAsync(TestContext.Current.CancellationToken);
        Assert.Single(list);
    }

    [Fact(DisplayName = "The ISessionWriter port should define a clear contract for writing operations on sessions.")]
    public async Task SessionWriterPortShouldDefineStandardOperations()
    {
        var mockWriter = new Mock<ISessionWriter>();

        // Verify Save
        await mockWriter.Object.RememberAsync(_testSession, TestContext.Current.CancellationToken);
        mockWriter.Verify(r => r.RememberAsync(_testSession, It.IsAny<CancellationToken>()), Times.Once);

        // Verify Delete
        await mockWriter.Object.ForgetAsync(_testId, TestContext.Current.CancellationToken);
        mockWriter.Verify(r => r.ForgetAsync(_testId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

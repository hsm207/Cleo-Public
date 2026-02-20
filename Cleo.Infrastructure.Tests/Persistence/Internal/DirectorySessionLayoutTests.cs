using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Tests.Common;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence.Internal;

public sealed class DirectorySessionLayoutTests
{
    private readonly Mock<ISessionPathResolver> _resolver;
    private readonly DirectorySessionLayout _layout;

    public DirectorySessionLayoutTests()
    {
        _resolver = new Mock<ISessionPathResolver>();
        _layout = new DirectorySessionLayout(_resolver.Object);
    }

    [Fact]
    public void GetSessionDirectory_ReturnsCorrectPath()
    {
        // Arrange
        var root = "/app/data/sessions";
        _resolver.Setup(x => x.GetSessionsRoot()).Returns(root);
        var sessionId = TestFactory.CreateSessionId("123");

        // Act
        var result = _layout.GetSessionDirectory(sessionId);

        // Assert
        Assert.Equal("/app/data/sessions/123", result);
    }

    [Fact]
    public void GetMetadataPath_ReturnsCorrectPath()
    {
        // Arrange
        var root = "/app/data/sessions";
        _resolver.Setup(x => x.GetSessionsRoot()).Returns(root);
        var sessionId = TestFactory.CreateSessionId("123");

        // Act
        var result = _layout.GetMetadataPath(sessionId);

        // Assert
        Assert.Equal("/app/data/sessions/123/session.json", result);
    }

    [Fact]
    public void GetHistoryPath_ReturnsCorrectPath()
    {
        // Arrange
        var root = "/app/data/sessions";
        _resolver.Setup(x => x.GetSessionsRoot()).Returns(root);
        var sessionId = TestFactory.CreateSessionId("123");

        // Act
        var result = _layout.GetHistoryPath(sessionId);

        // Assert
        Assert.Equal("/app/data/sessions/123/activities.jsonl", result);
    }
}

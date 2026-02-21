using Cleo.Infrastructure.Persistence.Internal;
using Cleo.Tests.Common;
using Moq;

namespace Cleo.Infrastructure.Tests.Persistence.Internal;

public sealed class DirectorySessionProvisionerTests
{
    private readonly Mock<ISessionLayout> _layout;
    private readonly Mock<IFileSystem> _fileSystem;
    private readonly DirectorySessionProvisioner _provisioner;

    public DirectorySessionProvisionerTests()
    {
        _layout = new Mock<ISessionLayout>();
        _fileSystem = new Mock<IFileSystem>();
        _provisioner = new DirectorySessionProvisioner(_layout.Object, _fileSystem.Object);
    }

    [Fact]
    public void EnsureSessionDirectory_CreatesDirectory_WhenItDoesNotExist()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var directoryPath = "/sessions/123";

        _layout.Setup(x => x.GetSessionDirectory(sessionId)).Returns(directoryPath);
        _fileSystem.Setup(x => x.DirectoryExists(directoryPath)).Returns(false);

        // Act
        _provisioner.EnsureSessionDirectory(sessionId);

        // Assert
        _fileSystem.Verify(x => x.CreateDirectory(directoryPath), Times.Once);
    }

    [Fact]
    public void EnsureSessionDirectory_DoesNotCreateDirectory_WhenItExists()
    {
        // Arrange
        var sessionId = TestFactory.CreateSessionId("123");
        var directoryPath = "/sessions/123";

        _layout.Setup(x => x.GetSessionDirectory(sessionId)).Returns(directoryPath);
        _fileSystem.Setup(x => x.DirectoryExists(directoryPath)).Returns(true);

        // Act
        _provisioner.EnsureSessionDirectory(sessionId);

        // Assert
        _fileSystem.Verify(x => x.CreateDirectory(It.IsAny<string>()), Times.Never);
    }
}

using Cleo.Cli.Commands;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.AuthenticateUser;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class AuthCommandTests : IDisposable
{
    private readonly Mock<IAuthenticateUserUseCase> _useCaseMock;
    private readonly Mock<ICredentialStore> _credentialStoreMock;
    private readonly Mock<ILogger<AuthCommand>> _loggerMock;
    private readonly AuthCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public AuthCommandTests()
    {
        _useCaseMock = new Mock<IAuthenticateUserUseCase>();
        _credentialStoreMock = new Mock<ICredentialStore>();
        _loggerMock = new Mock<ILogger<AuthCommand>>();
        _command = new AuthCommand(_useCaseMock.Object, _credentialStoreMock.Object, _loggerMock.Object);

        _stringWriter = new StringWriter();
        _originalOutput = Console.Out;
        Console.SetOut(_stringWriter);
    }

    public void Dispose()
    {
        Console.SetOut(_originalOutput);
        _stringWriter.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Given valid key, when running 'auth login', then it should authenticate and display success.")]
    public async Task Login_ValidKey_DisplaysSuccess()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<AuthenticateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticateUserResponse(true, "Welcome!"));

        // Act
        var exitCode = await _command.Build().InvokeAsync("auth login my-key");

        // Assert
        exitCode.Should().Be(0);
        _stringWriter.ToString().Should().Contain("✅ Welcome!");
    }

    [Fact(DisplayName = "Given invalid key, when running 'auth login', then it should display error.")]
    public async Task Login_InvalidKey_DisplaysError()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<AuthenticateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticateUserResponse(false, "Invalid key"));

        // Act
        await _command.Build().InvokeAsync("auth login bad-key");

        // Assert
        _stringWriter.ToString().Should().Contain("❌ Error: Invalid key");
    }

    [Fact(DisplayName = "When running 'auth logout', then it should clear credentials and say goodbye.")]
    public async Task Logout_ClearsCredentials()
    {
        // Act
        var exitCode = await _command.Build().InvokeAsync("auth logout");

        // Assert
        exitCode.Should().Be(0);
        _stringWriter.ToString().Should().Contain("🗑️ Credentials cleared");
        _credentialStoreMock.Verify(x => x.ClearIdentityAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Given exception, when running 'auth login', then it should handle it.")]
    public async Task Login_Exception_HandlesIt()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<AuthenticateUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Vault locked"));

        // Act
        await _command.Build().InvokeAsync("auth login key");

        // Assert
        _stringWriter.ToString().Should().Contain("💔 Error: Vault locked");
    }
}

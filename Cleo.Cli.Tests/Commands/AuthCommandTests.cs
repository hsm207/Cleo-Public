using Cleo.Cli.Commands;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.AuthenticateUser;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class AuthCommandTests : IDisposable
{
    private readonly Mock<IAuthenticateUserUseCase> _useCaseMock;
    private readonly Mock<IVault> _vaultMock;
    private readonly Mock<ILogger<AuthCommand>> _loggerMock;
    private readonly AuthCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public AuthCommandTests()
    {
        _useCaseMock = new Mock<IAuthenticateUserUseCase>();
        _vaultMock = new Mock<IVault>();
        _loggerMock = new Mock<ILogger<AuthCommand>>();

        // Arrange SUT
        _command = new AuthCommand(_useCaseMock.Object, _vaultMock.Object, _loggerMock.Object);

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
        // Simulate successful login logic
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<AuthenticateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticateUserResponse(true, "Welcome!"));

        var cmd = _command.Build();

        // Act
        var result = await cmd.InvokeAsync("login my-key");

        // Assert
        Assert.Equal(0, result);
        var output = _stringWriter.ToString();
        Assert.Contains("✅ Welcome!", output);
    }

    [Fact(DisplayName = "Given invalid key, when running 'auth login', then it should display error.")]
    public async Task Login_InvalidKey_DisplaysError()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<AuthenticateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticateUserResponse(false, "Invalid key"));

        var cmd = _command.Build();

        // Act
        var result = await cmd.InvokeAsync("login bad-key");

        // Assert
        Assert.Equal(0, result); // Command handled it gracefully
        var output = _stringWriter.ToString();
        Assert.Contains("❌ Error: Invalid key", output);
    }

    [Fact(DisplayName = "When running 'auth logout', then it should clear credentials and say goodbye.")]
    public async Task Logout_ClearsCredentials()
    {
        // Arrange
        var cmd = _command.Build();

        // Act
        var result = await cmd.InvokeAsync("logout");

        // Assert
        Assert.Equal(0, result);
        var output = _stringWriter.ToString();
        Assert.Contains("🗑️ Credentials cleared", output);
        _vaultMock.Verify(x => x.ClearAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Given exception, when running 'auth login', then it should handle it.")]
    public async Task Login_Exception_HandlesIt()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<AuthenticateUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Vault locked"));

        var cmd = _command.Build();

        // Act
        var result = await cmd.InvokeAsync("login key");

        // Assert
        Assert.Equal(0, result);
        var output = _stringWriter.ToString();
        Assert.Contains("💔 Error: Vault locked", output);
    }
}

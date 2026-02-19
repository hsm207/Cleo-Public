using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.Ports;
using Cleo.Core.UseCases.AuthenticateUser;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public sealed class AuthCommandTests
{
    private readonly Mock<IAuthenticateUserUseCase> _useCaseMock;
    private readonly Mock<IVault> _vaultMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly AuthCommand _command;

    public AuthCommandTests()
    {
        _useCaseMock = new Mock<IAuthenticateUserUseCase>();
        _vaultMock = new Mock<IVault>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();

        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k);
        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(k => k);

        _command = new AuthCommand(
            _useCaseMock.Object,
            _vaultMock.Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            new Mock<ILogger<AuthCommand>>().Object);
    }

    [Fact(DisplayName = "Login with valid key should call UseCase and PresentSuccess.")]
    public async Task Login_ValidKey_Authenticates()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.Is<AuthenticateUserRequest>(r => r.ApiKey == "valid-key"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticateUserResponse(true, "Welcome!"));

        // Act
        await _command.Build().InvokeAsync("auth login valid-key");

        // Assert
        _presenterMock.Verify(x => x.PresentSuccess("Welcome!"), Times.Once);
        _presenterMock.Verify(x => x.PresentError(It.IsAny<string>()), Times.Never);
    }

    [Fact(DisplayName = "Login failure should PresentError.")]
    public async Task Login_InvalidKey_PresentsError()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<AuthenticateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticateUserResponse(false, "Invalid key"));

        // Act
        await _command.Build().InvokeAsync("auth login bad-key");

        // Assert
        _presenterMock.Verify(x => x.PresentError("Invalid key"), Times.Once);
    }

    [Fact(DisplayName = "Login exception should PresentError.")]
    public async Task Login_Exception_PresentsError()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<AuthenticateUserRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network Error"));

        // Act
        await _command.Build().InvokeAsync("auth login bad-key");

        // Assert
        _presenterMock.Verify(x => x.PresentError("Network Error"), Times.Once);
    }

    [Fact(DisplayName = "Logout should clear vault and PresentSuccess.")]
    public async Task Logout_ClearsVault()
    {
        // Act
        await _command.Build().InvokeAsync("auth logout");

        // Assert
        _vaultMock.Verify(x => x.ClearAsync(It.IsAny<CancellationToken>()), Times.Once);
        _presenterMock.Verify(x => x.PresentSuccess("Auth_Logout_Success"), Times.Once);
    }
}

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

        // Fix mocks
        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(key =>
            key switch {
                "Cmd_Auth_Name" => "auth",
                "Cmd_Login_Name" => "login",
                "Cmd_Logout_Name" => "logout",
                "Arg_Key_Name" => "key",
                _ => key
            });
        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k);

        _command = new AuthCommand(
            _useCaseMock.Object,
            _vaultMock.Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            new Mock<ILogger<AuthCommand>>().Object);
    }

    [Fact(DisplayName = "Login should call UseCase and PresentSuccess.")]
    public async Task Login_Valid_LogsIn()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<AuthenticateUserRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticateUserResponse(true, "Welcome"));

        // Act
        await _command.Build().InvokeAsync("auth login my-key");

        // Assert
        _useCaseMock.Verify(x => x.ExecuteAsync(It.Is<AuthenticateUserRequest>(r => r.ApiKey == "my-key"), It.IsAny<CancellationToken>()), Times.Once);
        _presenterMock.Verify(x => x.PresentSuccess("Welcome"), Times.Once);
    }

    [Fact(DisplayName = "Logout should clear vault and PresentSuccess.")]
    public async Task Logout_Valid_LogsOut()
    {
        // Act
        await _command.Build().InvokeAsync("auth logout");

        // Assert
        _vaultMock.Verify(x => x.ClearAsync(It.IsAny<CancellationToken>()), Times.Once);
        _presenterMock.Verify(x => x.PresentSuccess(It.IsAny<string>()), Times.Once);
    }
}

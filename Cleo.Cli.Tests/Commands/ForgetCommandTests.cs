using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ForgetSession;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public sealed class ForgetCommandTests
{
    private readonly Mock<IForgetSessionUseCase> _useCaseMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly ForgetCommand _command;

    public ForgetCommandTests()
    {
        _useCaseMock = new Mock<IForgetSessionUseCase>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();

        // Fix mocks
        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(key =>
            key switch {
                "Cmd_Forget_Name" => "forget",
                "Arg_SessionId_Name" => "sessionId",
                "Forget_Success" => "Forgotten {0}",
                _ => key
            });
        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k);

        _command = new ForgetCommand(
            _useCaseMock.Object,
            _presenterMock.Object,
            _helpProviderMock.Object,
            new Mock<ILogger<ForgetCommand>>().Object);
    }

    [Fact(DisplayName = "Forget should call UseCase and PresentSuccess.")]
    public async Task Forget_Valid_ForgetsSession()
    {
        // Arrange
        var sessionId = new SessionId("sessions/123");

        // Act
        await _command.Build().InvokeAsync($"forget {sessionId}");

        // Assert
        _useCaseMock.Verify(x => x.ExecuteAsync(It.Is<ForgetSessionRequest>(r => r.Id == sessionId), It.IsAny<CancellationToken>()), Times.Once);
        _presenterMock.Verify(x => x.PresentSuccess(It.IsAny<string>()), Times.Once);
    }

    [Fact(DisplayName = "Forget should call PresentError on exception.")]
    public async Task Forget_Error_PresentsError()
    {
        // Arrange
        var sessionId = new SessionId("sessions/123");
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ForgetSessionRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Fail"));

        // Act
        await _command.Build().InvokeAsync($"forget {sessionId}");

        // Assert
        _presenterMock.Verify(x => x.PresentError("Fail"), Times.Once);
    }
}

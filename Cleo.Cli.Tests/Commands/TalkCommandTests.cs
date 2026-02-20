using Cleo.Cli.Commands;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.Correspond;
using Cleo.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public sealed class TalkCommandTests : IDisposable
{
    private readonly Mock<ICorrespondUseCase> _useCaseMock;
    private readonly Mock<IStatusPresenter> _presenterMock;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly Mock<ILogger<TalkCommand>> _loggerMock;
    private readonly TalkCommand _command;

    public TalkCommandTests()
    {
        _useCaseMock = new Mock<ICorrespondUseCase>();
        _presenterMock = new Mock<IStatusPresenter>();
        _helpProviderMock = new Mock<IHelpProvider>();
        _loggerMock = new Mock<ILogger<TalkCommand>>();

        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(key =>
            key switch {
                "Cmd_Talk_Name" => "talk",
                "Arg_SessionId_Name" => "sessionId",
                "Opt_Message_Aliases" => "--message,-m",
                _ => key
            });
        _helpProviderMock.Setup(x => x.GetCommandDescription(It.IsAny<string>())).Returns<string>(k => k + "_Desc");

        _command = new TalkCommand(_useCaseMock.Object, _presenterMock.Object, _helpProviderMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Given valid input, when running 'talk', then it should send message and present success.")]
    public async Task Talk_Valid_DisplaysSuccess()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<CorrespondRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CorrespondResponse(TestFactory.CreateSessionId("s1"), DateTimeOffset.UtcNow));

        // Act
        var exitCode = await _command.Build().InvokeAsync("talk sessions/s1 -m \"Hello\"");

        // Assert
        exitCode.Should().Be(0);
        _presenterMock.Verify(x => x.PresentMessageSent(), Times.Once);
    }

    [Fact(DisplayName = "Given an error, when running 'talk', then it should present error.")]
    public async Task Talk_Error_HandlesException()
    {
        // Arrange
        var exception = new Exception("Network unavailable");
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<CorrespondRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await _command.Build().InvokeAsync("talk sessions/s1 -m \"Hi\"");

        // Assert
        _presenterMock.Verify(x => x.PresentError("Network unavailable"), Times.Once);
        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), exception, (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}

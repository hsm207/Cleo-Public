using Cleo.Cli.Commands;
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
public class TalkCommandTests : IDisposable
{
    private readonly Mock<ICorrespondUseCase> _useCaseMock;
    private readonly Mock<ILogger<TalkCommand>> _loggerMock;
    private readonly TalkCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public TalkCommandTests()
    {
        _useCaseMock = new Mock<ICorrespondUseCase>();
        _loggerMock = new Mock<ILogger<TalkCommand>>();
        _command = new TalkCommand(_useCaseMock.Object, _loggerMock.Object);

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

    [Fact(DisplayName = "Given valid input, when running 'talk', then it should send message and display success.")]
    public async Task Talk_Valid_DisplaysSuccess()
    {
        // Arrange
        // CorrespondResponse(SessionId Id, DateTimeOffset SentAt)
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<CorrespondRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CorrespondResponse(TestFactory.CreateSessionId("s1"), DateTimeOffset.UtcNow));

        // Act
        var exitCode = await _command.Build().InvokeAsync("talk sessions/s1 -m \"Hello\"");

        // Assert
        exitCode.Should().Be(0);
        _stringWriter.ToString().Should().Contain("✅ Message sent!");
    }

    [Fact(DisplayName = "Given an error, when running 'talk', then it should handle exception.")]
    public async Task Talk_Error_HandlesException()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<CorrespondRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network unavailable"));

        // Act
        await _command.Build().InvokeAsync("talk sessions/s1 -m \"Hi\"");

        // Assert
        _stringWriter.ToString().Should().Contain("💔 Error: Network unavailable");
        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}

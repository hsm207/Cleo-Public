using Cleo.Cli.Commands;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ForgetSession;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class ForgetCommandTests : IDisposable
{
    private readonly Mock<IForgetSessionUseCase> _useCaseMock;
    private readonly Mock<ILogger<ForgetCommand>> _loggerMock;
    private readonly ForgetCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public ForgetCommandTests()
    {
        _useCaseMock = new Mock<IForgetSessionUseCase>();
        _loggerMock = new Mock<ILogger<ForgetCommand>>();
        _command = new ForgetCommand(_useCaseMock.Object, _loggerMock.Object);

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

    [Fact(DisplayName = "Given a session ID, when running 'forget', then it should remove the session and display success.")]
    public async Task Forget_Valid_RemovesSession()
    {
        // Arrange
        var sessionId = "session-123";
        _useCaseMock.Setup(x => x.ExecuteAsync(It.Is<ForgetSessionRequest>(r => r.Id.Value == sessionId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ForgetSessionResponse(new SessionId(sessionId)));

        // Act
        var exitCode = await _command.Build().InvokeAsync($"forget {sessionId}");

        // Assert
        exitCode.Should().Be(0);
        _stringWriter.ToString().Should().Contain($"🧹 Session {sessionId} removed");
    }

    [Fact(DisplayName = "Given an error, when running 'forget', then it should handle the exception.")]
    public async Task Forget_Error_HandlesException()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ForgetSessionRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Registry locked"));

        // Act
        await _command.Build().InvokeAsync("forget s1");

        // Assert
        _stringWriter.ToString().Should().Contain("Error: Registry locked");
        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}

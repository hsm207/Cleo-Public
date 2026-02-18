using Cleo.Cli.Commands;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ApprovePlan;
using Cleo.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

[Collection("ConsoleTests")]
public class ApproveCommandTests : IDisposable
{
    private readonly Mock<IApprovePlanUseCase> _useCaseMock;
    private readonly Mock<ILogger<ApproveCommand>> _loggerMock;
    private readonly ApproveCommand _command;
    private readonly StringWriter _stringWriter;
    private readonly TextWriter _originalOutput;

    public ApproveCommandTests()
    {
        _useCaseMock = new Mock<IApprovePlanUseCase>();
        _loggerMock = new Mock<ILogger<ApproveCommand>>();
        _command = new ApproveCommand(_useCaseMock.Object, _loggerMock.Object);

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

    [Fact(DisplayName = "Given valid inputs, when running 'approve', then it should approve the plan and display success.")]
    public async Task Approve_Valid_DisplaysSuccess()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ApprovePlanRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApprovePlanResponse(TestFactory.CreateSessionId("s1"), TestFactory.CreatePlanId("p1"), now));

        // Act
        var exitCode = await _command.Build().InvokeAsync("approve sessions/s1 plans/p1");

        // Assert
        exitCode.Should().Be(0);
        // Raw Truth: The CLI outputs the PlanId value directly. Since we relaxed validation,
        // "p1" is a valid PlanId, so the output will just contain "p1", not "plans/p1".
        // Wait, TestFactory.CreatePlanId("p1") creates a PlanId with value "p1".
        // The command outputs: $"✅ Plan {response.PlanId} approved..."
        // So expected string is "✅ Plan p1 approved".
        _stringWriter.ToString().Should().Contain("✅ Plan p1 approved");
    }

    [Fact(DisplayName = "Given an error, when running 'approve', then it should handle exception.")]
    public async Task Approve_Error_HandlesException()
    {
        // Arrange
        _useCaseMock.Setup(x => x.ExecuteAsync(It.IsAny<ApprovePlanRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Plan not found"));

        // Act
        await _command.Build().InvokeAsync("approve sessions/s1 plans/p1");

        // Assert
        _stringWriter.ToString().Should().Contain("💔 Error: Plan not found");
        _loggerMock.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Once);
    }
}

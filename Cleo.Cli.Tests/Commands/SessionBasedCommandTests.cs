using Cleo.Cli.Commands;
using Cleo.Core.UseCases.BrowseHistory;
using Cleo.Core.UseCases.RefreshPulse;
using Cleo.Core.UseCases.Correspond;
using Cleo.Core.UseCases.ForgetSession;
using Cleo.Core.UseCases.ApprovePlan;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.CommandLine;
using Xunit;

namespace Cleo.Cli.Tests.Commands;

public class SessionBasedCommandTests
{
    private readonly Mock<IBrowseHistoryUseCase> _browseHistoryMock;
    private readonly Mock<IRefreshPulseUseCase> _refreshPulseMock;
    private readonly Mock<ICorrespondUseCase> _correspondMock;
    private readonly Mock<IForgetSessionUseCase> _forgetSessionMock;
    private readonly Mock<IApprovePlanUseCase> _approvePlanMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;

    public SessionBasedCommandTests()
    {
        _browseHistoryMock = new Mock<IBrowseHistoryUseCase>();
        _refreshPulseMock = new Mock<IRefreshPulseUseCase>();
        _correspondMock = new Mock<ICorrespondUseCase>();
        _forgetSessionMock = new Mock<IForgetSessionUseCase>();
        _approvePlanMock = new Mock<IApprovePlanUseCase>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(new Mock<ILogger>().Object);
    }

    [Theory(DisplayName = "Given a session-based command, when viewing help, then the argument should be named 'sessionId'.")]
    [InlineData(typeof(ActivitiesCommand))]
    [InlineData(typeof(StatusCommand))]
    [InlineData(typeof(TalkCommand))]
    [InlineData(typeof(ForgetCommand))]
    [InlineData(typeof(ApproveCommand))]
    public void Build_ShouldUseSessionIdArgument(Type commandType)
    {
        // Act
        Command command = null;

        if (commandType == typeof(ActivitiesCommand))
        {
            command = new ActivitiesCommand(_browseHistoryMock.Object, new Mock<ILogger<ActivitiesCommand>>().Object).Build();
        }
        else if (commandType == typeof(StatusCommand))
        {
            command = new StatusCommand(_refreshPulseMock.Object, new Mock<ILogger<StatusCommand>>().Object).Build();
        }
        else if (commandType == typeof(TalkCommand))
        {
            command = new TalkCommand(_correspondMock.Object, new Mock<ILogger<TalkCommand>>().Object).Build();
        }
        else if (commandType == typeof(ForgetCommand))
        {
            command = new ForgetCommand(_forgetSessionMock.Object, new Mock<ILogger<ForgetCommand>>().Object).Build();
        }
        else if (commandType == typeof(ApproveCommand))
        {
            command = new ApproveCommand(_approvePlanMock.Object, new Mock<ILogger<ApproveCommand>>().Object).Build();
        }

        // Assert
        command.Should().NotBeNull();
        var argument = command!.Arguments.FirstOrDefault();
        argument.Should().NotBeNull();
        argument!.Name.Should().Be("sessionId");
    }
}

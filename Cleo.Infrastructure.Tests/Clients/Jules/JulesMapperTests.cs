using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules;

public class JulesMapperTests
{
    private readonly ISessionStatusMapper _statusMapper = new DefaultSessionStatusMapper();

    [Theory(DisplayName = "JulesMapper should map session status correctly.")]
    [MemberData(nameof(StatusScenarios))]
    public void ShouldMapSessionStatus(JulesSessionState inputState, SessionStatus expectedStatus)
    {
        var dto = new JulesSessionResponseDto(
            Name: "sessions/123",
            Id: "remote-123",
            State: inputState,
            Prompt: "Task prompt",
            SourceContext: new JulesSourceContextDto("org", new JulesGithubRepoContextDto("main")),
            Url: new Uri("https://dash"),
            RequirePlanApproval: true,
            AutomationMode: JulesAutomationMode.AutomationModeUnspecified,
            CreateTime: DateTimeOffset.UtcNow.ToString("O"),
            UpdateTime: null,
            Title: "My Task",
            Outputs: null
        );

        var session = JulesMapper.Map(dto, (TaskDescription)"Task", _statusMapper);

        Assert.Equal(expectedStatus, session.Pulse.Status);
    }

    public static TheoryData<JulesSessionState, SessionStatus> StatusScenarios => new()
    {
        { JulesSessionState.Queued, SessionStatus.StartingUp },
        { JulesSessionState.StartingUp, SessionStatus.StartingUp },
        { JulesSessionState.Planning, SessionStatus.Planning },
        { JulesSessionState.InProgress, SessionStatus.InProgress },
        { JulesSessionState.Paused, SessionStatus.Paused },
        { JulesSessionState.AwaitingUserFeedback, SessionStatus.AwaitingFeedback },
        { JulesSessionState.AwaitingPlanApproval, SessionStatus.AwaitingPlanApproval },
        { JulesSessionState.Completed, SessionStatus.Completed },
        { JulesSessionState.Failed, SessionStatus.Failed }
    };
}

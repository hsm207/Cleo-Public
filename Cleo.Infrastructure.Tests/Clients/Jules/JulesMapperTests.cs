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

    [Theory(DisplayName = "JulesMapper should map session status and vibe correctly.")]
    [MemberData(nameof(StatusScenarios))]
    public void ShouldMapSessionStatusAndVibe(JulesSessionState inputState, SessionStatus expectedStatus, string expectedVibeSnippet)
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
        Assert.Contains(expectedVibeSnippet, session.Pulse.Detail);
    }

    public static TheoryData<JulesSessionState, SessionStatus, string> StatusScenarios => new()
    {
        { JulesSessionState.Queued, SessionStatus.StartingUp, "spinning up... 🚀" },
        { JulesSessionState.StartingUp, SessionStatus.StartingUp, "spinning up... 🚀" },
        { JulesSessionState.Planning, SessionStatus.Planning, "mapping out her thoughts... 🧠" },
        { JulesSessionState.InProgress, SessionStatus.InProgress, "hard at work on your task! 🔨🔥" },
        // If Paused maps to SessionStatus.Paused, default message is "Session is Paused" unless overridden.
        // Let's assume standard behavior for now.
        { JulesSessionState.Paused, SessionStatus.Paused, "Session is Paused" },
        { JulesSessionState.AwaitingUserFeedback, SessionStatus.AwaitingFeedback, "needs your input to proceed. 🗣️" },
        { JulesSessionState.AwaitingPlanApproval, SessionStatus.AwaitingPlanApproval, "review and approve the plan! 📝✨" },
        { JulesSessionState.Completed, SessionStatus.Completed, "Current run finished. 🧘‍♀️💖" },
        { JulesSessionState.Failed, SessionStatus.Failed, "Something went wrong during execution. 🥀" }
    };
}

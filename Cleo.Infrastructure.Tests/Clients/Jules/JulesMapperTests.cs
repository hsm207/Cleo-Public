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

    [Fact(DisplayName = "JulesMapper should map session status correctly.")]
    public void ShouldMapSessionStatus()
    {
        // Using real DefaultSessionStatusMapper logic
        var dto = new JulesSessionResponseDto(
            Name: "sessions/123",
            Id: "remote-123",
            State: JulesSessionState.InProgress,
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

        Assert.Equal(SessionStatus.InProgress, session.Pulse.Status);
    }

    [Fact(DisplayName = "JulesMapper should map emoji detail for status.")]
    public void ShouldMapEmojiDetail()
    {
        var dto = new JulesSessionResponseDto(
            Name: "sessions/123",
            Id: "remote-123",
            State: JulesSessionState.Completed,
            Prompt: "Task prompt",
            SourceContext: new JulesSourceContextDto("org", new JulesGithubRepoContextDto("main")),
            Url: new Uri("https://dash"),
            RequirePlanApproval: true,
            AutomationMode: JulesAutomationMode.AutomationModeUnspecified,
            CreateTime: DateTimeOffset.UtcNow.ToString("O"),
            UpdateTime: null,
            Title: "Task",
            Outputs: null
        );

        var session = JulesMapper.Map(dto, (TaskDescription)"Task", _statusMapper);

        Assert.Equal(SessionStatus.Completed, session.Pulse.Status);
        Assert.Contains("Current run finished. 🧘‍♀️💖", session.Pulse.Detail);
    }
}

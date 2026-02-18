using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using Cleo.Tests.Common;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules.Mapping;

public class JulesMapperTests
{
    private static readonly DateTimeOffset TestTime = DateTimeOffset.UtcNow;
    private static readonly string TestTimeStr = TestTime.ToString("O", CultureInfo.InvariantCulture);
    private readonly ISessionStatusMapper _statusMapper = new DefaultSessionStatusMapper();

    [Fact(DisplayName = "JulesMapper should map session status correctly using the StatusMapper.")]
    public void JulesMapper_ShouldMap_Status()
    {
        var dto = new JulesSessionResponseDto(
            Name: "sessions/123",
            Id: "remote-123",
            State: JulesSessionState.InProgress,
            Prompt: "Task prompt",
            SourceContext: new JulesSourceContextDto("org", new JulesGithubRepoContextDto("sources/repo")),
            Url: new Uri("https://dash"),
            RequirePlanApproval: true,
            AutomationMode: JulesAutomationMode.AutomationModeUnspecified,
            CreateTime: DateTimeOffset.UtcNow.ToString("O"),
            UpdateTime: null,
            Title: "My Task",
            Outputs: null
        );

        var session = JulesMapper.Map(dto, _statusMapper);

        Assert.Equal(SessionStatus.InProgress, session.Pulse.Status);
    }

    [Fact(DisplayName = "JulesMapper should map 'Prompt' from DTO to Session 'Task' (Zero-Hollow Identity).")]
    public void JulesMapper_ShouldMap_PromptToTask()
    {
        var expectedTask = "Fix the universe";
        var dto = new JulesSessionResponseDto(
            Name: "sessions/123",
            Id: "remote-123",
            State: JulesSessionState.InProgress,
            Prompt: expectedTask,
            SourceContext: new JulesSourceContextDto("org", new JulesGithubRepoContextDto("sources/main"))
        );

        var session = JulesMapper.Map(dto, _statusMapper);

        Assert.Equal((TaskDescription)expectedTask, session.Task);
    }

    [Fact(DisplayName = "JulesMapper should Fail Fast (Throw) if Prompt is missing/invalid.")]
    public void JulesMapper_ShouldThrow_IfPromptInvalid()
    {
        var dto = new JulesSessionResponseDto(
            Name: "sessions/123",
            Id: "remote-123",
            State: JulesSessionState.InProgress,
            Prompt: "",
            SourceContext: new JulesSourceContextDto("org", new JulesGithubRepoContextDto("sources/main"))
        );

        Assert.Throws<ArgumentException>(() => JulesMapper.Map(dto, _statusMapper));
    }

    [Fact(DisplayName = "JulesMapper should heal dirty legacy remote data by applying prefixes.")]
    public void JulesMapper_ShouldHeal_DirtyRemoteData()
    {
        // Arrange
        // Simulate a "Dirty" response from a legacy API version or mock
        var dirtyId = TestFactory.Data.LegacySessionId; // "123"
        var dirtyRepo = TestFactory.Data.LegacyRepo;    // "user/repo"

        var dto = new JulesSessionResponseDto(
            Name: dirtyId,
            Id: "remote-123",
            State: JulesSessionState.InProgress,
            Prompt: "Task",
            SourceContext: new JulesSourceContextDto(dirtyRepo, new JulesGithubRepoContextDto("main")),
            Url: null,
            RequirePlanApproval: false,
            AutomationMode: JulesAutomationMode.AutomationModeUnspecified,
            CreateTime: TestTimeStr,
            UpdateTime: null,
            Title: "Title",
            Outputs: null
        );

        // Act
        var session = JulesMapper.Map(dto, _statusMapper);

        // Assert
        session.Id.Value.Should().Be($"sessions/{dirtyId}");
        session.Source.Repository.Should().Be($"sources/{dirtyRepo}");
    }

    [Fact(DisplayName = "Given a planGenerated DTO, PlanningActivityMapper should map it to a PlanningActivity.")]
    public void PlanningActivityMapper_ShouldMap_PlanGenerated()
    {
        // Arrange
        // JulesPlanStepDto(Id, Title, Description, Index)
        var steps = new List<JulesPlanStepDto>
        {
            new("step1", "Do thing", "Desc", 0)
        };
        // JulesPlanDto(Id, Steps, CreateTime)
        var plan = new JulesPlanDto("plans/plan-1", steps, TestTimeStr);
        var payload = new JulesPlanGeneratedPayloadDto(plan);
        var metadata = new JulesActivityMetadataDto("act-1", "rem-1", "desc", TestTimeStr, "agent", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.PlanningActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<PlanningActivity>().Subject;
        activity.PlanId.Value.Should().Be("plans/plan-1");
        activity.Steps.Should().HaveCount(1);
        activity.Steps.First().Title.Should().Be("Do thing");
        activity.Timestamp.Should().BeCloseTo(TestTime, TimeSpan.FromSeconds(1));

        activity.ExecutiveSummary.Should().Be("desc");
    }

    [Fact(DisplayName = "Given an agentMessaged DTO, AgentMessageActivityMapper should map it to a MessageActivity.")]
    public void AgentMessageActivityMapper_ShouldMap_AgentMessaged()
    {
        // Arrange
        var payload = new JulesAgentMessagedPayloadDto("Hello User");
        var metadata = new JulesActivityMetadataDto("act-2", "rem-2", "Message Summary", TestTimeStr, "agent", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.AgentMessageActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<MessageActivity>().Subject;
        activity.Text.Should().Be("Hello User");
        activity.Originator.Should().Be(ActivityOriginator.Agent);

        activity.ExecutiveSummary.Should().Be("Message Summary");
    }

    [Fact(DisplayName = "Given a userMessaged DTO, UserMessageActivityMapper should map it to a MessageActivity.")]
    public void UserMessageActivityMapper_ShouldMap_UserMessaged()
    {
        // Arrange
        var payload = new JulesUserMessagedPayloadDto("Hello Agent");
        var metadata = new JulesActivityMetadataDto("act-3", "rem-3", null, TestTimeStr, "user", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.UserMessageActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<MessageActivity>().Subject;
        activity.Text.Should().Be("Hello Agent");
        activity.Originator.Should().Be(ActivityOriginator.User);
    }

    [Fact(DisplayName = "Given a sessionFailed DTO, FailureActivityMapper should map it to a FailureActivity.")]
    public void FailureActivityMapper_ShouldMap_SessionFailed()
    {
        // Arrange
        var payload = new JulesSessionFailedPayloadDto("Critical Error");
        var metadata = new JulesActivityMetadataDto("act-fail", "rem-fail", "Failure Summary", TestTimeStr, "system", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.FailureActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<FailureActivity>().Subject;
        activity.Reason.Should().Be("Critical Error");
        activity.Originator.Should().Be(ActivityOriginator.System);

        activity.ExecutiveSummary.Should().Be("Failure Summary");
    }

    [Fact(DisplayName = "Given an unknown activity type, UnknownActivityMapper should map it to a generic ProgressActivity.")]
    public void UnknownActivityMapper_ShouldMap_Safely()
    {
        // Arrange
        var payload = new JulesUnknownPayloadDto("Weird", "{}");
        var metadata = new JulesActivityMetadataDto("act-unknown", "rem-unknown", "Strange event", TestTimeStr, "system", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.UnknownActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<ProgressActivity>().Subject;
        activity.Intent.Should().Contain("Unknown Activity Type: Weird"); // Mapped from 'Title'
        activity.Reasoning.Should().Contain("Raw JSON preserved"); // Mapped from 'Description'

        activity.ExecutiveSummary.Should().Be("Strange event");
    }

    [Fact(DisplayName = "Given a planApproved DTO, ApprovalActivityMapper should map it to an ApprovalActivity.")]
    public void ApprovalActivityMapper_ShouldMap_PlanApproved()
    {
        // Arrange
        var payload = new JulesPlanApprovedPayloadDto("plans/plan-123");
        var metadata = new JulesActivityMetadataDto("act-app", "rem-app", null, TestTimeStr, "user", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.ApprovalActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<ApprovalActivity>().Subject;
        activity.PlanId.Value.Should().Be("plans/plan-123");
        activity.Originator.Should().Be(ActivityOriginator.User);
    }

    [Fact(DisplayName = "ArtifactMappingHelper should map Media Artifacts correctly.")]
    public void ArtifactMappingHelper_ShouldMap_MediaArtifact()
    {
        // Arrange
        // Media data is a list of ArtifactDto
        var mediaDto = new JulesMediaDto("base64data", "image/png");
        var artifactDto = new JulesArtifactDto(null, mediaDto, null);
        var artifacts = new List<JulesArtifactDto> { artifactDto };

        // Act
        var payload = new JulesAgentMessagedPayloadDto("Look at this!");
        var metadata = new JulesActivityMetadataDto("act-media", "rem-media", null, TestTimeStr, "agent", artifacts);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.AgentMessageActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        result.Evidence.Should().HaveCount(1);
        var media = result.Evidence.First().Should().BeOfType<MediaArtifact>().Subject;
        media.MimeType.Should().Be("image/png");
        media.Data.Should().Be("base64data");
    }

    [Theory(DisplayName = "ActivityOriginatorMapper should map various role strings correctly.")]
    [InlineData("USER", ActivityOriginator.User)]
    [InlineData("user", ActivityOriginator.User)]
    [InlineData("AGENT", ActivityOriginator.Agent)]
    [InlineData("agent", ActivityOriginator.Agent)]
    [InlineData("SYSTEM", ActivityOriginator.System)]
    [InlineData("system", ActivityOriginator.System)]
    [InlineData("unknown", ActivityOriginator.User)] // Default
    [InlineData(null, ActivityOriginator.User)] // Default
    public void ActivityOriginatorMapper_ShouldMapCorrectly(string? input, ActivityOriginator expected)
    {
        var result = ActivityOriginatorMapper.Map(input);
        result.Should().Be(expected);
    }
}

using System.Globalization;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Clients.Jules.Mapping;

public class JulesMapperTests
{
    private static readonly DateTimeOffset TestTime = DateTimeOffset.UtcNow;
    private static readonly string TestTimeStr = TestTime.ToString("O", CultureInfo.InvariantCulture);

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
        var plan = new JulesPlanDto("plan-1", steps, TestTimeStr);
        var payload = new JulesPlanGeneratedPayloadDto(plan);
        var metadata = new JulesActivityMetadataDto("act-1", "rem-1", "desc", TestTimeStr, "agent", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.PlanningActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<PlanningActivity>().Subject;
        activity.PlanId.Should().Be("plan-1");
        activity.Steps.Should().HaveCount(1);
        activity.Steps.First().Title.Should().Be("Do thing");
        activity.Timestamp.Should().BeCloseTo(TestTime, TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Given an agentMessaged DTO, MessageActivityMapper should map it to a MessageActivity.")]
    public void MessageActivityMapper_ShouldMap_AgentMessaged()
    {
        // Arrange
        var payload = new JulesAgentMessagedPayloadDto("Hello User");
        var metadata = new JulesActivityMetadataDto("act-2", "rem-2", null, TestTimeStr, "agent", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.MessageActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<MessageActivity>().Subject;
        activity.Text.Should().Be("Hello User");
        activity.Originator.Should().Be(ActivityOriginator.Agent);
    }

    [Fact(DisplayName = "Given a userMessaged DTO, MessageActivityMapper should map it to a MessageActivity.")]
    public void MessageActivityMapper_ShouldMap_UserMessaged()
    {
        // Arrange
        var payload = new JulesUserMessagedPayloadDto("Hello Agent");
        var metadata = new JulesActivityMetadataDto("act-3", "rem-3", null, TestTimeStr, "user", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.MessageActivityMapper();

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
        var metadata = new JulesActivityMetadataDto("act-fail", "rem-fail", null, TestTimeStr, "system", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.FailureActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<FailureActivity>().Subject;
        activity.Reason.Should().Be("Critical Error");
        activity.Originator.Should().Be(ActivityOriginator.System);
    }

    [Fact(DisplayName = "Given an unknown activity type, UnknownActivityMapper should map it to a generic MessageActivity.")]
    public void UnknownActivityMapper_ShouldMap_Safely()
    {
        // Arrange
        var payload = new JulesProgressUpdatedPayloadDto("Weird", "Stuff");
        var metadata = new JulesActivityMetadataDto("act-unknown", "rem-unknown", "Strange event", TestTimeStr, "system", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.UnknownActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<MessageActivity>().Subject;
        activity.Text.Should().Contain("Unknown activity type received");
        activity.Text.Should().Contain("Strange event");
    }

    [Fact(DisplayName = "Given a planApproved DTO, ApprovalActivityMapper should map it to an ApprovalActivity.")]
    public void ApprovalActivityMapper_ShouldMap_PlanApproved()
    {
        // Arrange
        var payload = new JulesPlanApprovedPayloadDto("plan-123");
        var metadata = new JulesActivityMetadataDto("act-app", "rem-app", null, TestTimeStr, "user", null);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.ApprovalActivityMapper();

        // Act
        var result = mapper.Map(dto);

        // Assert
        var activity = result.Should().BeOfType<ApprovalActivity>().Subject;
        activity.PlanId.Should().Be("plan-123");
        activity.Originator.Should().Be(ActivityOriginator.User);
    }

    [Fact(DisplayName = "ArtifactMappingHelper should map Media Artifacts correctly.")]
    public void ArtifactMappingHelper_ShouldMap_MediaArtifact()
    {
        // Arrange
        var mediaDto = new JulesMediaDto("base64data", "image/png");
        var artifactDto = new JulesArtifactDto(null, mediaDto, null);
        var artifacts = new List<JulesArtifactDto> { artifactDto };

        // Act
        // This is static helper logic usually invoked by mappers.
        // We can test it via a concrete mapper usage or directly if public (it is internal).
        // Since we are in the same assembly via InternalsVisibleTo (or we invoke it via a mapper flow).
        // Let's use MessageActivityMapper which uses ArtifactMappingHelper.

        var payload = new JulesAgentMessagedPayloadDto("Look at this!");
        var metadata = new JulesActivityMetadataDto("act-media", "rem-media", null, TestTimeStr, "agent", artifacts);
        var dto = new JulesActivityDto(metadata, payload);

        var mapper = new Cleo.Infrastructure.Clients.Jules.Mapping.MessageActivityMapper();

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

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
}

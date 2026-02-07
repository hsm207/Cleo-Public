using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Jules;

public sealed class JulesMapperTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact(DisplayName = "JulesMapper should map PlanGenerated activity correctly.")]
    public void Map_PlanGenerated_ShouldReturnPlanningActivity()
    {
        var dto = CreateBaseDto() with {
            PlanGenerated = new PlanGeneratedDto(new PlanDto("plan-1", new[] {
                new PlanStepDto("s1", "Title", "Desc", 0)
            }))
        };

        var result = JulesMapper.Map(dto);

        result.Should().BeOfType<PlanningActivity>();
        var plan = (PlanningActivity)result;
        plan.Steps.Should().ContainSingle(s => s.Title == "Title");
    }

    [Fact(DisplayName = "JulesMapper should map ProgressUpdated activity correctly.")]
    public void Map_ProgressUpdated_ShouldReturnProgressActivity()
    {
        var dto = CreateBaseDto() with { ProgressUpdated = new object() };

        var result = JulesMapper.Map(dto);

        result.Should().BeOfType<ProgressActivity>();
    }

    [Fact(DisplayName = "JulesMapper should map SessionFailed activity correctly.")]
    public void Map_SessionFailed_ShouldReturnFailureActivity()
    {
        var dto = CreateBaseDto() with { SessionFailed = new SessionFailedDto("Quota Exceeded") };

        var result = JulesMapper.Map(dto);

        result.Should().BeOfType<FailureActivity>();
        ((FailureActivity)result).Reason.Should().Be("Quota Exceeded");
    }

    [Fact(DisplayName = "JulesMapper should map PlanApproved activity to a MessageActivity.")]
    public void Map_PlanApproved_ShouldReturnMessageActivity()
    {
        var dto = CreateBaseDto() with { PlanApproved = new PlanApprovedDto("plan-cake") };

        var result = JulesMapper.Map(dto);

        result.Should().BeOfType<MessageActivity>();
        ((MessageActivity)result).Text.Should().Contain("plan-cake");
    }

    [Fact(DisplayName = "JulesMapper should handle different originator strings.")]
    public async Task Map_ShouldHandleOriginators()
    {
        var userDto = CreateBaseDto() with { Originator = "user", MessageText = "Hi" };
        var agentDto = CreateBaseDto() with { Originator = "agent", MessageText = "Hi" };
        var systemDto = CreateBaseDto() with { Originator = "something-else", MessageText = "Hi" };

        JulesMapper.Map(userDto).Originator.Should().Be(ActivityOriginator.User);
        JulesMapper.Map(agentDto).Originator.Should().Be(ActivityOriginator.Agent);
        JulesMapper.Map(systemDto).Originator.Should().Be(ActivityOriginator.System);
    }

    [Fact(DisplayName = "JulesMapper should throw ArgumentNullException if DTO is null.")]
    public void Map_ShouldThrowOnNull()
    {
        Action act = () => JulesMapper.Map(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    private static JulesActivityDto CreateBaseDto() => new(
        "name", "id", Now, "agent", null, null, null, null, null, null);
}

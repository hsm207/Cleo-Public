using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Jules;

public sealed class JulesMapperTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    [Fact(DisplayName = "PlanningActivityMapper should map PlanGenerated activity correctly.")]
    public void Map_PlanGenerated_ShouldReturnPlanningActivity()
    {
        var sut = new PlanningActivityMapper();
        var dto = CreateBaseDto() with {
            PlanGenerated = new PlanGeneratedDto(new PlanDto("plan-1", new[] {
                new PlanStepDto("s1", "Title", "Desc", 0)
            }, Now))
        };

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<PlanningActivity>();
        var plan = (PlanningActivity)result;
        plan.Steps.Should().ContainSingle(s => s.Title == "Title");
    }

    [Fact(DisplayName = "ProgressActivityMapper should map ProgressUpdated activity correctly.")]
    public void Map_ProgressUpdated_ShouldReturnProgressActivity()
    {
        var sut = new ProgressActivityMapper();
        var dto = CreateBaseDto() with { ProgressUpdated = new ProgressUpdatedDto("Cooking", "Still at it") };

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<ProgressActivity>();
        ((ProgressActivity)result).Detail.Should().Be("Still at it");
    }

    [Fact(DisplayName = "FailureActivityMapper should map SessionFailed activity correctly.")]
    public void Map_SessionFailed_ShouldReturnFailureActivity()
    {
        var sut = new FailureActivityMapper();
        var dto = CreateBaseDto() with { SessionFailed = new SessionFailedDto("Quota Exceeded") };

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<FailureActivity>();
        ((FailureActivity)result).Reason.Should().Be("Quota Exceeded");
    }

    [Fact(DisplayName = "CompletionActivityMapper should map SessionCompleted activity correctly.")]
    public void Map_SessionCompleted_ShouldReturnCompletionActivity()
    {
        var sut = new CompletionActivityMapper();
        var dto = CreateBaseDto() with { SessionCompleted = new SessionCompletedDto() };

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<CompletionActivity>();
    }

    [Fact(DisplayName = "ApprovalActivityMapper should map PlanApproved activity correctly. ✅")]
    public void Map_PlanApproved_ShouldReturnApprovalActivity()
    {
        var sut = new ApprovalActivityMapper();
        var dto = CreateBaseDto() with { PlanApproved = new PlanApprovedDto("plan-cake") };

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<ApprovalActivity>();
        ((ApprovalActivity)result).PlanId.Should().Be("plan-cake");
    }

    [Fact(DisplayName = "MessageActivityMapper should handle userMessaged and agentMessaged fields.")]
    public void Map_ShouldHandleNewMessageFields()
    {
        var sut = new MessageActivityMapper();
        var userDto = CreateBaseDto() with { Originator = "user", UserMessaged = new UserMessagedDto("User Hi") };
        var agentDto = CreateBaseDto() with { Originator = "agent", AgentMessaged = new AgentMessagedDto("Agent Yo") };

        ((MessageActivity)sut.Map(userDto)).Text.Should().Be("User Hi");
        ((MessageActivity)sut.Map(agentDto)).Text.Should().Be("Agent Yo");
    }

    [Fact(DisplayName = "ArtifactMappingHelper should attach evidence to any activity type! 📎💎")]
    public void Map_ShouldAttachArtifactsToActivity()
    {
        var sut = new ProgressActivityMapper();
        var artifacts = new[] {
            new ArtifactDto(null, null, new BashOutputDto("ls", "out", 0)),
            new ArtifactDto(new ChangeSetDto("src", new GitPatchDto("diff", "base", null)), null, null)
        };
        var dto = CreateBaseDto() with { 
            ProgressUpdated = new ProgressUpdatedDto("T", "D"),
            Artifacts = artifacts
        };

        var result = sut.Map(dto);
        result.Evidence.Should().HaveCount(2);
        result.Evidence.Should().Contain(a => a is BashOutput);
        result.Evidence.Should().Contain(a => a is ChangeSet);
    }

    [Fact(DisplayName = "JulesMapper.Map should throw ArgumentNullException if DTO is null.")]
    public void Map_ShouldThrowOnNull()
    {
        var statusMapper = new DefaultSessionStatusMapper();
        Action act = () => JulesMapper.Map(null!, (TaskDescription)"t", statusMapper);
        act.Should().Throw<ArgumentNullException>();
    }

    private static JulesActivityDto CreateBaseDto() => new(
        "name", "id", "desc", Now, "agent", null, null, null, null, null, null, null, null);
}

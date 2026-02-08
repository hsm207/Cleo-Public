using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos;
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
        ((ProgressActivity)result).Detail.Should().Be("Cooking: Still at it");
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

    [Fact(DisplayName = "MessageActivityMapper should map PlanApproved activity to a MessageActivity.")]
    public void Map_PlanApproved_ShouldReturnMessageActivity()
    {
        var sut = new MessageActivityMapper();
        var dto = CreateBaseDto() with { PlanApproved = new PlanApprovedDto("plan-cake") };

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<MessageActivity>();
        ((MessageActivity)result).Text.Should().Contain("plan-cake");
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

    [Fact(DisplayName = "MessageActivityMapper should handle different originator strings.")]
    public void Map_ShouldHandleOriginators()
    {
        var sut = new MessageActivityMapper();
        var userDto = CreateBaseDto() with { Originator = "user", UserMessaged = new UserMessagedDto("Hi") };
        var agentDto = CreateBaseDto() with { Originator = "agent", AgentMessaged = new AgentMessagedDto("Hi") };
        var systemDto = CreateBaseDto() with { Originator = "something-else" };

        sut.Map(userDto).Originator.Should().Be(ActivityOriginator.User);
        sut.Map(agentDto).Originator.Should().Be(ActivityOriginator.Agent);
        sut.Map(systemDto).Originator.Should().Be(ActivityOriginator.System);
    }

    [Fact(DisplayName = "ResultActivityMapper should map Result activity correctly.")]
    public void Map_Result_ShouldReturnResultActivity()
    {
        var sut = new ResultActivityMapper();
        var dto = CreateBaseDto() with {
            Artifacts = new[] {
                new ArtifactDto(new ChangeSetDto("src", new GitPatchDto("diff", "base", null)), null, null)
            }
        };

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<ResultActivity>();
        var act = (ResultActivity)result;
        act.Patch.UniDiff.Should().Be("diff");
        
        // Exercise the 'false' branch of ResultActivityMapper.CanMap
        var emptyArtifactDto = CreateBaseDto() with { Artifacts = new ArtifactDto[] { new(null, null, null) } };
        sut.CanMap(emptyArtifactDto).Should().BeFalse();
    }

    [Fact(DisplayName = "ExecutionActivityMapper should map BashOutput activity correctly.")]
    public void Map_BashOutput_ShouldReturnExecutionActivity()
    {
        var sut = new ExecutionActivityMapper();
        var dto = CreateBaseDto() with {
            Artifacts = new[] {
                new ArtifactDto(null, null, new BashOutputDto("ls", "out", 0))
            }
        };

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<ExecutionActivity>();
        var act = (ExecutionActivity)result;
        act.Command.Should().Be("ls");
        act.Output.Should().Be("out");
        act.ExitCode.Should().Be(0);
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

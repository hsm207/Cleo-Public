/*
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Jules;

public sealed class JulesMapperTests
{
    private static readonly string Now = DateTimeOffset.UtcNow.ToString("O");

    [Fact(DisplayName = "PlanningActivityMapper should map PlanGenerated activity correctly.")]
    public void Map_PlanGenerated_ShouldReturnPlanningActivity()
    {
        var sut = new PlanningActivityMapper();
        var dto = JulesDtoTestFactory.Create(
            "name", "id", "desc", Now, "agent", 
            planGenerated: new JulesPlanGeneratedDto(new JulesPlanDto("plan-1", new[] {
                new JulesPlanStepDto("s1", "Title", "Desc", 0)
            }, Now)));

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
        var dto = JulesDtoTestFactory.Create(
            "name", "id", "desc", Now, "agent",
            progressUpdated: new JulesProgressUpdatedDto("Cooking", "Still at it"));

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<ProgressActivity>();
        ((ProgressActivity)result).Description.Should().Be("Still at it");
    }

    [Fact(DisplayName = "FailureActivityMapper should map SessionFailed activity correctly.")]
    public void Map_SessionFailed_ShouldReturnFailureActivity()
    {
        var sut = new FailureActivityMapper();
        var dto = JulesDtoTestFactory.Create(
            "name", "id", "desc", Now, "system",
            sessionFailed: new JulesSessionFailedDto("Quota Exceeded"));

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<FailureActivity>();
        ((FailureActivity)result).Reason.Should().Be("Quota Exceeded");
    }

    [Fact(DisplayName = "CompletionActivityMapper should map SessionCompleted activity correctly.")]
    public void Map_SessionCompleted_ShouldReturnCompletionActivity()
    {
        var sut = new CompletionActivityMapper();
        var dto = JulesDtoTestFactory.Create(
            "name", "id", "desc", Now, "system",
            sessionCompleted: new JulesSessionCompletedDto());

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<CompletionActivity>();
    }

    [Fact(DisplayName = "ApprovalActivityMapper should map PlanApproved activity correctly. ✅")]
    public void Map_PlanApproved_ShouldReturnApprovalActivity()
    {
        var sut = new ApprovalActivityMapper();
        var dto = JulesDtoTestFactory.Create(
            "name", "id", "desc", Now, "user",
            planApproved: new JulesPlanApprovedDto("plan-cake"));

        sut.CanMap(dto).Should().BeTrue();
        var result = sut.Map(dto);

        result.Should().BeOfType<ApprovalActivity>();
        ((ApprovalActivity)result).PlanId.Should().Be("plan-cake");
    }

    [Fact(DisplayName = "MessageActivityMapper should handle userMessaged and agentMessaged fields.")]
    public void Map_ShouldHandleNewMessageFields()
    {
        var sut = new MessageActivityMapper();
        var userDto = JulesDtoTestFactory.Create("n", "1", "d", Now, "user", userMessaged: new JulesUserMessagedDto("User Hi"));
        var agentDto = JulesDtoTestFactory.Create("n", "2", "d", Now, "agent", agentMessaged: new JulesAgentMessagedDto("Agent Yo"));

        ((MessageActivity)sut.Map(userDto)).Text.Should().Be("User Hi");
        ((MessageActivity)sut.Map(agentDto)).Text.Should().Be("Agent Yo");
    }

    [Fact(DisplayName = "ArtifactMappingHelper should attach evidence to any activity type! 📎💎")]
    public void Map_ShouldAttachArtifactsToActivity()
    {
        var sut = new ProgressActivityMapper();
        var artifacts = new List<JulesArtifactDto> {
            new JulesArtifactDto(null, null, new JulesBashOutputDto("ls", "out", 0)),
            new JulesArtifactDto(new JulesChangeSetDto("src", new JulesGitPatchDto("diff", "base", null)), null, null)
        };
        var dto = JulesDtoTestFactory.Create(
            "n", "1", "d", Now, "agent",
            progressUpdated: new JulesProgressUpdatedDto("T", "D"),
            artifacts: artifacts);

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
}
*/
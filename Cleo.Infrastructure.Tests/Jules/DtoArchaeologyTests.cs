using Cleo.Infrastructure.Clients.Jules.Dtos;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Jules;

public sealed class DtoArchaeologyTests
{
    [Fact(DisplayName = "Exhaustive check of all DTO properties for coverage.")]
    public void TouchAllDtoProperties()
    {
        var now = DateTimeOffset.UtcNow;
        
        // 1. Activity DTOs
        var planStep = new PlanStepDto("id", "Title", "Desc", 0);
        planStep.Id.Should().Be("id");
        planStep.Title.Should().Be("Title");
        planStep.Description.Should().Be("Desc");
        planStep.Index.Should().Be(0);

        var plan = new PlanDto("pid", new[] { planStep });
        plan.Id.Should().Be("pid");
        plan.Steps.Should().NotBeEmpty();

        var planGen = new PlanGeneratedDto(plan);
        planGen.Plan.Should().Be(plan);

        var planApp = new PlanApprovedDto("paid");
        planApp.PlanId.Should().Be("paid");

        var patch = new GitPatchDto("diff", "base");
        patch.UnidiffPatch.Should().Be("diff");
        patch.BaseCommitId.Should().Be("base");

        var changeSet = new ChangeSetDto(patch);
        changeSet.GitPatch.Should().Be(patch);

        var artifact = new ArtifactDto(changeSet);
        artifact.ChangeSet.Should().Be(changeSet);

        var fail = new SessionFailedDto("reason");
        fail.Reason.Should().Be("reason");

        var activity = new JulesActivityDto("n", "i", now, "o", planGen, planApp, "text", new[] { artifact }, new object(), fail);
        activity.Name.Should().Be("n");
        activity.Id.Should().Be("i");
        activity.CreateTime.Should().Be(now);
        activity.Originator.Should().Be("o");
        activity.PlanGenerated.Should().Be(planGen);
        activity.PlanApproved.Should().Be(planApp);
        activity.MessageText.Should().Be("text");
        activity.Artifacts.Should().ContainSingle();
        activity.ProgressUpdated.Should().NotBeNull();
        activity.SessionFailed.Should().Be(fail);

        var listAct = new ListActivitiesResponse(new[] { activity }, "token");
        listAct.Activities.Should().NotBeEmpty();
        listAct.NextPageToken.Should().Be("token");

        // 2. Session DTOs
        var github = new GithubRepoContextDto("main");
        github.StartingBranch.Should().Be("main");

        var source = new SourceContextDto("repo", github);
        source.Source.Should().Be("repo");
        source.GithubRepoContext.Should().Be(github);

        var session = new JulesSessionDto("sn", "si", "state", "prompt", source);
        session.Name.Should().Be("sn");
        session.Id.Should().Be("si");
        session.State.Should().Be("state");
        session.Prompt.Should().Be("prompt");
        session.SourceContext.Should().Be(source);
    }
}

using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
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

        var plan = new PlanDto("pid", new[] { planStep }, now);
        plan.Id.Should().Be("pid");
        plan.Steps.Should().NotBeEmpty();
        plan.CreateTime.Should().Be(now);

        var planGen = new PlanGeneratedDto(plan);
        planGen.Plan.Should().Be(plan);

        var planApp = new PlanApprovedDto("paid");
        planApp.PlanId.Should().Be("paid");

        var userMsg = new UserMessagedDto("hi");
        userMsg.UserMessage.Should().Be("hi");

        var agentMsg = new AgentMessagedDto("yo");
        agentMsg.AgentMessage.Should().Be("yo");

        var progress = new ProgressUpdatedDto("title", "desc");
        progress.Title.Should().Be("title");
        progress.Description.Should().Be("desc");

        var patch = new GitPatchDto("diff", "base", "feat: msg");
        patch.UnidiffPatch.Should().Be("diff");
        patch.BaseCommitId.Should().Be("base");
        patch.SuggestedCommitMessage.Should().Be("feat: msg");

        var changeSet = new ChangeSetDto("src", patch);
        changeSet.Source.Should().Be("src");
        changeSet.GitPatch.Should().Be(patch);

        var media = new MediaDto("data", "image/png");
        media.Data.Should().Be("data");
        media.MimeType.Should().Be("image/png");

        var bash = new BashOutputDto("ls", "out", 0);
        bash.Command.Should().Be("ls");
        bash.Output.Should().Be("out");
        bash.ExitCode.Should().Be(0);

        var artifact = new ArtifactDto(changeSet, media, bash);
        artifact.ChangeSet.Should().Be(changeSet);
        artifact.Media.Should().Be(media);
        artifact.BashOutput.Should().Be(bash);

        var fail = new SessionFailedDto("reason");
        fail.Reason.Should().Be("reason");

        var completed = new SessionCompletedDto();

        var activity = new JulesActivityDto("n", "i", "desc", now, "o", new[] { artifact }, planGen, planApp, userMsg, agentMsg, progress, completed, fail);
        activity.Name.Should().Be("n");
        activity.Id.Should().Be("i");
        activity.Description.Should().Be("desc");
        activity.CreateTime.Should().Be(now);
        activity.Originator.Should().Be("o");
        activity.Artifacts.Should().ContainSingle();
        activity.PlanGenerated.Should().Be(planGen);
        activity.PlanApproved.Should().Be(planApp);
        activity.UserMessaged.Should().Be(userMsg);
        activity.AgentMessaged.Should().Be(agentMsg);
        activity.ProgressUpdated.Should().Be(progress);
        activity.SessionCompleted.Should().Be(completed);
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

        var session = new JulesSessionResponse("sn", "si", "state", "prompt", source, new Uri("https://jules.com"), true, "AUTO_CREATE_PR", now, now);
        session.Name.Should().Be("sn");
        session.Id.Should().Be("si");
        session.State.Should().Be("state");
        session.Prompt.Should().Be("prompt");
        session.SourceContext.Should().Be(source);
        session.Url.Should().Be(new Uri("https://jules.com"));
        session.RequirePlanApproval.Should().BeTrue();
        session.AutomationMode.Should().Be("AUTO_CREATE_PR");
        session.CreateTime.Should().Be(now);
        session.UpdateTime.Should().Be(now);
    }
}

using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using System.Linq;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// Provides utility methods for mapping between Jules API Data Transfer Objects (DTOs) 
/// and core domain entities and value objects.
/// </summary>
internal static class JulesMapper
{
    public static Session Map(JulesSessionResponse dto, TaskDescription originalTask, ISessionStatusMapper statusMapper)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(statusMapper);

        var pulse = MapPulse(dto, statusMapper);
        
        var session = new Session(
            new SessionId(dto.Name),
            originalTask,
            new SourceContext(dto.SourceContext.Source, dto.SourceContext.GithubRepoContext?.StartingBranch ?? string.Empty),
            pulse,
            dto.Url
        );

        // Map the formal Pull Request output if it exists 💎🎁
        var changeSetOutput = dto.Outputs?.FirstOrDefault(o => o.ChangeSet?.GitPatch != null);
        if (changeSetOutput != null)
        {
            var patch = changeSetOutput.ChangeSet!.GitPatch!;
            var source = changeSetOutput.ChangeSet.Source ?? "remote-source";
            var evidence = new List<Artifact> { new ChangeSet(source, new GitPatch(patch.UnidiffPatch ?? string.Empty, "remote", null)) };
            
            // Attach the patch to a synthetic completion activity to mark delivery
            session.AddActivity(new CompletionActivity(
                $"output-{session.Id.Value}", 
                DateTimeOffset.UtcNow, 
                evidence));
        }

        var prOutput = dto.Outputs?.FirstOrDefault(o => o.PullRequest != null);
        if (prOutput?.PullRequest != null)
        {
            var pr = prOutput.PullRequest;
            session.SetPullRequest(new PullRequest(pr.Url, pr.Title, pr.Description));
        }

        return session;
    }

    public static SessionPulse MapPulse(JulesSessionResponse dto, ISessionStatusMapper statusMapper)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(statusMapper);

        var status = statusMapper.Map(dto.State);
        var detail = GetFriendlyStatusDetail(status, dto.State);
        return new SessionPulse(status, detail);
    }

    private static string GetFriendlyStatusDetail(SessionStatus status, string rawState) => status switch
    {
        SessionStatus.StartingUp => "The collaboration is spinning up... 🚀",
        SessionStatus.Planning => "Jules is mapping out her thoughts... 🧠",
        SessionStatus.AwaitingPlanApproval => "Waiting for you to review and approve the plan! 📝✨",
        SessionStatus.AwaitingFeedback => "Jules needs your input to proceed. 🗣️",
        SessionStatus.InProgress => "Jules is hard at work on your task! 🔨🔥",
        SessionStatus.Completed => "Current run finished. 🧘‍♀️💖",
        SessionStatus.Abandoned => "Session closed without making any changes. ⌛️🥀",
        SessionStatus.Failed => "Something went wrong during execution. 🥀",
        _ => $"Session is {rawState}"
    };
}

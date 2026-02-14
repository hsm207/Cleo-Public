using Cleo.Core.Domain.Entities;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;
using Cleo.Infrastructure.Clients.Jules.Mapping;
using System.Globalization;
using System.Linq;

namespace Cleo.Infrastructure.Clients.Jules;

/// <summary>
/// Provides utility methods for mapping between Jules API Data Transfer Objects (DTOs) 
/// and core domain entities and value objects.
/// </summary>
internal static class JulesMapper
{
    public static Session Map(JulesSessionResponseDto dto, TaskDescription originalTask, ISessionStatusMapper statusMapper)
    {
        var pulse = MapPulse(dto, statusMapper);
        var automationMode = dto.AutomationMode == JulesAutomationMode.AutoCreatePr ? AutomationMode.AutoCreatePr : AutomationMode.Unspecified;
        
        var session = new Session(
            new SessionId(dto.Name),
            dto.Id,
            originalTask,
            new SourceContext(dto.SourceContext.Source, dto.SourceContext.GithubRepoContext?.StartingBranch ?? string.Empty),
            pulse,
            ParseDateTime(dto.CreateTime),
            dto.UpdateTime != null ? ParseDateTime(dto.UpdateTime) : null,
            dto.Title,
            dto.RequirePlanApproval,
            automationMode,
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
                "remote-output-id",
                DateTimeOffset.UtcNow, 
                ActivityOriginator.System,
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

    public static SessionPulse MapPulse(JulesSessionResponseDto dto, ISessionStatusMapper statusMapper)
    {
        var status = statusMapper.Map(dto.State);
        return new SessionPulse(status);
    }

    private static DateTimeOffset ParseDateTime(string? date)
    {
        if (string.IsNullOrEmpty(date)) return DateTimeOffset.UtcNow;
        return DateTimeOffset.Parse(date, CultureInfo.InvariantCulture);
    }
}

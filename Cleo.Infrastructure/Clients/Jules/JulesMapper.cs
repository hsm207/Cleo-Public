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
    public static Session Map(JulesSessionResponseDto dto, ISessionStatusMapper statusMapper)
    {
        var pulse = MapPulse(dto, statusMapper);
        var automationMode = dto.AutomationMode == JulesAutomationMode.AutoCreatePr ? AutomationMode.AutoCreatePr : AutomationMode.Unspecified;

        // High-Fidelity Mapping: Use 'Prompt' as the authoritative TaskDescription.
        // Self-Healing Strategy 🩹: Ensure incoming API IDs conform to new Domain Prefixes.

        var safeSource = EnsurePrefix(dto.SourceContext.Source, "sources/");
        var safeSessionId = EnsurePrefix(dto.Name, "sessions/");

        var session = new Session(
            new SessionId(safeSessionId),
            dto.Id,
            (TaskDescription)dto.Prompt,
            new SourceContext(safeSource, dto.SourceContext.GithubRepoContext?.StartingBranch ?? string.Empty),
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
            // High-Fidelity Mandate: We strictly enforce presence of Description and Topology. 💎
            session.SetPullRequest(new PullRequest(
                pr.Url,
                pr.Title,
                pr.Description,
                pr.HeadRef,
                pr.BaseRef));
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

    /// <summary>
    /// Ensures that the provided value starts with the specified prefix.
    /// If not, it prepends the prefix (Self-Healing).
    /// </summary>
    private static string EnsurePrefix(string value, string prefix)
    {
        if (string.IsNullOrWhiteSpace(value)) return value; // Let Domain throw if empty
        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? value : $"{prefix}{value}";
    }
}

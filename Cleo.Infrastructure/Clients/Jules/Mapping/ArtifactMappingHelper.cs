using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

internal static class ArtifactMappingHelper
{
    public static IReadOnlyCollection<Artifact> MapArtifacts(IReadOnlyList<ArtifactDto>? dtos)
    {
        if (dtos == null) return Array.Empty<Artifact>();

        var artifacts = new List<Artifact>();
        foreach (var dto in dtos)
        {
            if (dto.ChangeSet?.GitPatch != null)
            {
                var patch = dto.ChangeSet.GitPatch;
                artifacts.Add(new ChangeSet(
                    dto.ChangeSet.Source ?? "unknown-source",
                    new GitPatch(
                        patch.UnidiffPatch ?? string.Empty, 
                        patch.BaseCommitId ?? string.Empty, 
                        patch.SuggestedCommitMessage)));
            }
            else if (dto.BashOutput != null)
            {
                artifacts.Add(new BashOutput(
                    dto.BashOutput.Command ?? string.Empty, 
                    dto.BashOutput.Output ?? string.Empty, 
                    dto.BashOutput.ExitCode ?? 0));
            }
            else if (dto.Media != null)
            {
                artifacts.Add(new VisualSnapshot(
                    dto.Media.MimeType ?? string.Empty, 
                    dto.Media.Data ?? string.Empty));
            }
        }

        return artifacts.AsReadOnly();
    }
}

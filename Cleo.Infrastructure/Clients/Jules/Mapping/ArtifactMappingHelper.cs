using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Clients.Jules.Dtos.Responses;

namespace Cleo.Infrastructure.Clients.Jules.Mapping;

internal static class ArtifactMappingHelper
{
    public static IReadOnlyCollection<Artifact> MapArtifacts(ArtifactDto[]? dtos)
    {
        if (dtos == null) return Array.Empty<Artifact>();

        var artifacts = new List<Artifact>();
        foreach (var dto in dtos)
        {
            if (dto.ChangeSet?.GitPatch != null)
            {
                var patch = dto.ChangeSet.GitPatch;
                artifacts.Add(new CodeProposal(new SolutionPatch(
                    patch.UnidiffPatch ?? string.Empty, 
                    patch.BaseCommitId ?? string.Empty, 
                    patch.SuggestedCommitMessage)));
            }
            else if (dto.BashOutput != null)
            {
                artifacts.Add(new CommandEvidence(
                    dto.BashOutput.Command ?? string.Empty, 
                    dto.BashOutput.Output ?? string.Empty, 
                    dto.BashOutput.ExitCode));
            }
            else if (dto.Media != null)
            {
                artifacts.Add(new MediaEvidence(
                    dto.Media.MimeType ?? string.Empty, 
                    dto.Media.Data ?? string.Empty));
            }
        }

        return artifacts.AsReadOnly();
    }
}

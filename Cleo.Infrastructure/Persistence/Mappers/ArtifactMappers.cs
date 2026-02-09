using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class CommandEvidenceMapper : IArtifactPersistenceMapper
{
    public string TypeKey => "COMMAND";
    public bool CanHandle(Artifact artifact) => artifact is CommandEvidence;

    public string Serialize(Artifact artifact)
    {
        var command = (CommandEvidence)artifact;
        return JsonSerializer.Serialize(new CommandPayloadDto(command.Command, command.Output, command.ExitCode));
    }

    public Artifact Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<CommandPayloadDto>(json);
        return new CommandEvidence(dto?.Command ?? "", dto?.Output ?? "", dto?.ExitCode ?? -1);
    }

    private sealed record CommandPayloadDto(string Command, string Output, int ExitCode);
}

internal sealed class CodeProposalMapper : IArtifactPersistenceMapper
{
    public string TypeKey => "PATCH";
    public bool CanHandle(Artifact artifact) => artifact is CodeProposal;

    public string Serialize(Artifact artifact)
    {
        var proposal = (CodeProposal)artifact;
        return JsonSerializer.Serialize(new PatchPayloadDto(
            proposal.Patch.UniDiff, 
            proposal.Patch.BaseCommitId, 
            proposal.Patch.SuggestedCommitMessage));
    }

    public Artifact Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<PatchPayloadDto>(json);
        return new CodeProposal(new SolutionPatch(
            dto?.UniDiff ?? "", 
            dto?.BaseCommitId ?? "", 
            dto?.SuggestedCommitMessage));
    }

    private sealed record PatchPayloadDto(string UniDiff, string BaseCommitId, string? SuggestedCommitMessage);
}

internal sealed class MediaEvidenceMapper : IArtifactPersistenceMapper
{
    public string TypeKey => "MEDIA";
    public bool CanHandle(Artifact artifact) => artifact is MediaEvidence;

    public string Serialize(Artifact artifact)
    {
        var media = (MediaEvidence)artifact;
        return JsonSerializer.Serialize(new MediaPayloadDto(media.MimeType, media.Data));
    }

    public Artifact Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<MediaPayloadDto>(json);
        return new MediaEvidence(dto?.MimeType ?? "", dto?.Data ?? "");
    }

    private sealed record MediaPayloadDto(string MimeType, string Data);
}

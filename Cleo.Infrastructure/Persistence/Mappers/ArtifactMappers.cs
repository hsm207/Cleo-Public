using System.Text.Json;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Infrastructure.Persistence.Internal;

namespace Cleo.Infrastructure.Persistence.Mappers;

internal sealed class BashOutputMapper : IArtifactPersistenceMapper
{
    public string TypeKey => "BASHOUTPUT";
    public bool CanHandle(Artifact artifact) => artifact is BashOutput;

    public string Serialize(Artifact artifact)
    {
        var bash = (BashOutput)artifact;
        return JsonSerializer.Serialize(new BashPayloadDto(bash.Command, bash.Output, bash.ExitCode));
    }

    public Artifact Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<BashPayloadDto>(json);
        return new BashOutput(dto?.Command ?? "", dto?.Output ?? "", dto?.ExitCode ?? -1);
    }

    private sealed record BashPayloadDto(string Command, string Output, int ExitCode);
}

internal sealed class ChangeSetMapper : IArtifactPersistenceMapper
{
    public string TypeKey => "CHANGESET";
    public bool CanHandle(Artifact artifact) => artifact is ChangeSet;

    public string Serialize(Artifact artifact)
    {
        var changeSet = (ChangeSet)artifact;
        return JsonSerializer.Serialize(new ChangeSetPayloadDto(
            changeSet.Source,
            changeSet.Patch.UniDiff,
            changeSet.Patch.BaseCommitId,
            changeSet.Patch.SuggestedCommitMessage));
    }

    public Artifact Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<ChangeSetPayloadDto>(json);
        return new ChangeSet(
            dto?.Source ?? "unknown", 
            new GitPatch(
                dto?.UniDiff ?? "", 
                dto?.BaseCommitId ?? "", 
                dto?.SuggestedCommitMessage));
    }

    private sealed record ChangeSetPayloadDto(string Source, string UniDiff, string BaseCommitId, string? SuggestedCommitMessage);
}

internal sealed class MediaMapper : IArtifactPersistenceMapper
{
    public string TypeKey => "MEDIA"; 
    public bool CanHandle(Artifact artifact) => artifact is MediaArtifact;

    public string Serialize(Artifact artifact)
    {
        var media = (MediaArtifact)artifact;
        return JsonSerializer.Serialize(new MediaPayloadDto(media.MimeType, media.Data));
    }

    public Artifact Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<MediaPayloadDto>(json);
        return new MediaArtifact(dto?.MimeType ?? "", dto?.Data ?? "");
    }

    private sealed record MediaPayloadDto(string MimeType, string Data);
}

namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// A set of code modifications targeting a specific source.
/// </summary>
public record ChangeSet : Artifact
{
    public string Source { get; init; }
    public GitPatch Patch { get; init; }

    public ChangeSet(string source, GitPatch patch)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("Source cannot be empty.", nameof(source));
        }

        ArgumentNullException.ThrowIfNull(patch);

        Source = source;
        Patch = patch;
    }

    public override string GetSummary() => $"📦 ChangeSet: Produced patch for '{Source}'";
}

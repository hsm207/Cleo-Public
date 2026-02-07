namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents the tangible code solution produced by Jules.
/// </summary>
public record SolutionPatch
{
    public string UniDiff { get; init; }
    public string BaseCommitId { get; init; }
    public string? SuggestedCommitMessage { get; init; }

    public SolutionPatch(string uniDiff, string baseCommitId, string? suggestedCommitMessage = null)
    {
        if (string.IsNullOrWhiteSpace(uniDiff))
        {
            throw new ArgumentException("UniDiff content cannot be empty.", nameof(uniDiff));
        }

        if (string.IsNullOrWhiteSpace(baseCommitId))
        {
            throw new ArgumentException("Base commit identifier cannot be empty.", nameof(baseCommitId));
        }

        UniDiff = uniDiff;
        BaseCommitId = baseCommitId;
        SuggestedCommitMessage = suggestedCommitMessage;
    }
}

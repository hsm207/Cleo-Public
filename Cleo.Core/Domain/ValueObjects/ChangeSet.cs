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

    /// <summary>
    /// Returns a human-friendly summary of the ChangeSet.
    /// RFC 009: Implements Signal-to-Noise Policy by reporting Impact Magnitude
    /// instead of an exhaustive file list when the impact is significant.
    /// </summary>
    public override string GetSummary()
    {
        var files = Patch.GetModifiedFiles();
        var fileSummary = GetNarrativeFileSummary(files);

        var shortSha = Patch.BaseCommitId.Length >= 7
            ? Patch.BaseCommitId[..7]
            : Patch.BaseCommitId;

        return $"ChangeSet [{shortSha}]: {fileSummary}";
    }

    private static string GetNarrativeFileSummary(IReadOnlyList<string> files)
    {
        const int NarrativeThreshold = 5;

        if (files.Count == 0) return "Produced patch";

        if (files.Count <= NarrativeThreshold)
        {
            return $"Updated [{string.Join(", ", files)}]";
        }

        // RFC 009: Impact Magnitude Summarization
        // Identify common prefixes to provide a semantic hint (e.g., "Cleo.Core")
        var commonPath = ExtractCommonPath(files);
        var locationHint = string.IsNullOrEmpty(commonPath) ? "files" : $"{commonPath}/*";

        return $"{files.Count} {locationHint} modified";
    }

    private static string ExtractCommonPath(IReadOnlyList<string> files)
    {
        if (files.Count == 0) return string.Empty;

        var matchingChars = files[0];

        foreach (var file in files.Skip(1))
        {
            var length = 0;
            var maxLen = Math.Min(matchingChars.Length, file.Length);
            while (length < maxLen && matchingChars[length] == file[length])
            {
                length++;
            }
            matchingChars = matchingChars[..length];
        }

        var lastSlash = matchingChars.LastIndexOf('/');
        if (lastSlash > 0)
        {
            return matchingChars[..lastSlash];
        }

        return string.Empty;
    }
}

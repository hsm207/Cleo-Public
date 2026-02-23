using System.IO.Hashing;
using System.Text;
using System.Text.RegularExpressions;

namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// A high-fidelity representation of a Git patch.
/// </summary>
public record GitPatch
{
    private static readonly Regex FileHeaderRegex = new(@"^\+\+\+\s+b/(.*)$", RegexOptions.Multiline | RegexOptions.Compiled);

    public string UniDiff { get; init; }
    public string BaseCommitId { get; init; }
    public string? SuggestedCommitMessage { get; init; }
    public string Fingerprint { get; init; }

    // Enforce Static Factory Pattern
    private GitPatch(string uniDiff, string baseCommitId, string fingerprint, string? suggestedCommitMessage = null)
    {
        ArgumentNullException.ThrowIfNull(uniDiff);
        ArgumentNullException.ThrowIfNull(baseCommitId);
        ArgumentNullException.ThrowIfNull(fingerprint);

        // Note: We allow an empty UniDiff to support agent "startup" heartbeats. 
        // In these cases, the agent may report an attached ChangeSet before any physical
        // code changes have been synthesized. 🤖✨
        if (string.IsNullOrWhiteSpace(baseCommitId))
        {
            throw new ArgumentException("Base commit identifier cannot be empty.", nameof(baseCommitId));
        }

        if (string.IsNullOrWhiteSpace(fingerprint))
        {
            throw new ArgumentException("Fingerprint cannot be empty.", nameof(fingerprint));
        }

        UniDiff = uniDiff;
        BaseCommitId = baseCommitId;
        SuggestedCommitMessage = suggestedCommitMessage;
        Fingerprint = fingerprint;
    }

    /// <summary>
    /// Creates a new GitPatch from a raw API source (e.g., Diff).
    /// Always calculates the fingerprint.
    /// </summary>
    public static GitPatch FromApi(string uniDiff, string baseCommitId, string? suggestedCommitMessage = null)
    {
        // Safe check for uniDiff before passing to CalculateFingerprint is handled by 'uniDiff ?? string.Empty'
        // But for the constructor, we must ensure uniDiff is not null if the constructor throws ArgumentNullException.
        // However, 'uniDiff ?? string.Empty' in CalculateFingerprint handles the hash.
        // The constructor call needs a non-null string.
        var safeDiff = uniDiff ?? string.Empty;
        var fingerprint = CalculateFingerprint(safeDiff);
        return new GitPatch(safeDiff, baseCommitId, fingerprint, suggestedCommitMessage);
    }

    /// <summary>
    /// Restores a GitPatch from persistence or a trusted source.
    /// Strictly expects a valid fingerprint.
    /// </summary>
    public static GitPatch Restore(string uniDiff, string baseCommitId, string fingerprint, string? suggestedCommitMessage = null)
    {
        return new GitPatch(uniDiff, baseCommitId, fingerprint, suggestedCommitMessage);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Fingerprints are displayed in lowercase hex.")]
    private static string CalculateFingerprint(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = XxHash128.Hash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Extracts the unique filenames modified in this patch.
    /// </summary>
    public IReadOnlyList<string> GetModifiedFiles()
    {
        var matches = FileHeaderRegex.Matches(UniDiff);
        return matches
            .Select(m => m.Groups[1].Value.Trim())
            .Distinct()
            .ToList()
            .AsReadOnly();
    }
}

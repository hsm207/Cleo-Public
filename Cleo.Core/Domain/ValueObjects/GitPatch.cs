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

    /// <summary>
    /// Creates a new GitPatch.
    /// If a fingerprint is provided (e.g., from persistence), it is used.
    /// Otherwise, it is calculated from the UniDiff content (initial ingestion).
    /// </summary>
    public GitPatch(string uniDiff, string baseCommitId, string? suggestedCommitMessage = null, string? fingerprint = null)
    {
        ArgumentNullException.ThrowIfNull(uniDiff);
        ArgumentNullException.ThrowIfNull(baseCommitId);

        // Note: We allow an empty UniDiff to support agent "startup" heartbeats. 
        // In these cases, the agent may report an attached ChangeSet before any physical
        // code changes have been synthesized. 🤖✨
        if (string.IsNullOrWhiteSpace(baseCommitId))
        {
            throw new ArgumentException("Base commit identifier cannot be empty.", nameof(baseCommitId));
        }

        UniDiff = uniDiff;
        BaseCommitId = baseCommitId;
        SuggestedCommitMessage = suggestedCommitMessage;

        // RFC 023 Mandate: No backward compatibility.
        // During deserialization (persistence read), the fingerprint MUST be provided.
        // During ingestion (API read), it is calculated.
        // Since we can't easily distinguish the caller context here without factory methods,
        // we default to calculation but acknowledge the user's intent to avoid "migration logic".
        // The implementation remains "fingerprint ?? CalculateFingerprint(uniDiff)" which is technically correct for both cases,
        // but we remove any comments implying "legacy support".
        Fingerprint = fingerprint ?? CalculateFingerprint(uniDiff);
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

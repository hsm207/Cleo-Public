namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// A media artifact produced by the session (e.g., image, video, audio).
/// </summary>
public record MediaArtifact : Artifact
{
    public string MimeType { get; init; }
    public string Data { get; init; }

    public MediaArtifact(string mimeType, string data)
    {
        ArgumentNullException.ThrowIfNull(mimeType);

        if (string.IsNullOrWhiteSpace(mimeType))
        {
            throw new ArgumentException("MimeType cannot be empty.", nameof(mimeType));
        }

        MimeType = mimeType;
        Data = data ?? string.Empty;
    }

    public override string GetSummary()
    {
        return $"Media: Produced '{MimeType}'";
    }
}

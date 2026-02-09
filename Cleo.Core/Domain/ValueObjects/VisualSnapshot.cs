namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// A media output produced by the session.
/// </summary>
public record VisualSnapshot : Artifact
{
    public string MimeType { get; init; }
    public string Data { get; init; }

    public VisualSnapshot(string mimeType, string data)
    {
        ArgumentNullException.ThrowIfNull(mimeType);

        if (string.IsNullOrWhiteSpace(mimeType))
        {
            throw new ArgumentException("MimeType cannot be empty.", nameof(mimeType));
        }

        MimeType = mimeType;
        Data = data ?? string.Empty;
    }

    public override string GetSummary() => $"🖼️ Media: Produced '{MimeType}'";
}

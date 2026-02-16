namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// A high-level deliverable representing a formal code submission.
/// </summary>
public record PullRequest
{
    public Uri Url { get; init; }
    public string Title { get; init; }
    public string? Description { get; init; }
    public string? HeadRef { get; init; }
    public string? BaseRef { get; init; }

    public PullRequest(
        Uri url,
        string title,
        string? description = null,
        string? headRef = null,
        string? baseRef = null)
    {
        ArgumentNullException.ThrowIfNull(url);
        
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Pull request title cannot be empty.", nameof(title));
        }

        Url = url;
        Title = title;
        Description = description;
        HeadRef = headRef;
        BaseRef = baseRef;
    }
}

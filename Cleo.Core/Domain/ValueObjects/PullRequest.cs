namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// A high-level deliverable representing a formal code submission.
/// </summary>
public record PullRequest
{
    public Uri Url { get; init; }
    public string Title { get; init; }
    public string? Description { get; init; }

    public PullRequest(Uri url, string title, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(url);
        
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Pull request title cannot be empty.", nameof(title));
        }

        Url = url;
        Title = title;
        Description = description;
    }
}

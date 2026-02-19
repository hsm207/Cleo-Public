namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// A high-level deliverable representing a formal code submission.
/// </summary>
public record PullRequest
{
    public Uri Url { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public string HeadRef { get; init; }
    public string BaseRef { get; init; }

    public PullRequest(
        Uri url,
        string title,
        string description,
        string headRef,
        string baseRef)
    {
        ArgumentNullException.ThrowIfNull(url);

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Pull request title cannot be empty.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Pull request description cannot be empty.", nameof(description));
        }

        if (string.IsNullOrWhiteSpace(headRef))
        {
            throw new ArgumentException("Pull request head branch (HeadRef) cannot be empty.", nameof(headRef));
        }

        if (string.IsNullOrWhiteSpace(baseRef))
        {
            throw new ArgumentException("Pull request base branch (BaseRef) cannot be empty.", nameof(baseRef));
        }

        Url = url;
        Title = title;
        Description = description;
        HeadRef = headRef;
        BaseRef = baseRef;
    }
}

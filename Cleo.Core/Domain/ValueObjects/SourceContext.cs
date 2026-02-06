namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents the location where Jules should perform her task.
/// </summary>
public record SourceContext
{
    public string Repository { get; init; }
    public string StartingBranch { get; init; }

    public SourceContext(string repository, string startingBranch)
    {
        if (string.IsNullOrWhiteSpace(repository))
        {
            throw new ArgumentException("Repository name cannot be empty.", nameof(repository));
        }

        if (string.IsNullOrWhiteSpace(startingBranch))
        {
            throw new ArgumentException("Starting branch cannot be empty.", nameof(startingBranch));
        }

        Repository = repository;
        StartingBranch = startingBranch;
    }
}

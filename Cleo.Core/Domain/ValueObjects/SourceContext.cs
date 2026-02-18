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

        if (!repository.StartsWith("sources/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Repository name must start with 'sources/'.", nameof(repository));
        }

        Repository = repository;
        StartingBranch = startingBranch;
    }
}

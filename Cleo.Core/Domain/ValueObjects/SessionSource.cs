namespace Cleo.Core.Domain.ValueObjects;

/// <summary>
/// Represents a connected source repository in Jules.
/// </summary>
public record SessionSource(string Name, string Owner, string Repo);

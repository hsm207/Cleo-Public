namespace Cleo.Cli.Models;

/// <summary>
/// A lean view model containing exactly what the StatusPresenter needs.
/// Fulfills the Interface Segregation Principle (ISP) by decoupling from Domain Entities.
/// </summary>
internal record StatusViewModel(
    string StateTitle,
    string PrOutcome,
    string LastActivityTime,
    string LastActivityHeadline,
    string? LastActivitySubHeadline,
    IReadOnlyList<string> LastActivityThoughts,
    IReadOnlyList<string> LastActivityArtifactSummaries);

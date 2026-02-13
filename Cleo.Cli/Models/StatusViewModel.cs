using Cleo.Core.Domain.ValueObjects;

namespace Cleo.Cli.Models;

/// <summary>
/// A lean view model containing exactly what the StatusPresenter needs.
/// Fulfills the Interface Segregation Principle (ISP).
/// </summary>
internal record StatusViewModel(
    string StateTitle,
    string PrOutcome,
    SessionActivity LastActivity);

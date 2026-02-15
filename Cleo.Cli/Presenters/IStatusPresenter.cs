using Cleo.Cli.Models;

namespace Cleo.Cli.Presenters;

/// <summary>
/// Defines the contract for presenting the session status.
/// Fulfills the Dependency Inversion Principle (DIP).
/// </summary>
internal interface IStatusPresenter
{
    string Format(StatusViewModel model);
}

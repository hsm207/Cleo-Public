using System.CommandLine;
using Cleo.Cli.Models;

namespace Cleo.Cli.Presenters;

/// <summary>
/// Defines the contract for presenting the session status.
/// Fulfills the Dependency Inversion Principle (DIP).
/// </summary>
internal interface IStatusPresenter
{
    void PresentSuccess(string message);
    void PresentNewSession(string sessionId, string? dashboardUri);
    void PresentWarning(string message);
    void PresentError(string message);
    void PresentStatus(StatusViewModel model);
}

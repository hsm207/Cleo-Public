using System.CommandLine;
using Cleo.Cli.Models;
using Cleo.Core.Domain.ValueObjects;

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
    void PresentSessionList(IEnumerable<(string Id, string Task, string State)> sessions);
    void PresentEmptyList();
    void PresentActivityLog(string sessionId, IEnumerable<SessionActivity> activities, bool showAll, int? limit, PullRequest? pullRequest);
    void PresentEmptyLog();
    void PresentPlan(Cleo.Core.UseCases.ViewPlan.ViewPlanResponse response);
    void PresentEmptyPlan();
    void PresentRepositories(IEnumerable<string> repositories);
    void PresentEmptyRepositories();
}

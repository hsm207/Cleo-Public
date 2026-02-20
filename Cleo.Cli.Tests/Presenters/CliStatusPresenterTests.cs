using Cleo.Cli.Aesthetics;
using Cleo.Cli.Models;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ViewPlan;
using Cleo.Tests.Common;
using FluentAssertions;
using Moq;
using System.CommandLine;
using System.CommandLine.IO;
using Xunit;

namespace Cleo.Cli.Tests.Presenters;

[Collection("ConsoleTests")]
public sealed class CliStatusPresenterTests : IDisposable
{
    private readonly TestConsole _testConsole;
    private readonly Mock<IHelpProvider> _helpProviderMock;
    private readonly CliStatusPresenter _presenter;

    public CliStatusPresenterTests()
    {
        _testConsole = new TestConsole();
        _helpProviderMock = new Mock<IHelpProvider>();
        _presenter = new CliStatusPresenter(_testConsole, _helpProviderMock.Object);

        // Setup default mock behaviors
        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(key => key);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "PresentSuccess should write message and newline.")]
    public void PresentSuccess_WritesMessage()
    {
        _presenter.PresentSuccess("Great job");
        _testConsole.Out.ToString().Should().Be("Great job" + Environment.NewLine);
    }

    [Fact(DisplayName = "PresentMessageSent should write Talk_Success resource.")]
    public void PresentMessageSent_WritesResource()
    {
        _helpProviderMock.Setup(x => x.GetResource("Talk_Success")).Returns("Sent!");
        _presenter.PresentMessageSent();
        _testConsole.Out.ToString().Should().Be("Sent!" + Environment.NewLine);
    }

    [Fact(DisplayName = "PresentNewSession should write dashboard URL if present.")]
    public void PresentNewSession_WithDashboard_WritesUrl()
    {
        _helpProviderMock.Setup(x => x.GetResource("New_SessionId")).Returns("ID: {0}");
        _helpProviderMock.Setup(x => x.GetResource("New_Portal")).Returns("Link: {0}");

        _presenter.PresentNewSession("123", "http://dash");

        var output = _testConsole.Out.ToString()!;
        output.Should().Contain("New_Success");
        output.Should().Contain("ID: 123");
        output.Should().Contain("Link: http://dash");
    }

    [Fact(DisplayName = "PresentNewSession should skip dashboard URL if null.")]
    public void PresentNewSession_WithoutDashboard_SkipsUrl()
    {
        _presenter.PresentNewSession("123", null);
        _testConsole.Out.ToString().Should().NotContain("New_Portal");
    }

    [Fact(DisplayName = "PresentEmptyPlan should write Plan_Empty resource.")]
    public void PresentEmptyPlan_WritesResource()
    {
        _presenter.PresentEmptyPlan();
        _testConsole.Out.ToString().Should().Contain("Plan_Empty");
    }

    [Fact(DisplayName = "PresentPlan should format approved plan correctly.")]
    public void PresentPlan_Approved_FormatsCorrectly()
    {
        var response = new ViewPlanResponse(
            true,
            new PlanId("p1"),
            DateTimeOffset.UtcNow,
            new[] { new PlanStepModel(1, "Step 1", "Desc\nLine2") },
            true);

        _helpProviderMock.Setup(x => x.GetResource("Plan_Header")).Returns("Header: {0} {1}");
        _helpProviderMock.Setup(x => x.GetResource("Plan_Title_Approved")).Returns("Approved");

        _presenter.PresentPlan(response);

        var output = _testConsole.Out.ToString()!;
        output.Should().Contain("Header: Approved p1");
        output.Should().Contain("1. Step 1");
        output.Should().Contain("   Desc");
        output.Should().Contain("   Line2");
    }

    [Fact(DisplayName = "PresentPlan should format proposed plan correctly.")]
    public void PresentPlan_Proposed_FormatsCorrectly()
    {
        var response = new ViewPlanResponse(
            false,
            new PlanId("p1"),
            DateTimeOffset.UtcNow,
            new[] { new PlanStepModel(1, "Step 1", null) },
            true);

        _helpProviderMock.Setup(x => x.GetResource("Plan_Header")).Returns("Header: {0} {1}");
        _helpProviderMock.Setup(x => x.GetResource("Plan_Title_Proposed")).Returns("Proposed");

        _presenter.PresentPlan(response);

        _testConsole.Out.ToString().Should().Contain("Header: Proposed p1");
    }

    [Fact(DisplayName = "PresentEmptyRepositories should write Repos_Empty.")]
    public void PresentEmptyRepositories_WritesResource()
    {
        _presenter.PresentEmptyRepositories();
        _testConsole.Out.ToString().Should().Contain("Repos_Empty");
    }

    [Fact(DisplayName = "PresentRepositories should write list.")]
    public void PresentRepositories_WritesList()
    {
        _helpProviderMock.Setup(x => x.GetResource("Repos_Item_Format")).Returns("- {0}");
        _presenter.PresentRepositories(new[] { "repo1", "repo2" });

        var output = _testConsole.Out.ToString()!;
        output.Should().Contain("Repos_Header");
        output.Should().Contain("- repo1");
        output.Should().Contain("- repo2");
    }

    [Fact(DisplayName = "PresentWarning should write message.")]
    public void PresentWarning_WritesMessage()
    {
        _presenter.PresentWarning("Watch out");
        _testConsole.Out.ToString().Should().Be("Watch out" + Environment.NewLine);
    }

    [Fact(DisplayName = "PresentError should format error.")]
    public void PresentError_FormatsError()
    {
        _helpProviderMock.Setup(x => x.GetResource("New_Error")).Returns("Error: {0}");
        _presenter.PresentError("Bang");
        _testConsole.Out.ToString().Should().Be("Error: Bang" + Environment.NewLine);
    }

    [Fact(DisplayName = "PresentStatus should format view model.")]
    public void PresentStatus_FormatsViewModel()
    {
        var vm = new StatusViewModel(
            "Running", "PR Open", "10:00", "Coding", "Details",
            new[] { "Thought 1", "Thought 2" }.ToList().AsReadOnly(),
            new[] { "File 1" }.ToList().AsReadOnly());

        _helpProviderMock.Setup(x => x.GetResource("Label_SessionState")).Returns("State");
        _helpProviderMock.Setup(x => x.GetResource("Label_PullRequest")).Returns("PR");
        _helpProviderMock.Setup(x => x.GetResource("Label_LastActivity")).Returns("Last");

        _presenter.PresentStatus(vm);

        var output = _testConsole.Out.ToString()!;
        output.Should().Contain("State: [Running]");
        output.Should().Contain("PR: PR Open");
        output.Should().Contain("Last: [10:00] Coding");
        output.Should().Contain("Details");
        output.Should().Contain($"{CliAesthetic.ThoughtBubble} Thought 1");
        output.Should().Contain("Thought 2");
        output.Should().Contain($"{CliAesthetic.ArtifactBox} File 1");
    }

    [Fact(DisplayName = "PresentSessionList should format list.")]
    public void PresentSessionList_FormatsList()
    {
        _helpProviderMock.Setup(x => x.GetResource("List_Item_Format")).Returns("{0}|{1}|{2}");
        _presenter.PresentSessionList(new[] { ("1", "Task", "State") });

        var output = _testConsole.Out.ToString()!;
        output.Should().Contain("List_Header");
        output.Should().Contain("1|Task|State");
    }

    [Fact(DisplayName = "PresentEmptyList should write List_Empty.")]
    public void PresentEmptyList_WritesResource()
    {
        _presenter.PresentEmptyList();
        _testConsole.Out.ToString().Should().Contain("List_Empty");
    }

    [Fact(DisplayName = "PresentEmptyLog should write Log_Empty.")]
    public void PresentEmptyLog_WritesResource()
    {
        _presenter.PresentEmptyLog();
        _testConsole.Out.ToString().Should().Contain("Log_Empty");
    }

    [Fact(DisplayName = "PresentActivityLog should handle showing all activities.")]
    public void PresentActivityLog_ShowAll()
    {
        var activities = new[] {
            new ProgressActivity("1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.System, "A1")
        };

        _helpProviderMock.Setup(x => x.GetResource("Log_ShowingAll")).Returns("All {0}");
        _presenter.PresentActivityLog("s1", activities, true, null, null);

        var output = _testConsole.Out.ToString()!;
        output.Should().Contain("All 1");
        output.Should().Contain("A1");
    }

    [Fact(DisplayName = "PresentActivityLog should handle significant activities logic.")]
    public void PresentActivityLog_ShowSignificant()
    {
        // 1 significant, 1 insignificant (trace)
        var activities = new SessionActivity[] {
            new ProgressActivity("1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.System, "Sig", "Reasoning"),
            new ProgressActivity("2", "r2", DateTimeOffset.UtcNow, ActivityOriginator.System, "Trace", null)
        };

        _helpProviderMock.Setup(x => x.GetResource("Log_ShowingSignificant")).Returns("Sig {0}/{1} Hidden {2}");
        _presenter.PresentActivityLog("s1", activities, false, 10, null);

        var output = _testConsole.Out.ToString()!;
        output.Should().Contain("Sig");
        output.Should().NotContain("Trace");
        output.Should().Contain("Sig 1/1 Hidden 1");
    }

    [Fact(DisplayName = "PresentActivityLog should display PR info if present.")]
    public void PresentActivityLog_WithPR()
    {
         var activities = new SessionActivity[] {};
         var pr = new PullRequest(new Uri("http://pr"), "T", "O", "H", "B");

         _helpProviderMock.Setup(x => x.GetResource("Log_PullRequest")).Returns("PR: {0}");
         _presenter.PresentActivityLog("s1", activities, true, null, pr);

         _testConsole.Out.ToString().Should().Contain("PR: http://pr");
    }

    [Fact(DisplayName = "PresentActivityLog should handle hidden earlier activities.")]
    public void PresentActivityLog_HiddenEarlier()
    {
        // 2 significant activities, limit 1
        var activities = new SessionActivity[] {
            new ProgressActivity("1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.System, "Old", "Reasoning"),
            new ProgressActivity("2", "r2", DateTimeOffset.UtcNow, ActivityOriginator.System, "New", "Reasoning")
        };

        _helpProviderMock.Setup(x => x.GetResource("Log_HiddenEarlier")).Returns("HiddenEarlier {0}");
        _presenter.PresentActivityLog("s1", activities, false, 1, null);

        var output = _testConsole.Out.ToString()!;
        output.Should().Contain("HiddenEarlier 1");
        output.Should().Contain("New");
        output.Should().NotContain("Old");
    }

    [Fact(DisplayName = "PresentActivityLog should handle heartbeats gap.")]
    public void PresentActivityLog_HiddenHeartbeats()
    {
        // Sig -> Trace -> Sig
        var activities = new SessionActivity[] {
            new ProgressActivity("1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.System, "Sig1", "R"),
            new ProgressActivity("2", "r2", DateTimeOffset.UtcNow, ActivityOriginator.System, "Trace", null),
            new ProgressActivity("3", "r3", DateTimeOffset.UtcNow, ActivityOriginator.System, "Sig2", "R")
        };

        _helpProviderMock.Setup(x => x.GetResource("Log_HiddenHeartbeats")).Returns("Gap {0}");
        _presenter.PresentActivityLog("s1", activities, false, 10, null);

        var output = _testConsole.Out.ToString()!;
        output.Should().Contain("Sig1");
        output.Should().Contain("Gap 1");
        output.Should().Contain("Sig2");
    }
}

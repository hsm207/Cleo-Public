using Cleo.Cli.Models;
using Cleo.Cli.Aesthetics;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.UseCases.ViewPlan;
using FluentAssertions;
using Xunit;
using Moq;
using System.CommandLine;
using System.CommandLine.IO;

namespace Cleo.Cli.Tests.Presenters;

public sealed class StatusPresenterTests
{
    private readonly Mock<IHelpProvider> _helpProviderMock = new();
    private readonly CliStatusPresenter _sut;
    private readonly TestConsole _testConsole = new();

    public StatusPresenterTests()
    {
        _helpProviderMock.Setup(x => x.GetResource(It.IsAny<string>())).Returns<string>(k => $"[{k}]");
        _sut = new CliStatusPresenter(_testConsole, _helpProviderMock.Object);
    }

    [Fact]
    public void PresentSuccess_WritesMessage()
    {
        _sut.PresentSuccess("Great job!");
        _testConsole.Out.ToString().Should().Contain($"Great job!{Environment.NewLine}");
    }

    [Fact]
    public void PresentWarning_WritesMessage()
    {
        _sut.PresentWarning("Watch out!");
        _testConsole.Out.ToString().Should().Contain($"Watch out!{Environment.NewLine}");
    }

    [Fact]
    public void PresentError_FormatsMessage()
    {
        _helpProviderMock.Setup(x => x.GetResource("New_Error")).Returns("Error: {0}");
        _sut.PresentError("Fail");
        _testConsole.Out.ToString().Should().Contain($"Error: Fail{Environment.NewLine}");
    }

    [Fact]
    public void PresentNewSession_FormatsOutput()
    {
        _helpProviderMock.Setup(x => x.GetResource("New_Success")).Returns("Success!");
        _helpProviderMock.Setup(x => x.GetResource("New_SessionId")).Returns("ID: {0}");
        _helpProviderMock.Setup(x => x.GetResource("New_Portal")).Returns("Link: {0}");

        _sut.PresentNewSession("123", "http://portal");

        var output = _testConsole.Out.ToString();
        output.Should().Contain("Success!");
        output.Should().Contain("ID: 123");
        output.Should().Contain("Link: http://portal");
    }

    [Fact]
    public void PresentSessionList_FormatsList()
    {
        _helpProviderMock.Setup(x => x.GetResource("List_Header")).Returns("Header");
        _helpProviderMock.Setup(x => x.GetResource("List_Item_Format")).Returns("{0} - {1} [{2}]");

        _sut.PresentSessionList(new[] { ("1", "Task", "State") });

        var output = _testConsole.Out.ToString();
        output.Should().Contain("Header");
        output.Should().Contain("1 - Task [State]");
    }

    [Fact]
    public void PresentEmptyList_FormatsMessage()
    {
        _helpProviderMock.Setup(x => x.GetResource("List_Empty")).Returns("Empty");
        _sut.PresentEmptyList();
        _testConsole.Out.ToString().Should().Contain("Empty");
    }

    [Fact]
    public void PresentRepositories_FormatsList()
    {
        _helpProviderMock.Setup(x => x.GetResource("Repos_Header")).Returns("Repos:");
        _helpProviderMock.Setup(x => x.GetResource("Repos_Item_Format")).Returns("- {0}");

        _sut.PresentRepositories(new[] { "repo1" });

        var output = _testConsole.Out.ToString();
        output.Should().Contain("Repos:");
        output.Should().Contain("- repo1");
    }

    [Fact]
    public void PresentEmptyRepositories_FormatsMessage()
    {
        _helpProviderMock.Setup(x => x.GetResource("Repos_Empty")).Returns("No Repos");
        _sut.PresentEmptyRepositories();
        _testConsole.Out.ToString().Should().Contain("No Repos");
    }

    [Fact]
    public void PresentPlan_FormatsPlan()
    {
        _helpProviderMock.Setup(x => x.GetResource("Plan_Header")).Returns("Plan: {0} {1}");
        _helpProviderMock.Setup(x => x.GetResource("Plan_Generated")).Returns("Gen: {0}");
        _helpProviderMock.Setup(x => x.GetResource("Plan_Title_Approved")).Returns("Approved");

        var response = new ViewPlanResponse(true, new PlanId("p1"), DateTimeOffset.UtcNow, new[] { new PlanStepModel(1, "Step 1", "Desc\nLine 2") }, true);

        _sut.PresentPlan(response);

        var output = _testConsole.Out.ToString();
        output.Should().Contain("Plan: Approved p1");
        output.Should().Contain("1. Step 1");
        output.Should().Contain("   Desc");
        output.Should().Contain("   Line 2");
    }

    [Fact]
    public void PresentEmptyPlan_FormatsMessage()
    {
        _helpProviderMock.Setup(x => x.GetResource("Plan_Empty")).Returns("No Plan");
        _sut.PresentEmptyPlan();
        _testConsole.Out.ToString().Should().Contain("No Plan");
    }

    [Fact]
    public void PresentEmptyLog_FormatsMessage()
    {
        _helpProviderMock.Setup(x => x.GetResource("Log_Empty")).Returns("No Log");
        _sut.PresentEmptyLog();
        _testConsole.Out.ToString().Should().Contain("No Log");
    }

    [Fact]
    public void PresentActivityLog_All_FormatsLog()
    {
        _helpProviderMock.Setup(x => x.GetResource("Log_Header")).Returns("Log: {0}");
        _helpProviderMock.Setup(x => x.GetResource("Log_ShowingAll")).Returns("Total: {0}");

        var activities = new[] { new ProgressActivity("a1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.System, "Intent") };

        _sut.PresentActivityLog("s1", activities, true, null, null);

        var output = _testConsole.Out.ToString();
        output.Should().Contain("Log: s1");
        output.Should().Contain("Intent");
        output.Should().Contain("Total: 1");
    }

    [Fact]
    public void PresentActivityLog_Significant_FormatsLog()
    {
        _helpProviderMock.Setup(x => x.GetResource("Log_Header")).Returns("Log: {0}");
        _helpProviderMock.Setup(x => x.GetResource("Log_ShowingSignificant")).Returns("Shown: {0} Total: {1} Hidden: {2}");

        var activities = new List<SessionActivity>
        {
            new ProgressActivity("a1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.System, "Significant", "Thought"), // Significant
            new ProgressActivity("a2", "r2", DateTimeOffset.UtcNow, ActivityOriginator.System, "Trace"), // Not Significant
            new ProgressActivity("a3", "r3", DateTimeOffset.UtcNow, ActivityOriginator.System, "Significant", "Thought")  // Significant
        };

        _sut.PresentActivityLog("s1", activities, false, 10, null);

        var output = _testConsole.Out.ToString();
        output.Should().Contain("Significant");
        output.Should().NotContain("Trace"); // Assuming RenderActivity filters based on logic
        output.Should().Contain("Shown: 2 Total: 2 Hidden: 1");
    }

    [Fact]
    public void PresentActivityLog_Significant_Truncated_FormatsLog()
    {
        _helpProviderMock.Setup(x => x.GetResource("Log_Header")).Returns("Log: {0}");
        _helpProviderMock.Setup(x => x.GetResource("Log_ShowingSignificant")).Returns("Shown: {0} Total: {1} Hidden: {2}");
        _helpProviderMock.Setup(x => x.GetResource("Log_HiddenEarlier")).Returns("HiddenEarlier: {0}");
        _helpProviderMock.Setup(x => x.GetResource("Log_HiddenHeartbeats")).Returns("HiddenHeartbeats: {0}");

        var activities = new List<SessionActivity>();

        // Add 20 significant activities
        for (int i = 0; i < 20; i++)
        {
            activities.Add(new ProgressActivity($"a{i}", $"r{i}", DateTimeOffset.UtcNow, ActivityOriginator.System, $"Sig {i}", "Thought"));
            // Add a gap heartbeat
            activities.Add(new ProgressActivity($"h{i}", $"hr{i}", DateTimeOffset.UtcNow, ActivityOriginator.System, "Trace"));
        }

        // Limit to 5
        _sut.PresentActivityLog("s1", activities, false, 5, null);

        var output = _testConsole.Out.ToString();
        output.Should().Contain("HiddenEarlier: 15"); // 20 - 5 = 15 hidden significant
        output.Should().Contain("HiddenHeartbeats: 1"); // Gap between displayed ones
    }

    [Fact]
    public void PresentActivityLog_WithPR_FormatsPR()
    {
        _helpProviderMock.Setup(x => x.GetResource("Log_Header")).Returns("Log: {0}");
        _helpProviderMock.Setup(x => x.GetResource("Log_PullRequest")).Returns("PR: {0}");

        var activities = new[] { new ProgressActivity("a1", "r1", DateTimeOffset.UtcNow, ActivityOriginator.System, "Intent") };
        var pr = new PullRequest(new Uri("http://pr"), "Title", "Open", "head", "base");

        _sut.PresentActivityLog("s1", activities, true, null, pr);

        var output = _testConsole.Out.ToString();
        output.Should().Contain("PR: http://pr");
    }

    [Fact]
    public void PresentStatus_FormatsStatus()
    {
        var model = new StatusViewModel("State", "PR", "Time", "Headline", "Sub", new[] { "Thought" }, new[] { "Artifact" });
        _sut.PresentStatus(model);

        var output = _testConsole.Out.ToString();
        output.Should().Contain("[State]");
        output.Should().Contain("PR");
        output.Should().Contain("Headline");
        output.Should().Contain("Sub");
        output.Should().Contain("Thought");
        output.Should().Contain("Artifact");
    }
}

using Cleo.Cli.Models;
using Cleo.Cli.Aesthetics;
using Cleo.Cli.Presenters;
using Cleo.Core.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Cleo.Cli.Tests.Presenters;

public class StatusPresenterTests
{
    private readonly CliStatusPresenter _sut = new();

    [Fact(DisplayName = "Presenter should format the 3 canonical lines correctly")]
    public void ShouldFormatCanonicalLines()
    {
        // Arrange
        var activity = new MessageActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.User, "Hello");
        var model = new StatusViewModel("Working", "⏳ In Progress", activity);

        // Act
        var output = _sut.Format(model);

        // Assert
        output.Should().Contain($"{CliAesthetic.SessionStateLabel}: [Working]");
        output.Should().Contain($"{CliAesthetic.PullRequestLabel}: ⏳ In Progress");
        output.Should().Contain(CliAesthetic.LastActivityLabel);
        output.Should().Contain("Hello");
    }

    [Fact(DisplayName = "Presenter should indent thoughts from ProgressActivity using polymorphic GetThoughts")]
    public void ShouldIndentThoughts()
    {
        // Arrange
        var activity = new ProgressActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Task", "Line 1\nLine 2");
        var model = new StatusViewModel("Working", "⏳ In Progress", activity);

        // Act
        var output = _sut.Format(model);

        // Assert
        output.Should().Contain($"\n{CliAesthetic.Indent}{CliAesthetic.ThoughtBubble} Line 1");
        output.Should().Contain($"\n{CliAesthetic.Indent}   Line 2");
    }

    [Fact(DisplayName = "Presenter should format evidence (artifacts) from activity")]
    public void ShouldFormatEvidence()
    {
        // Arrange
        var evidence = new List<Artifact> { new BashOutput("ls", "out", 0) };
        var activity = new ProgressActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.Agent, "Task", null, evidence);
        var model = new StatusViewModel("Working", "⏳ In Progress", activity);

        // Act
        var output = _sut.Format(model);

        // Assert
        output.Should().Contain($"{CliAesthetic.ArtifactBox} 🖥️ BashOutput: Executed 'ls' (Exit Code: 0)");
    }

    [Fact(DisplayName = "Presenter should format session assigned activity")]
    public void ShouldFormatSessionAssigned()
    {
        // Arrange
        var activity = new SessionAssignedActivity("a", "r", DateTimeOffset.UtcNow, ActivityOriginator.User, (TaskDescription)"Mission");
        var model = new StatusViewModel("Queued", "⏳ In Progress", activity);

        // Act
        var output = _sut.Format(model);

        // Assert
        output.Should().Contain("Session Assigned: Mission");
    }

    [Fact(DisplayName = "Presenter should throw ArgumentNullException if model is null")]
    public void ShouldThrowIfModelIsNull()
    {
        Action act = () => _sut.Format(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

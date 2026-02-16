using Cleo.Cli.Models;
using Cleo.Cli.Aesthetics;
using Cleo.Cli.Presenters;
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
        var model = new StatusViewModel(
            "Working",
            "⏳ In Progress",
            "12:00",
            "Hello",
            null, // SubHeadline
            Array.Empty<string>(),
            Array.Empty<string>());

        // Act
        var output = _sut.Format(model);

        // Assert
        output.Should().Contain($"{CliAesthetic.SessionStateLabel}: [Working]");
        output.Should().Contain($"{CliAesthetic.PullRequestLabel}: ⏳ In Progress");
        output.Should().Contain($"{CliAesthetic.LastActivityLabel}: [12:00] Hello");
    }

    [Fact(DisplayName = "Presenter should indent thoughts correctly")]
    public void ShouldIndentThoughts()
    {
        // Arrange
        var thoughts = new[] { "Line 1", "Line 2" };
        var model = new StatusViewModel(
            "Working",
            "⏳ In Progress",
            "12:00",
            "Task",
            null, // SubHeadline
            thoughts,
            Array.Empty<string>());

        // Act
        var output = _sut.Format(model);

        // Assert
        output.Should().Contain($"\n{CliAesthetic.Indent}{CliAesthetic.ThoughtBubble} Line 1");
        output.Should().Contain($"\n{CliAesthetic.Indent}   Line 2");
    }

    [Fact(DisplayName = "Presenter should format evidence (artifacts) correctly")]
    public void ShouldFormatEvidence()
    {
        // Arrange
        var artifacts = new[] { "BashOutput: Executed 'ls' (Exit Code: 0)" };
        var model = new StatusViewModel(
            "Working",
            "⏳ In Progress",
            "12:00",
            "Task",
            null, // SubHeadline
            Array.Empty<string>(),
            artifacts);

        // Act
        var output = _sut.Format(model);

        // Assert
        output.Should().Contain($"{CliAesthetic.ArtifactBox} BashOutput: Executed 'ls' (Exit Code: 0)");
    }

    [Fact(DisplayName = "Presenter should throw ArgumentNullException if model is null")]
    public void ShouldThrowIfModelIsNull()
    {
        Action act = () => _sut.Format(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

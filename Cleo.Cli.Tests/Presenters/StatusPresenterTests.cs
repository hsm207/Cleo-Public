using Cleo.Cli.Models;
using Cleo.Cli.Aesthetics;
using Cleo.Cli.Presenters;
using Cleo.Cli.Services;
using FluentAssertions;
using Xunit;
using Moq;
using System.CommandLine;
using System.CommandLine.IO;

namespace Cleo.Cli.Tests.Presenters;

public class StatusPresenterTests
{
    private readonly Mock<IConsole> _consoleMock = new();
    private readonly Mock<IHelpProvider> _helpProviderMock = new();
    private readonly CliStatusPresenter _sut;
    private readonly TestConsole _testConsole = new();

    public StatusPresenterTests()
    {
        _sut = new CliStatusPresenter(_testConsole, _helpProviderMock.Object);
    }

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
        _sut.PresentStatus(model);
        var output = _testConsole.Out.ToString()!;

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
        _sut.PresentStatus(model);
        var output = _testConsole.Out.ToString()!;

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
        _sut.PresentStatus(model);
        var output = _testConsole.Out.ToString()!;

        // Assert
        output.Should().Contain($"{CliAesthetic.ArtifactBox} BashOutput: Executed 'ls' (Exit Code: 0)");
    }

    [Fact(DisplayName = "Presenter should throw ArgumentNullException if model is null")]
    public void ShouldThrowIfModelIsNull()
    {
        Action act = () => _sut.PresentStatus(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

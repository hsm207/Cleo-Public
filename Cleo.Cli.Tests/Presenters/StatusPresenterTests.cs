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
    private readonly Mock<IHelpProvider> _helpProviderMock = new();
    private readonly CliStatusPresenter _sut;
    private readonly TestConsole _testConsole = new();

    public StatusPresenterTests()
    {
        _sut = new CliStatusPresenter(_testConsole, _helpProviderMock.Object);
    }

    [Fact(DisplayName = "PresentSuccess should write message followed by newline")]
    public void PresentSuccess_WritesMessage()
    {
        // Act
        _sut.PresentSuccess("Great job!");

        // Assert
        var output = _testConsole.Out.ToString()!;
        output.Should().Be($"Great job!{Environment.NewLine}");
    }

    [Fact(DisplayName = "PresentWarning should write message followed by newline")]
    public void PresentWarning_WritesMessage()
    {
        // Act
        _sut.PresentWarning("Watch out!");

        // Assert
        var output = _testConsole.Out.ToString()!;
        output.Should().Be($"Watch out!{Environment.NewLine}");
    }

    [Fact(DisplayName = "PresentError should format error using resource template")]
    public void PresentError_FormatsMessage()
    {
        // Arrange
        _helpProviderMock.Setup(x => x.GetResource("New_Error")).Returns("Error: {0}");

        // Act
        _sut.PresentError("Something went wrong");

        // Assert
        var output = _testConsole.Out.ToString()!;
        output.Should().Be($"Error: Something went wrong{Environment.NewLine}");
    }

    [Fact(DisplayName = "PresentNewSession should format success, session ID, and portal URI")]
    public void PresentNewSession_FormatsOutput()
    {
        // Arrange
        _helpProviderMock.Setup(x => x.GetResource("New_Success")).Returns("Success!");
        _helpProviderMock.Setup(x => x.GetResource("New_SessionId")).Returns("ID: {0}");
        _helpProviderMock.Setup(x => x.GetResource("New_Portal")).Returns("Link: {0}");

        // Act
        _sut.PresentNewSession("123", "http://portal");

        // Assert
        var output = _testConsole.Out.ToString()!;
        output.Should().Contain($"Success!{Environment.NewLine}");
        output.Should().Contain($"ID: 123{Environment.NewLine}");
        output.Should().Contain($"Link: http://portal{Environment.NewLine}");
    }

    [Fact(DisplayName = "PresentNewSession should skip portal line if URI is null")]
    public void PresentNewSession_SkipsPortalIfNull()
    {
        // Arrange
        _helpProviderMock.Setup(x => x.GetResource("New_Success")).Returns("Success!");
        _helpProviderMock.Setup(x => x.GetResource("New_SessionId")).Returns("ID: {0}");
        _helpProviderMock.Setup(x => x.GetResource("New_Portal")).Returns("Link: {0}");

        // Act
        _sut.PresentNewSession("123", null);

        // Assert
        var output = _testConsole.Out.ToString()!;
        output.Should().Contain($"Success!{Environment.NewLine}");
        output.Should().Contain($"ID: 123{Environment.NewLine}");
        output.Should().NotContain("Link:");
    }

    [Fact(DisplayName = "PresentStatus should format the 3 canonical lines correctly")]
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

    [Fact(DisplayName = "PresentStatus should include subheadline if present")]
    public void ShouldIncludeSubHeadline()
    {
        // Arrange
        var model = new StatusViewModel(
            "Working",
            "PR",
            "12:00",
            "Head",
            "SubHead",
            Array.Empty<string>(),
            Array.Empty<string>());

        // Act
        _sut.PresentStatus(model);
        var output = _testConsole.Out.ToString()!;

        // Assert
        output.Should().Contain($"\n{CliAesthetic.Indent}SubHead");
    }

    [Fact(DisplayName = "PresentStatus should indent thoughts correctly")]
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

    [Fact(DisplayName = "PresentStatus should format evidence (artifacts) correctly")]
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

    [Fact(DisplayName = "PresentStatus should throw ArgumentNullException if model is null")]
    public void ShouldThrowIfModelIsNull()
    {
        Action act = () => _sut.PresentStatus(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

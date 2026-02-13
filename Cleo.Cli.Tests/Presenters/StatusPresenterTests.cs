using Cleo.Cli.Models;
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
        output.Should().Contain("🧘‍♀️ Session State: [Working]");
        output.Should().Contain("🎁 Pull Request: ⏳ In Progress");
        output.Should().Contain("📝 Last Activity:");
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
        output.Should().Contain("\n          💭 Line 1");
        output.Should().Contain("\n             Line 2");
    }

    [Fact(DisplayName = "Presenter should throw ArgumentNullException if model is null")]
    public void ShouldThrowIfModelIsNull()
    {
        Action act = () => _sut.Format(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

using Cleo.Infrastructure.Common;
using FluentAssertions;
using Xunit;

namespace Cleo.Infrastructure.Tests.Common;

public class StrategySelectorTests
{
    [Fact(DisplayName = "Select should return matching strategy.")]
    public void Select_ShouldReturnMatching()
    {
        var strategies = new[] { "Alpha", "Beta", "Gamma" };
        var result = StrategySelector.Select(strategies, "B", (s, c) => s.StartsWith(c));
        result.Should().Be("Beta");
    }

    [Fact(DisplayName = "Select should return null when no match found.")]
    public void Select_ShouldReturnNull_WhenNoMatch()
    {
        var strategies = new[] { "Alpha", "Beta" };
        var result = StrategySelector.Select(strategies, "Z", (s, c) => s.StartsWith(c));
        result.Should().BeNull();
    }

    [Fact(DisplayName = "SelectOrThrow should return matching strategy.")]
    public void SelectOrThrow_ShouldReturnMatching()
    {
        var strategies = new[] { "A", "B", "C" };
        var result = StrategySelector.SelectOrThrow(strategies, "B", (s, c) => s == c, () => "Error");
        result.Should().Be("B");
    }

    [Fact(DisplayName = "SelectOrThrow should throw InvalidOperationException when no match found.")]
    public void SelectOrThrow_ShouldThrow_WhenNoMatch()
    {
        var strategies = new[] { "A", "B", "C" };
        var act = () => StrategySelector.SelectOrThrow(strategies, "Z", (s, c) => s == c, () => "My Error Message");

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("My Error Message");
    }
}

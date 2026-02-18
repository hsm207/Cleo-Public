using Cleo.Core.Domain.ValueObjects;
using Cleo.Core.Domain.Entities;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class HistoryCriteriaTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private static readonly ProgressActivity DefaultActivity = new("act-1", "rem-1", Now, ActivityOriginator.Agent, "Intent", "Thinking");

    [Fact(DisplayName = "Given no criteria, IsSatisfiedBy should return true.")]
    public void NoneSatisfiesAll()
    {
        var criteria = HistoryCriteria.None;
        Assert.True(criteria.IsSatisfiedBy(DefaultActivity));
    }

    [Fact(DisplayName = "Given Type criteria, IsSatisfiedBy should respect it.")]
    public void TypeFiltersCorrectly()
    {
        var criteria = new HistoryCriteria(Types: new[] { typeof(ProgressActivity) });

        Assert.True(criteria.IsSatisfiedBy(DefaultActivity));
        Assert.False(criteria.IsSatisfiedBy(new CompletionActivity("c", "r", Now, ActivityOriginator.System)));
    }

    [Fact(DisplayName = "Given Since criteria, IsSatisfiedBy should respect it.")]
    public void SinceFiltersCorrectly()
    {
        var criteria = new HistoryCriteria(Since: Now.AddMinutes(-1));

        var old = DefaultActivity with { Timestamp = Now.AddMinutes(-2) };
        var newer = DefaultActivity with { Timestamp = Now };

        Assert.False(criteria.IsSatisfiedBy(old));
        Assert.True(criteria.IsSatisfiedBy(newer));
    }

    [Fact(DisplayName = "Given Until criteria, IsSatisfiedBy should respect it.")]
    public void UntilFiltersCorrectly()
    {
        var criteria = new HistoryCriteria(Until: Now.AddMinutes(1));

        var newer = DefaultActivity with { Timestamp = Now.AddMinutes(2) };
        var older = DefaultActivity with { Timestamp = Now };

        Assert.False(criteria.IsSatisfiedBy(newer));
        Assert.True(criteria.IsSatisfiedBy(older));
    }

    [Fact(DisplayName = "Given Text criteria, IsSatisfiedBy should match Intent (Content Summary).")]
    public void TextMatchesIntent()
    {
        var criteria = new HistoryCriteria(Text: "Intent");
        Assert.True(criteria.IsSatisfiedBy(DefaultActivity));
    }

    [Fact(DisplayName = "Given Text criteria, IsSatisfiedBy should match Reasoning.")]
    public void TextMatchesReasoning()
    {
        var criteria = new HistoryCriteria(Text: "Thinking");
        Assert.True(criteria.IsSatisfiedBy(DefaultActivity));
    }

    [Fact(DisplayName = "Given Text criteria, IsSatisfiedBy should be case insensitive.")]
    public void TextIsCaseInsensitive()
    {
        var criteria = new HistoryCriteria(Text: "INTENT");
        Assert.True(criteria.IsSatisfiedBy(DefaultActivity));
    }

    [Fact(DisplayName = "Given Text criteria, IsSatisfiedBy should return false if no match.")]
    public void TextReturnsFalseOnMismatch()
    {
        var criteria = new HistoryCriteria(Text: "Banana");
        Assert.False(criteria.IsSatisfiedBy(DefaultActivity));
    }

    [Fact(DisplayName = "Given null activity, IsSatisfiedBy should throw ArgumentNullException.")]
    public void ThrowsOnNullActivity()
    {
        var criteria = HistoryCriteria.None;
        Assert.Throws<ArgumentNullException>(() => criteria.IsSatisfiedBy(null!));
    }
}

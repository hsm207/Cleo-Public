using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class ChangeSetTests
{
    private static readonly GitPatch Patch = new("diff", "sha");

    [Fact(DisplayName = "ChangeSet should be created with valid arguments.")]
    public void ShouldCreateWithValidArgs()
    {
        var changeSet = new ChangeSet("sources/repo", Patch);

        Assert.Equal("sources/repo", changeSet.Source);
        Assert.Equal(Patch, changeSet.Patch);
    }

    [Fact(DisplayName = "ChangeSet should throw if Source is empty.")]
    public void ShouldThrowIfSourceEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ChangeSet("", Patch));
        Assert.Throws<ArgumentException>(() => new ChangeSet(" ", Patch));
        Assert.Throws<ArgumentNullException>(() => new ChangeSet(null!, Patch));
    }

    [Fact(DisplayName = "ChangeSet should throw if Patch is null.")]
    public void ShouldThrowIfPatchNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ChangeSet("source", null!));
    }

    [Fact(DisplayName = "ChangeSet should provide a human-friendly summary with file details.")]
    public void ShouldProvideSummary()
    {
        var diff = @"--- a/file1.cs
+++ b/file1.cs
--- a/README.md
+++ b/README.md
";
        var changeSet = new ChangeSet("sources/github/hsm207/Cleo", new GitPatch(diff, "sha"));
        Assert.Equal("📦 ChangeSet: Updated [file1.cs, README.md]", changeSet.GetSummary());
    }
}

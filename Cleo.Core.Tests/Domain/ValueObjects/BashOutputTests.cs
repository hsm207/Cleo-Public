using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public sealed class BashOutputTests
{
    [Fact(DisplayName = "BashOutput should be created with valid arguments.")]
    public void ShouldCreateWithValidArgs()
    {
        var output = new BashOutput("echo hi", "hi", 0);

        Assert.Equal("echo hi", output.Command);
        Assert.Equal("hi", output.Output);
        Assert.Equal(0, output.ExitCode);
    }

    [Fact(DisplayName = "BashOutput should throw if Command is empty.")]
    public void ShouldThrowIfCommandEmpty()
    {
        Assert.Throws<ArgumentException>(() => new BashOutput("", "out", 0));
        Assert.Throws<ArgumentException>(() => new BashOutput(" ", "out", 0));
        Assert.Throws<ArgumentNullException>(() => new BashOutput(null!, "out", 0));
    }

    [Fact(DisplayName = "BashOutput should allow empty Output.")]
    public void ShouldAllowEmptyOutput()
    {
        var output = new BashOutput("cmd", "", 0);
        Assert.Equal("", output.Output);
    }

    [Fact(DisplayName = "BashOutput should provide a human-friendly summary.")]
    public void ShouldProvideSummary()
    {
        var output = new BashOutput("npm install", "ok", 0);
        Assert.Equal("BashOutput: Executed 'npm install' (Exit Code: 0)", output.GetSummary());
    }
}

using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class VisualSnapshotTests
{
    [Fact(DisplayName = "VisualSnapshot should be created with valid arguments.")]
    public void ShouldCreateWithValidArgs()
    {
        var media = new VisualSnapshot("image/png", "base64data");

        Assert.Equal("image/png", media.MimeType);
        Assert.Equal("base64data", media.Data);
    }

    [Fact(DisplayName = "VisualSnapshot should throw if MimeType is empty.")]
    public void ShouldThrowIfMimeTypeEmpty()
    {
        Assert.Throws<ArgumentException>(() => new VisualSnapshot("", "data"));
        Assert.Throws<ArgumentException>(() => new VisualSnapshot(" ", "data"));
        Assert.Throws<ArgumentNullException>(() => new VisualSnapshot(null!, "data"));
    }

    [Fact(DisplayName = "VisualSnapshot should provide a human-friendly summary.")]
    public void ShouldProvideSummary()
    {
        var media = new VisualSnapshot("image/png", "data");
        Assert.Equal("🖼️ Media: Produced 'image/png'", media.GetSummary());
    }
}

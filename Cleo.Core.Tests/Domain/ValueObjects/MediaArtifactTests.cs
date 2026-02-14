using Cleo.Core.Domain.ValueObjects;
using Xunit;

namespace Cleo.Core.Tests.Domain.ValueObjects;

public class MediaArtifactTests
{
    [Fact(DisplayName = "MediaArtifact should be created with valid arguments.")]
    public void ShouldCreateWithValidArgs()
    {
        var media = new MediaArtifact("image/png", "base64data");

        Assert.Equal("image/png", media.MimeType);
        Assert.Equal("base64data", media.Data);
    }

    [Fact(DisplayName = "MediaArtifact should throw if MimeType is empty.")]
    public void ShouldThrowIfMimeTypeEmpty()
    {
        Assert.Throws<ArgumentException>(() => new MediaArtifact("", "data"));
        Assert.Throws<ArgumentException>(() => new MediaArtifact(" ", "data"));
        Assert.Throws<ArgumentNullException>(() => new MediaArtifact(null!, "data"));
    }

    [Fact(DisplayName = "MediaArtifact should provide a human-friendly summary.")]
    public void ShouldProvideSummary()
    {
        var imageMedia = new MediaArtifact("image/png", "data");
        Assert.Equal("Media: Produced 'image/png'", imageMedia.GetSummary());

        var genericMedia = new MediaArtifact("application/pdf", "data");
        Assert.Equal("Media: Produced 'application/pdf'", genericMedia.GetSummary());
    }
}

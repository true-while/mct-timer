using mct_timer.Models;
using Xunit;

namespace mct_timer.Tests;

public class SessionMediaHelperTests
{
    [Theory]
    [InlineData("https://example.com/banner.gif", ShowcaseMediaKind.Image, "https://example.com/banner.gif")]
    [InlineData("https://example.com/video.mp4", ShowcaseMediaKind.Video, "https://example.com/video.mp4")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", ShowcaseMediaKind.Embed, "https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", ShowcaseMediaKind.Embed, "https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://vimeo.com/123456789", ShowcaseMediaKind.Embed, "https://player.vimeo.com/video/123456789")]
    public void TryNormalizeMediaUrl_ReturnsSupportedMedia(string input, ShowcaseMediaKind expectedKind, string expectedUrl)
    {
        var result = SessionMediaHelper.TryNormalizeMediaUrl(input, out var mediaUrl, out var kind);

        Assert.True(result);
        Assert.Equal(expectedKind, kind);
        Assert.Equal(expectedUrl, mediaUrl);
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://example.com/file.pdf")]
    [InlineData("https://example.com/embed")]
    public void TryNormalizeMediaUrl_ReturnsFalse_ForUnsupportedMedia(string input)
    {
        var result = SessionMediaHelper.TryNormalizeMediaUrl(input, out var mediaUrl, out var kind);

        Assert.False(result);
        Assert.Equal(ShowcaseMediaKind.None, kind);
        Assert.Equal(string.Empty, mediaUrl);
    }
}

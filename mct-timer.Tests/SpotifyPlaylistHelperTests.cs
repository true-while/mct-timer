using mct_timer.Models;
using Xunit;

namespace mct_timer.Tests;

public class SpotifyPlaylistHelperTests
{
    private const string PlaylistId = "37i9dQZF1DXcBWIGoYBM5M";
    private const string EmbedUrl = "https://open.spotify.com/embed/playlist/37i9dQZF1DXcBWIGoYBM5M";

    [Theory]
    [InlineData("https://open.spotify.com/playlist/37i9dQZF1DXcBWIGoYBM5M")]
    [InlineData("https://open.spotify.com/playlist/37i9dQZF1DXcBWIGoYBM5M?si=abc123")]
    [InlineData("spotify:playlist:37i9dQZF1DXcBWIGoYBM5M")]
    [InlineData("https://open.spotify.com/embed/playlist/37i9dQZF1DXcBWIGoYBM5M")]
    [InlineData(PlaylistId)]
    public void TryNormalizeEmbedUrl_ReturnsCleanEmbedUrl_ForSupportedInputs(string input)
    {
        var result = SpotifyPlaylistHelper.TryNormalizeEmbedUrl(input, out var embedUrl);

        Assert.True(result);
        Assert.Equal(EmbedUrl, embedUrl);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a playlist")]
    [InlineData("https://example.com/playlist/37i9dQZF1DXcBWIGoYBM5M")]
    [InlineData("http://open.spotify.com/playlist/37i9dQZF1DXcBWIGoYBM5M")]
    [InlineData("spotify:album:37i9dQZF1DXcBWIGoYBM5M")]
    [InlineData("https://open.spotify.com/playlist/not-valid")]
    public void TryNormalizeEmbedUrl_ReturnsFalse_ForInvalidInputs(string input)
    {
        var result = SpotifyPlaylistHelper.TryNormalizeEmbedUrl(input, out var embedUrl);

        Assert.False(result);
        Assert.Equal(string.Empty, embedUrl);
    }
}

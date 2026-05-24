using mct_timer.Models;
using Xunit;

namespace mct_timer.Tests;

public class InstructorProfileHelperTests
{
    [Theory]
    [InlineData("https://www.linkedin.com/in/example")]
    [InlineData("linkedin.com/in/example")]
    public void TryNormalizeLinkedInUrl_ReturnsUrl_ForLinkedInProfiles(string input)
    {
        var result = InstructorProfileHelper.TryNormalizeLinkedInUrl(input, out var profileUrl);

        Assert.True(result);
        Assert.StartsWith("https://", profileUrl);
        Assert.Contains("linkedin.com", profileUrl);
    }

    [Theory]
    [InlineData("https://example.com/in/example")]
    [InlineData("not a url")]
    public void TryNormalizeLinkedInUrl_ReturnsFalse_ForUnsupportedUrls(string input)
    {
        var result = InstructorProfileHelper.TryNormalizeLinkedInUrl(input, out var profileUrl);

        Assert.False(result);
        Assert.Equal(string.Empty, profileUrl);
    }

    [Fact]
    public void TryNormalizeQrCodeUrl_ReturnsImageUrl_ForPublicImage()
    {
        var result = InstructorProfileHelper.TryNormalizeQrCodeUrl("https://example.com/qr.png", out var qrCodeUrl);

        Assert.True(result);
        Assert.Equal("https://example.com/qr.png", qrCodeUrl);
    }

    [Fact]
    public void TryNormalizeQrCodeUrl_ReturnsFalse_ForNonImage()
    {
        var result = InstructorProfileHelper.TryNormalizeQrCodeUrl("https://example.com/qr.pdf", out var qrCodeUrl);

        Assert.False(result);
        Assert.Equal(string.Empty, qrCodeUrl);
    }
}

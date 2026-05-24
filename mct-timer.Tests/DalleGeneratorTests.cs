using mct_timer.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Xunit;

namespace mct_timer.Tests;

public class DalleGeneratorTests
{
    [Theory]
    [InlineData(null, "key", "dall-e-3")]
    [InlineData("", "key", "dall-e-3")]
    [InlineData("not-a-url", "key", "dall-e-3")]
    [InlineData("https://example.openai.azure.com/", "", "dall-e-3")]
    [InlineData("https://example.openai.azure.com/", "key", "")]
    public void IsConfigured_ReturnsFalse_WhenRequiredSettingsAreMissing(string? endpoint, string key, string model)
    {
        var generator = new DalleGenerator(endpoint, key, model, CreateTelemetryClient());

        Assert.False(generator.IsConfigured);
    }

    [Fact]
    public void IsConfigured_ReturnsTrue_WhenRequiredSettingsArePresent()
    {
        var generator = new DalleGenerator("https://example.openai.azure.com/", "key", "dall-e-3", CreateTelemetryClient());

        Assert.True(generator.IsConfigured);
    }

    private static TelemetryClient CreateTelemetryClient() => new(TelemetryConfiguration.CreateDefault());
}

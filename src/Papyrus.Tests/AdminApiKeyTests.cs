using Microsoft.Extensions.Configuration;
using Papyrus.Api.Admin.Authentication;

namespace Papyrus.Tests;

public sealed class AdminApiKeyTests
{
    [Fact]
    public void MatchesAny_accepts_each_configured_key()
    {
        // Arrange
        var configuredKeys = new[] { "primary-key", "rotating-key" };

        // Act
        var primaryAccepted = AdminApiKey.MatchesAny(configuredKeys, "primary-key");
        var rotatingAccepted = AdminApiKey.MatchesAny(configuredKeys, "rotating-key");

        // Assert
        Assert.True(primaryAccepted);
        Assert.True(rotatingAccepted);
    }

    [Fact]
    public void MatchesAny_rejects_unknown_and_empty_keys()
    {
        // Arrange
        var configuredKeys = new[] { "primary-key", "rotating-key" };

        // Act
        var unknownAccepted = AdminApiKey.MatchesAny(configuredKeys, "unknown-key");
        var emptyAccepted = AdminApiKey.MatchesAny(configuredKeys, null);

        // Assert
        Assert.False(unknownAccepted);
        Assert.False(emptyAccepted);
    }

    [Fact]
    public void Rotation_window_accepts_old_and_new_key_then_retires_old_key()
    {
        // Arrange
        var transitionOptions = new AdminApiKeyOptions
        {
            ApiKeys = ["old-key", "new-key"]
        };
        var retiredOptions = new AdminApiKeyOptions
        {
            ApiKeys = ["new-key"]
        };

        // Act
        var oldKeyDuringTransition = AdminApiKey.MatchesAny(transitionOptions.GetConfiguredKeys(), "old-key");
        var newKeyDuringTransition = AdminApiKey.MatchesAny(transitionOptions.GetConfiguredKeys(), "new-key");
        var oldKeyAfterRetirement = AdminApiKey.MatchesAny(retiredOptions.GetConfiguredKeys(), "old-key");
        var newKeyAfterRetirement = AdminApiKey.MatchesAny(retiredOptions.GetConfiguredKeys(), "new-key");

        // Assert
        Assert.True(oldKeyDuringTransition);
        Assert.True(newKeyDuringTransition);
        Assert.False(oldKeyAfterRetirement);
        Assert.True(newKeyAfterRetirement);
    }

    [Fact]
    public void Options_bind_legacy_key_and_multiple_keys_from_configuration()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{AdminApiKeyOptions.SectionName}:ApiKey"] = "legacy-key",
                [$"{AdminApiKeyOptions.SectionName}:ApiKeys:0"] = "primary-key",
                [$"{AdminApiKeyOptions.SectionName}:ApiKeys:1"] = "secondary-key"
            })
            .Build();
        var options = new AdminApiKeyOptions();

        // Act
        configuration.GetSection(AdminApiKeyOptions.SectionName).Bind(options);

        // Assert
        Assert.Equal(["legacy-key", "primary-key", "secondary-key"], options.GetConfiguredKeys());
    }
}

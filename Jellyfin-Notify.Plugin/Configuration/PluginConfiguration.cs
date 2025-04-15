using System.Text.Json;
using JellyfinNotify.Plugin.Models;
using MediaBrowser.Model.Plugins;

namespace JellyfinNotify.Plugin.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        // set default options here
        JellyfinInstanceUrl = string.Empty;
        NtfyUrl = string.Empty;
        NtfyUser = string.Empty;
        NtfyPass = string.Empty;
        NotifyUserIfWithinDays = 1;
        UserNotificationConfigurationJsonString = JsonSerializer.Serialize(new UserNotificationConfiguration() { SeriesConfigurationItems = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IEnumerable<UserSeriesNotificationConfigurationItem>>() });
    }

    /// <summary>
    /// Gets or sets a string setting.
    /// </summary>
    public string NtfyUrl { get; set; }

    /// <summary>
    /// Gets or sets a string setting.
    /// </summary>
    public string JellyfinInstanceUrl { get; set; }

    /// <summary>
    /// Gets or sets a string setting.
    /// </summary>
    public string NtfyUser { get; set; }

    /// <summary>
    /// Gets or sets a string setting.
    /// </summary>
    public string NtfyPass { get; set; }

    /// <summary>
    /// Gets or sets a integer setting.
    /// </summary>
    public int NotifyUserIfWithinDays { get; set; }

    public string UserNotificationConfigurationJsonString { get; set; }
}

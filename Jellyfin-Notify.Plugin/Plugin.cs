using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Entities;
using JellyfinNotify.Plugin.Configuration;
using JellyfinNotify.Plugin.Data;
using JellyfinNotify.Plugin.Extensions;
using JellyfinNotify.Plugin.Models;
using JellyfinNotify.Plugin.Notifications;
using JellyfinNotify.Plugin.UserData;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace JellyfinNotify.Plugin;

/// <summary>
/// The main plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages, IScheduledTask
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILibraryManager libraryManager, IUserManager userManager, ILogger<Plugin> logger, ILocalizationManager localization, IConfigurationManager configurationManager)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        Logger = logger;
        LibraryManager = libraryManager;
        UserManager = userManager;
        Localization = localization;
        NotificationFactory = new NotificationFactory();
    }

    /// <inheritdoc />
    public override string Name => "Jellfin-Notify";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("089d5ae5-5e97-4dcb-8b64-1773af1ec720");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    public static ILogger? Logger { get; private set; }

    public static IUserManager? UserManager { get; private set; }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static ILibraryManager? LibraryManager { get; private set; }

    public static INotificationFactory? NotificationFactory { get; private set; }

    public ILocalizationManager Localization { get; private set; }

    public string Key => "NotifyUsersOfUpcomingEpisodesTask";

    public string Category => Localization.GetLocalizedString("TasksLibraryCategory");

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        return
        [
            new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerDaily, TimeOfDayTicks = TimeSpan.FromHours(16).Ticks }, // Trigger every day at 4pm to notify about tomorrow
            #if DEBUG
                new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerStartup }
            #endif
        ];
    }

    public UserNotificationConfiguration GetUserNotificationConfiguration()
    {
        var jsonString = Configuration.UserNotificationConfigurationJsonString;
        UserNotificationConfiguration? userNotificationConfig;
        try
        {
            userNotificationConfig = JsonSerializer.Deserialize<UserNotificationConfiguration>(jsonString);
            if (userNotificationConfig == null)
            {
                throw new MissingMemberException("UserNotificationConfiguration is missing, reverting to default config.");
            }

            return userNotificationConfig;
        }
        catch (Exception ex)
        {
            Logger!.LogCritical("Error when deserializing UserNotificationConfiguration: {Message}", ex.Message);
            return new UserNotificationConfiguration() { SeriesConfigurationItems = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IEnumerable<UserSeriesNotificationConfigurationItem>>() };
        }
    }

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        IEnumerable<Task<NextEpisodeTaskResult>> tasks = UserManager!.Users.Select(u => Task.Run(() => EpisodeNotificationRetrieval.GetNextEpisodeDetailsForUser(u)));

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        foreach (var result in results)
        {
            var filteredResults = result.Episodes.Where(e => e.AirDate.IsInDays(Configuration.NotifyUserIfWithinDays));
            foreach (var episode in filteredResults)
            {
                Logger!.LogInformation("Notifying {User} that {Series} is airing in {Days} days.", result.User.Username, episode.SeriesName, Configuration.NotifyUserIfWithinDays);

                await SendEpisodeNotificationIfUserIsSubscribed(result.User, episode, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task SendEpisodeNotificationIfUserIsSubscribed(User user, EpisodeNotificationDetails episode, CancellationToken cancellationToken)
    {
        if (Data.UserConfiguration.IsUserSubscribedToSeries(user.Id, episode.SeriesGuid))
        {
            await NotificationFactory!.SendNotification(episode, NotificationDeliveryMethod.NtfyPush, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            }
        ];
    }
}

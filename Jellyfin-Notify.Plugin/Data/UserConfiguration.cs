using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JellyfinNotify.Plugin.Models;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace JellyfinNotify.Plugin.Data
{
    public static class UserConfiguration
    {
        public static bool IsUserSubscribedToSeries(Guid userId, Guid seriesId)
        {
            GetUserNotificationConfiguration().SeriesConfigurationItems.TryGetValue(userId.ToString(), out IEnumerable<UserSeriesNotificationConfigurationItem>? userConfig);
            if (userConfig != null)
            {
                var seriesConfig = userConfig.SingleOrDefault(c => c.SeriesGuid == seriesId.ToString());
                return seriesConfig?.IsEnabled ?? false;
            }

            return false;
        }

        public static bool UnsubscribeUserFromSeries(Guid userGuid, Guid seriesGuid)
        {
            var userNotificationConfig = GetUserNotificationConfiguration();
            if (userNotificationConfig != null)
            {
                userNotificationConfig.SeriesConfigurationItems.TryGetValue(userGuid.ToString(), out IEnumerable<UserSeriesNotificationConfigurationItem>? userConfig);
                if (userConfig != null)
                {
                    var seriesConfig = userConfig.SingleOrDefault(c => c.SeriesGuid == seriesGuid.ToString());
                    if (seriesConfig != null)
                    {
                        seriesConfig.IsEnabled = false;
                        UpdateSeriesConfiguration(seriesGuid, userGuid, false);
                        return true;
                    }
                }
            }

            Plugin.Logger!.LogCritical("An invalid unsubscribe request was recieved for User: {UserGuid}, Series: {SeriesGuid}", userGuid, seriesGuid);
            // TODO: have an admin ntfy config option so they get a push if this happens. This should never happen unless someone is poking at the webserver or the dictionary of values is corrupt.
            return false;
        }

        public static bool SubscribeUserToSeries(Guid userGuid, Guid seriesGuid)
        {
            var userNotificationConfig = GetUserNotificationConfiguration();
            if (userNotificationConfig != null)
            {
                userNotificationConfig.SeriesConfigurationItems.TryGetValue(userGuid.ToString(), out IEnumerable<UserSeriesNotificationConfigurationItem>? userConfig);
                if (userConfig != null)
                {
                    var seriesConfig = userConfig.SingleOrDefault(c => c.SeriesGuid == seriesGuid.ToString());
                    if (seriesConfig != null)
                    {
                        seriesConfig.IsEnabled = true;
                        UpdateSeriesConfiguration(seriesGuid, userGuid, true);
                        return true;
                    }
                }
            }

            Plugin.Logger!.LogCritical("An invalid unsubscribe request was recieved for User: {UserGuid}, Series: {SeriesGuid}", userGuid, seriesGuid);
            // TODO: have an admin ntfy config option so they get a push if this happens. This should never happen unless someone is poking at the webserver or the dictionary of values is corrupt.
            return false;
        }

        // Do not pass in isEnabled unless you want to modify it from its current value ( or default value if it is a new item ).
        public static void UpdateSeriesConfiguration(Guid seriesId, Guid userId, bool? isEnabled = null, string? optionalNewSeriesName = null)
        {
            var userNotificationConfig = GetUserNotificationConfiguration();

            if (userNotificationConfig!.SeriesConfigurationItems.TryGetValue(userId.ToString(), out var userConfig))
            {
                var seriesConfiguration = userConfig.SingleOrDefault(config => config.SeriesGuid == seriesId.ToString());
                if (seriesConfiguration != null)
                {
                    seriesConfiguration.SeriesName = optionalNewSeriesName ?? seriesConfiguration.SeriesName; // In case the name updated, this is unlikely.
                    seriesConfiguration.IsEnabled = isEnabled ?? seriesConfiguration.IsEnabled;

                    var updatedUserConfig = userConfig.Select(config => config.SeriesGuid == seriesId.ToString() ? seriesConfiguration : config);
                    userNotificationConfig.SeriesConfigurationItems.Remove(userId.ToString());
                    userNotificationConfig.SeriesConfigurationItems.Add(userId.ToString(), updatedUserConfig);
                }
                else // This must be a new entry
                {
                    var seriesName = Plugin.LibraryManager!.GetItemById(seriesId)?.Name;

                    if (seriesName == null)
                    {
                        Plugin.Logger!.LogCritical("An invalid update series configuration request was recieved for User: {UserGuid}, Series: {SeriesGuid}, Optional New Series Name: {OSeriesName}.", userId, seriesId, optionalNewSeriesName ?? "Null");
                        // TODO: have an admin ntfy config option so they get a push if this happens. This should never happen unless someone is poking at the webserver or the dictionary of values is corrupt.
                        return;
                    }

                    var updatedUserConfig = userConfig.Append(new UserSeriesNotificationConfigurationItem()
                    { SeriesName = seriesName!, IsEnabled = isEnabled ?? true, SeriesGuid = seriesId.ToString() });
                    userNotificationConfig.SeriesConfigurationItems.Remove(userId.ToString());
                    userNotificationConfig.SeriesConfigurationItems.Add(userId.ToString(), updatedUserConfig);
                    Plugin.Logger!.LogInformation("An unsubscribe request was successful for User: {UserGuid}, Series: {SeriesGuid}", userId, seriesId);
                }

                Plugin.Instance!.Configuration!.UserNotificationConfigurationJsonString = JsonSerializer.Serialize(userNotificationConfig);
                Plugin.Instance.SaveConfiguration();
            }
            else // User does not have a configuraton yet, add one.
            {
                var seriesName = Plugin.LibraryManager!.GetItemById(seriesId)?.Name;

                if (seriesName == null)
                {
                    Plugin.Logger!.LogCritical("An invalid update series configuration request was recieved for User: {UserGuid}, Series: {SeriesGuid}, Optional New Series Name: {OSeriesName}.", userId, seriesId, optionalNewSeriesName ?? "Null");
                    // TODO: have an admin ntfy config option so they get a push if this happens. This should never happen unless someone is poking at the webserver or the dictionary of values is corrupt.
                    return;
                }

                var newUserConfig = new List<UserSeriesNotificationConfigurationItem>()
            {
                new()
                {
                    SeriesName = seriesName!,
                    IsEnabled = true,
                    SeriesGuid = seriesId.ToString()
                }
            };
                userNotificationConfig.SeriesConfigurationItems.Add(userId.ToString(), newUserConfig);
                Plugin.Instance!.Configuration.UserNotificationConfigurationJsonString = JsonSerializer.Serialize(userNotificationConfig);
                Plugin.Instance.SaveConfiguration();
            }
        }

        public static UserNotificationConfiguration GetUserNotificationConfiguration()
        {
            var jsonString = Plugin.Instance!.Configuration.UserNotificationConfigurationJsonString;
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
                Plugin.Logger!.LogCritical("Error when deserializing UserNotificationConfiguration: {Message}", ex.Message);
                return new UserNotificationConfiguration() { SeriesConfigurationItems = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.IEnumerable<UserSeriesNotificationConfigurationItem>>() };
            }
        }
    }
}

using System;
using System.Globalization;
using System.Linq;
using Jellyfin.Data.Entities;
using JellyfinNotify.Plugin.Data;
using JellyfinNotify.Plugin.Extensions;
using JellyfinNotify.Plugin.Notifications;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities.TV;
using Microsoft.Extensions.Logging;

namespace JellyfinNotify.Plugin.Extensions
{
    /// <summary>
    /// TODO.
    /// </summary>
    public static class SeriesExtensions
    {
        public static EpisodeNotificationDetails? LatestUnairedEpisodeDetails(this Series series, User user)
        {
            var latestUnairedEpisode = series.GetEpisodes(user, new DtoOptions() { EnableUserData = true }, true)
                        .Where(e => (e as Episode)!.IsAiringWithin(TimeSpan.FromDays(30)))
                        .OrderBy(e => e.PremiereDate).FirstOrDefault() as Episode;

            if (latestUnairedEpisode == null)
            {
                return null;
            }

            var expectedPremierDate = latestUnairedEpisode.PremiereDate.ToDateOnly();

            if (expectedPremierDate == null)
            {
                Plugin.Logger!.LogDebug("Episode: {Name} is missing an expected Premier Date", latestUnairedEpisode.Name);
                return null;
            }

            TimeOnly expectedPremierTime;
            if (!TimeOnly.TryParse(latestUnairedEpisode.Series.AirTime, new CultureInfo("en-US"), out expectedPremierTime))
            {
                Plugin.Logger!.LogDebug("Episode: {Name} is from a Series that is missing an expected Premier Time", latestUnairedEpisode.Name);
                return null;
            }

            return latestUnairedEpisode == null ? null :
            new EpisodeNotificationDetails
            {
                Title = latestUnairedEpisode.Name,
                SeriesName = series.Name,
                AirDate = expectedPremierDate.Value,
                AirTime = expectedPremierTime,
                SeriesGuid = series.Id,
                EpisodeGuid = latestUnairedEpisode.Id,
                AttachmentPath = AttachmentRetrieval.GetAttachmentPath("episode", latestUnairedEpisode.Id),
                RecipientUserGuid = user.Id,
                RecipientUsername = user.Username,
                Content = GenerateEpisodeContent(series.Name, expectedPremierDate.Value)
            };
        }

        private static string GenerateEpisodeContent(string seriesName, DateOnly airDate)
        {
            var daysUntil = airDate.DaysFromToday();
            var daysMessage = daysUntil == 1 ? "Tomorrow" : "in " + daysUntil.ToString(CultureInfo.InvariantCulture) + " days";

            return $"{seriesName} airs {daysMessage}!";
        }
    }
}

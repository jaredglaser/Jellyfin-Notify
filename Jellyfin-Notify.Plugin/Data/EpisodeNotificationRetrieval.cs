using System.Collections.Generic;
using System.Linq;
using Jellyfin.Data.Entities;
using Jellyfin.Data.Enums;
using JellyfinNotify.Plugin.Extensions;
using JellyfinNotify.Plugin.Notifications;
using JellyfinNotify.Plugin.UserData;
using MediaBrowser.Controller.Entities;
using Microsoft.Extensions.Logging;
using Episode = MediaBrowser.Controller.Entities.TV.Episode;
using Series = MediaBrowser.Controller.Entities.TV.Series;

namespace JellyfinNotify.Plugin.Data
{
    public static class EpisodeNotificationRetrieval
    {
        public static NextEpisodeTaskResult GetNextEpisodeDetailsForUser(User user)
        {
            Plugin.Logger!.LogInformation("User info: {UserInfo}", user?.Permissions.ToString());
            var query = new InternalItemsQuery
            {
                IncludeItemTypes =
                [
                        BaseItemKind.Episode,
                ],
                User = user,
                IsPlayed = true,
                OrderBy = [(ItemSortBy.Random, SortOrder.Ascending)]
            };

            IEnumerable<Series> series = Plugin.LibraryManager!.QueryItems(query).Items.Select(e => (e as Episode)?.Series).Distinct().Where(s => s != null).Cast<Series>();

            IEnumerable<(Series, EpisodeNotificationDetails)> nextEpisodeDetailsForSeries = [];
            if (series != null)
            {
                nextEpisodeDetailsForSeries = series.Select(s => (s, s.LatestUnairedEpisodeDetails(user!))).Where(s => s.Item2 != null).Cast<(Series, EpisodeNotificationDetails)>();
            }

            foreach (var nextEpisodeDetailForSeries in nextEpisodeDetailsForSeries)
            {
                Data.UserConfiguration.UpdateSeriesConfiguration(nextEpisodeDetailForSeries.Item1.Id, user!.Id, null, nextEpisodeDetailForSeries.Item2.SeriesName);
            }

            return new NextEpisodeTaskResult { Episodes = nextEpisodeDetailsForSeries.Select(n => n.Item2), User = user! };
        }
    }
}

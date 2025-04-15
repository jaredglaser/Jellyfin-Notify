using System;

namespace JellyfinNotify.Plugin.Notifications
{
    public record EpisodeNotificationDetails : BaseNotificationDetails
    {
        public DateOnly AirDate { get; set; }

        public required string SeriesName { get; set; }

        public required Guid SeriesGuid { get; set; }

        public required Guid EpisodeGuid { get; set; }

        public TimeOnly AirTime { get; set; }
    }
}

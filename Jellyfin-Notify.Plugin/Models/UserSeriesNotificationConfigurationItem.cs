namespace JellyfinNotify.Plugin.Models
{
    public class UserSeriesNotificationConfigurationItem
    {
        public required string SeriesGuid { get; set; }

        public bool IsEnabled { get; set; }

        public required string SeriesName { get; set; }
    }
}

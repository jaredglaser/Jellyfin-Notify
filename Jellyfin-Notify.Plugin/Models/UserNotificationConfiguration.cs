using System.Collections.Generic;

namespace JellyfinNotify.Plugin.Models
{
    public class UserNotificationConfiguration
    {
        // Dictionary for Config Values. Key is the User's id GUID.
        public required Dictionary<string, IEnumerable<UserSeriesNotificationConfigurationItem>> SeriesConfigurationItems { get; init; }
    }
}

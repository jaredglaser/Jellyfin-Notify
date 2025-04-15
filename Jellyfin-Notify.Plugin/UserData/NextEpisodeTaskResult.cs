using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jellyfin.Data.Entities;
using JellyfinNotify.Plugin.Notifications;

namespace JellyfinNotify.Plugin.UserData
{
    public record NextEpisodeTaskResult
    {
        public required User User { get; set; }

        public required IEnumerable<EpisodeNotificationDetails> Episodes { get; set; }
}
}

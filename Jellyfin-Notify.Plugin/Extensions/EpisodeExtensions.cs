using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.TV;

namespace JellyfinNotify.Plugin.Extensions
{
    /// <summary>
    /// TODO.
    /// </summary>
    public static class EpisodeExtensions
    {
        /// <summary>
        /// TODO.
        /// </summary>
        /// <param name="episode">Instance of the <see cref="Episode"/> interface.</param>
        /// <param name="timespan">Instance of the <see cref="TimeSpan"/> interface.</param>
        /// <returns> if episode is airing within the timespan. </returns>
        public static bool IsAiringWithin(this Episode episode, TimeSpan timespan)
        {
            if (episode.PremiereDate.HasValue && episode.PremiereDate.Value.ToLocalTime() > DateTime.Now)
            {
                if (episode.PremiereDate.Value.ToLocalTime() - DateTime.Now <= timespan)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

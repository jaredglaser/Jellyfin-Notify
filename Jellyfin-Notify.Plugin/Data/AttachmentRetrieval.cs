using System;
using System.Linq;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;

namespace JellyfinNotify.Plugin.Data
{
    public static class AttachmentRetrieval
    {
        private static ILibraryManager LibraryManager => Plugin.LibraryManager!;

        public static string? GetAttachmentPath(string type, Guid attachmentGuid)
        {
            if (type.Equals("episode", StringComparison.OrdinalIgnoreCase))
            {
                var episodeImage = LibraryManager!.GetItemById<Episode>(attachmentGuid)?.ImageInfos.Where(i => i.Type == MediaBrowser.Model.Entities.ImageType.Thumb).OrderByDescending(i => i.DateModified).FirstOrDefault();

                if (episodeImage == null)
                {
                    var fallbackImage = LibraryManager!.GetItemById<Episode>(attachmentGuid)?.Series.ImageInfos.Where(s => s.Type == MediaBrowser.Model.Entities.ImageType.Thumb).OrderByDescending(i => i.DateModified).FirstOrDefault();
                    return fallbackImage?.Path;
                }

                return episodeImage.Path;
            }

            return null;
        }
    }
}

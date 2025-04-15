using System;

namespace JellyfinNotify.Plugin.Notifications
{
    public record BaseNotificationDetails
    {
        public required string Title { get; set; }

        public required string Content { get; set; }

        public required Guid RecipientUserGuid { get; set; }

        public required string RecipientUsername { get; set; }

        public string? AttachmentPath { get; set; }
    }
}

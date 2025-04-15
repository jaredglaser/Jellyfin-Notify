using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JellyfinNotify.Plugin.Configuration;
using Microsoft.Extensions.Logging;

namespace JellyfinNotify.Plugin.Notifications
{
    public class NotificationFactory : INotificationFactory
    {
        public NotificationFactory()
        {
            var client = new HttpClient();
            Client = client;
        }

        private HttpClient Client { get; set; }

        private PluginConfiguration Configuration => Plugin.Instance!.Configuration;

        private ILogger Logger => Plugin.Logger!;

        public async Task SendNotification(BaseNotificationDetails notificationItem, NotificationDeliveryMethod deliveryMethod, CancellationToken cancellationToken)
        {
            switch (deliveryMethod)
            {
                case NotificationDeliveryMethod.NtfyPush:
                    switch (notificationItem)
                    {
                        case EpisodeNotificationDetails episodeNotification:
                            await SendNtfyNotification(episodeNotification, cancellationToken).ConfigureAwait(false);
                            break;
                        default:
                            break;
                    }

                    break;
                default:
                    break;
            }
        }

        private HttpRequestMessage GenerateNtfyEpisodePushNotification(EpisodeNotificationDetails episodeNotification)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, Configuration.NtfyUrl + $"/Jellyfin-Notify-{episodeNotification.RecipientUsername}");
            if (Configuration?.NtfyUser != null && Configuration?.NtfyPass != null)
            {
                var basicAuth = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Configuration.NtfyUser}:{Configuration.NtfyPass}"));
                httpRequest.Headers.Add("Authorization", basicAuth);
            }

            if (episodeNotification.AttachmentPath != null)
            {
                httpRequest.Content = new StreamContent(File.OpenRead(episodeNotification.AttachmentPath));
                httpRequest.Headers.Add("Filename", Path.GetFileName(episodeNotification.AttachmentPath));
                httpRequest.Headers.Add("Actions", $"http, Unsubscribe, {Configuration!.JellyfinInstanceUrl}/PluginApi/Unsubscribe?userGuidString={episodeNotification.RecipientUserGuid}&seriesGuidString={episodeNotification.SeriesGuid}, method=GET");
            }

            // TODO: make this configurable
            httpRequest.Headers.Add("Title", "New Episode Alert");
            httpRequest.Headers.Add("Message", episodeNotification.Content);

            return httpRequest;
        }

        private async Task SendNtfyNotification(EpisodeNotificationDetails episodeNotificationDetails, CancellationToken cancellationToken)
        {
            try
            {
                var httpRequest = GenerateNtfyEpisodePushNotification(episodeNotificationDetails);
                var ntfyResult = await Client!.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
                Logger.LogInformation("Sent Ntfy Notification Status: {Status}, User: {User}, Message: {Message}", ntfyResult.StatusCode, episodeNotificationDetails.RecipientUsername, episodeNotificationDetails.Content);
            }
            catch (UriFormatException)
            {
                Logger!.LogError("Url for Ntfy Service is not valid. Example: http://mydomain.com/subscriptionTopic");
            }
            catch (Exception ex)
            {
                Logger!.LogError("{Message}", ex.Message);
            }
        }
    }
}

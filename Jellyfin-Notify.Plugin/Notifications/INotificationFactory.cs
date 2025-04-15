using System.Threading;
using System.Threading.Tasks;

namespace JellyfinNotify.Plugin.Notifications
{
    public interface INotificationFactory
    {
        public Task SendNotification(BaseNotificationDetails notificationItem, NotificationDeliveryMethod deliveryMethod, CancellationToken cancellationToken);
    }
}

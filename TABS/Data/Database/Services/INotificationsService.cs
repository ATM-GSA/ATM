using System.Collections.Generic;
using System.Threading.Tasks;

namespace TABS.Data
{
    public interface INotificationsService
    {
        Task<bool> EnqueueNotification(int UserID, Notification notif);
        Task<NotificationsQueue> GetNotificationsQueue(int UserID);
        Task<bool> RemoveNotifications(int UserID, List<Notification> notifs);
    }
}
using Newtonsoft.Json;
using System.Collections.Generic;

namespace TABS.Data
{
    /// <summary>
    /// Default user notifications
    /// </summary>
    public class DefaultNotificationQueue
    {
        public static NotificationsQueue GetDefaultNotificationQueue()
        {
            return new()
            {
                notifications = new List<Notification>() { }
            };
        }

        public static string GetSerializedString()
        {
            return JsonConvert.SerializeObject(GetDefaultNotificationQueue());
        }
    }
}


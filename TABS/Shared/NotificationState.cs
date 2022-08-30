using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TABS.Data;

namespace TABS.Shared
{
    public class NotificationState
    {
        public event Action OnChange;
        NotificationsQueue NotificationsQueue = new NotificationsQueue();
    }
}

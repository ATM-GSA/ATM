using System;

namespace TABS.Data
{
    public class ModuleAlert : Notification
    {
        public int applicationID { get; set; }
        public int moduleType { get; set; }
        public DateTime nextUpdate { get; set; }
    }
}

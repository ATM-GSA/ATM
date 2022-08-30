using System.Collections.Generic;

namespace TABS.Data
{
    public class Preferences
    {
        public List<int> favouriteApplications { get; set; }
        public string language { get; set; }
        public bool gridView { get; set; }
        public string theme { get; set; }
        public List<string> recentSearches { get; set; }
        public List<WidgetPreference> widgets { get; set; }
        public NotificationPreferences notificationPreferences { get; set; }
    }

    public class WidgetPreference
    {
        public int id { get; set; }
        public bool fullWidth { get; set; }
    }

    public class NotificationPreferences
    {
        public bool updates { get; set; }
        public bool reminders { get; set; }
        public bool emailOff { get; set; }

        /* 1 = Daily, 2 = Weekly */
        public int emailFreq { get; set; }

        /* Refer to DayOfWeek enum */
        public int emailWeekDigDay { get; set; }
    }
}

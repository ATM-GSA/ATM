using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TABS.Data
{
    /// <summary>
    /// Default user preferences
    /// </summary>
    public class DefaultPreferences
    {
        public static Preferences GetDefaultPreferences()
        {
            return new()
            {
                favouriteApplications = new List<int>(),
                widgets = new List<WidgetPreference>() { new() { id = 1, fullWidth = false }, new() { id = 2, fullWidth = false } }, // add the calendar and reminders by default
                language = "en-US",
                gridView = false,
                theme = "Light",
                notificationPreferences = new NotificationPreferences() { updates = true, reminders = true, emailOff = false, emailFreq = 1, emailWeekDigDay = (int)DayOfWeek.Friday },
                recentSearches = new List<string>()
            };
        }

        public static string GetSerializedString()
        {
            return JsonConvert.SerializeObject(GetDefaultPreferences());
        }
    }
}
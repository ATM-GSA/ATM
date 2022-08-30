using System;
using System.Collections.Generic;
using TABS.Data;


namespace TABS.Shared
{
    public class ModuleStatusTag
    {
        public int Value { get; set; }
        public string Name { get; set; }
        public string DisplayColor { get; set; }

        public static List<ModuleStatusTag> DefaultTags = getDefaultTags();

        public static List<ModuleStatusTag> getDefaultTags()
        {
            List<ModuleStatusTag> defaultTags = new List<ModuleStatusTag> {
                new ModuleStatusTag { Value = 0, Name = getName(0), DisplayColor = getDisplayColor(0) }
            };
            foreach (int status in Enum.GetValues(typeof(ModuleStatusFlagEnum)))
            {
                defaultTags.Add(new ModuleStatusTag { Value = status, Name = getName(status), DisplayColor = getDisplayColor(status) });
            }
            return defaultTags;

        }

        public static string getDisplayColor(int status)
        {
            switch (status)
            {
                default: // when status == 0 (up to date)
                    return "green";
                case (int)ModuleStatusFlagEnum.NeedsAttention:
                    return "red";
            }
        }

        public static string getName(int status)
        {
            switch (status)
            {
                default: // when status == 0 (up to date)
                    return "UpToDate";
                case (int)ModuleStatusFlagEnum.NeedsAttention:
                    return "NeedsAttention";
            }
        }
    }
}

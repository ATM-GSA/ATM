using System.Collections.Generic;

namespace TABS.Data.Emails
{
    public class EmailTemplateService
    {
        private static readonly Dictionary<string, ITemplate> TemplateDict = new()
        {
            { "Approval", new Approval() },
            { "Denied", new Denied() },
            { "Announcement", new Announcement() },
            { "DigestWeekly", new DigestWeekly() },
            { "DigestDaily", new DigestDaily() },
            { "Alert", new Alert()},
        };

        public static ITemplate GetTemplate(string Name)
        {
            return TemplateDict.ContainsKey(Name) ? TemplateDict[Name] : null;
        }
    }
}

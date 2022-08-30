using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Quartz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TABS.Data
{
    [DisallowConcurrentExecution]
    public class EmailDigestJob : IJob
    {
        private readonly IServiceProvider _provider;

        public EmailDigestJob(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            DateTime now = DateTime.Now;
            DateTime lastWeek = now.AddDays(-7);

            // Don't run this job on weekends
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday) return;

            Console.WriteLine($"[{DateTime.Now}] Generating Email Digests...");

            // Create a new scope
            using (var scope = _provider.CreateScope())
            {
                // Resolve the Scoped service
                NotificationsService notifService = scope.ServiceProvider.GetService<NotificationsService>();
                ApplicationService appService = scope.ServiceProvider.GetService<ApplicationService>();
                UserService userService = scope.ServiceProvider.GetService<UserService>();
                EmailService emailService = scope.ServiceProvider.GetService<EmailService>();
                IStringLocalizer localizer = scope.ServiceProvider.GetService<IStringLocalizer<App>>();
                Global global = scope.ServiceProvider.GetService<Global>();

                List<User> users = await userService.GetApprovedUsers();
                List<string> langs = new() { "en-US", "fr-CA" };
                Dictionary<int, Application> apps = new();

                foreach (User user in users)
                {
                    Preferences pref = user.GetPreferences();
                    NotificationsQueue notificationsQueue = user.GetNotifications();

                    if (pref == null || notificationsQueue == null) continue;

                    // skip this user if they have emails turned off
                    if (pref.notificationPreferences.emailOff) continue;

                    EmailFrequencyEnum digestType = EmailFrequencyEnum.Daily;
                    if (pref.notificationPreferences.emailFreq == (int)EmailFrequencyEnum.Weekly)
                    {
                        // send weekly digests only the preferred day
                        if (now.DayOfWeek != (DayOfWeek)pref.notificationPreferences.emailWeekDigDay) continue;
                        digestType = EmailFrequencyEnum.Weekly;
                    }

                    Dictionary<string, List<object>> reminders = new() { { "en-US", new() }, { "fr-CA", new() } };
                    Dictionary<string, List<object>> updates = new() { { "en-US", new() }, { "fr-CA", new() } };
                    Dictionary<int, Application> appDict = new();
                    Dictionary<Tuple<int, ModuleTypeEnum>, int> updatesCountDict = new();

                    // structure data for digest
                    foreach (Notification notification in notificationsQueue.notifications)
                    {
                        NotificationTypeEnum notificationType = (NotificationTypeEnum)notification.type;

                        // skip announcements
                        if (notificationType == NotificationTypeEnum.Announcement) continue;


                        // check the timestamp
                        // if timestamp condition is not met, skip this notification
                        if (digestType == EmailFrequencyEnum.Daily)
                        {
                            if (notification.timestamp.Date != now.Date) continue;
                        }
                        else if (digestType == EmailFrequencyEnum.Weekly)
                        {
                            if (notification.timestamp.Date < lastWeek.Date || notification.timestamp.Date > now.Date) continue;
                        }


                        if (notificationType == NotificationTypeEnum.ModuleAlert)
                        {
                            // reminder
                            if (notification is ModuleAlert moduleAlert)
                            {

                                Application app;
                                if (appDict.ContainsKey(moduleAlert.applicationID))
                                {
                                    app = appDict[moduleAlert.applicationID];
                                }
                                else
                                {
                                    app = await appService.GetApplicationByID(moduleAlert.applicationID, new List<ModuleTypeEnum> { ModuleTypeEnum.ApplicationIdentification });
                                    appDict[app.ApplicationID] = app;
                                }

                                ModuleTypeEnum moduleType = (ModuleTypeEnum)moduleAlert.moduleType;

                                if (app == null || app.Identification == null || moduleType == ModuleTypeEnum.Undefined) continue;

                                // create reminder for each language
                                langs.ForEach(lang =>
                                {
                                    CultureInfo.CurrentUICulture = new CultureInfo(lang);
                                    reminders[lang].Add(new
                                    {
                                        Link = global.GetModuleDetailsURL(app.ShortID, moduleType.ToString()),
                                        AppName = $"{app.Identification.Name} ({app.Identification.APMID})",
                                        Module = localizer[moduleType.ToString()],
                                        UpdateDate = moduleAlert.nextUpdate.ToString("MMMM dd, yyyy"),
                                    });
                                });
                            }
                        }
                        else if (notificationType == NotificationTypeEnum.ModuleUpdate)
                        {
                            // update
                            if (notification is ModuleUpdate update)
                            {
                                Application app;
                                if (appDict.ContainsKey(update.applicationID))
                                {
                                    app = appDict[update.applicationID];
                                }
                                else
                                {
                                    app = await appService.GetApplicationByID(update.applicationID, new List<ModuleTypeEnum> { ModuleTypeEnum.ApplicationIdentification });
                                    appDict[app.ApplicationID] = app;
                                }

                                ModuleTypeEnum moduleType = (ModuleTypeEnum)update.moduleType;

                                if (app == null || app.Identification == null || moduleType == ModuleTypeEnum.Undefined) continue;

                                Tuple<int, ModuleTypeEnum> key = new(app.ApplicationID, moduleType);
                                if (updatesCountDict.ContainsKey(key))
                                {
                                    // increase the update count
                                    updatesCountDict[key] += 1;
                                }
                                else
                                {
                                    // init the update count
                                    updatesCountDict[key] = 1;
                                }
                            }
                        }
                    }

                    // convert the dictionary into a list of objects for each language
                    langs.ForEach(lang =>
                    {
                        CultureInfo.CurrentUICulture = new CultureInfo(lang);
                        updates[lang] = updatesCountDict.OrderByDescending(pair => pair.Value).Select(pair => new
                        {
                            Link = global.GetModuleDetailsURL(appDict[pair.Key.Item1].ShortID, pair.Key.Item2.ToString()),
                            AppName = $"{appDict[pair.Key.Item1].Identification.Name} ({appDict[pair.Key.Item1].Identification.APMID})",
                            Module = localizer[pair.Key.Item2.ToString()],
                            NumUpdates = pair.Value
                        }).Cast<object>().ToList();
                    });

                    // only send email if they have at least one update or reminder
                    int totalCount = 0;
                    langs.ForEach(lang =>
                    {
                        totalCount += (reminders[lang].Count + updates[lang].Count);
                    });
                    if (totalCount > 0)
                    {
                        object model = new
                        {
                            Name = user.Name,
                            StartDate = lastWeek.ToString("MMMM dd, yyyy"),
                            EndDate = now.ToString("MMMM dd, yyyy"),
                            Reminders = reminders,
                            Updates = updates,
                            Env = context.JobDetail.JobDataMap.GetString("env")
                        };

                        try
                        {
                            await emailService.SendTemplatedEmail(user.Email, digestType == EmailFrequencyEnum.Daily ? "DigestDaily" : "DigestWeekly", model);
                            Console.WriteLine($"[{DateTime.Now}] Sent {digestType} Digest to {user.Name}");
                        }
                        catch
                        {
                            /* No email for this person ¯\_(ツ)_/¯ */
                            Console.WriteLine($"[{DateTime.Now}] Failed to Send {digestType} Digest to {user.Name}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now}] Nice! No updates or reminders for {user.Name}");
                    }
                }
            }
        }
    }
}
using Eccc.Sali;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TABS.Audit;

namespace TABS.Data
{
    [DisallowConcurrentExecution]
    public class ModuleAlertJob : IJob
    {
        private readonly IServiceProvider _provider;

        public ModuleAlertJob(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            DateTime now = DateTime.Now;

            // Don't run this job on weekends
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday) return;

            bool sendMail = true;
            if (context.JobDetail.JobDataMap.GetString("sendMail") == "false")
            {
                sendMail = false;
            }

            Console.WriteLine($"[{DateTime.Now}] Executing database scan for module alerts ...");

            // Create a new scope
            using (var scope = _provider.CreateScope())
            {
                // Resolve the Scoped service
                ApplicationService appService = scope.ServiceProvider.GetService<ApplicationService>();
                UserService userService = scope.ServiceProvider.GetService<UserService>();
                NotificationsService notifService = scope.ServiceProvider.GetService<NotificationsService>();
                AuditLogService auditLogService = scope.ServiceProvider.GetService<AuditLogService>();
                EmailService emailService = scope.ServiceProvider.GetService<EmailService>();
                IStringLocalizer localizer = scope.ServiceProvider.GetService<IStringLocalizer<App>>();
                Global global = scope.ServiceProvider.GetService<Global>();

                Dictionary<int, Dictionary<string, List<object>>> userReminderDict = new();
                List<string> langs = new(){ "en-US", "fr-CA" };
                List<int> appIds = await appService.GetAllApplicationsIDs();

                var watch = new Stopwatch();
                watch.Start();

                foreach (int appId in appIds)
                {
                    Application app = await appService.GetApplicationByID(appId);

                    // skip deleted and archived apps
                    if (app == null || app.IsDeleted || app.IsArchived || !app.IsComplete)
                    {
                        continue;
                    }

                    Dictionary<ModuleTypeEnum, Module> modules = app.GetModules();
                    List<Module> outOfDateModules = new();

                    foreach (KeyValuePair<ModuleTypeEnum, Module> pair in modules)
                    {
                        Module module = pair.Value;
                        if (module != null)
                        {
                            int timeDiff = (module.NextUpdate.Date - now.Date).Days;
                            if (timeDiff == 7)
                            {
                                // Set the module to needs attention and update in the db 
                                try
                                {
                                    module.StatusFlags = (int)ModuleStatusFlagEnum.NeedsAttention;
                                    var dbContext = DBContextFactory.CreateInstance();
                                    dbContext.Update(module);
                                    await dbContext.SaveChangesAsync();

                                    // the below won't work because this job is not actually a user, so it doens't have an auth state
                                    await auditLogService.LogAsync(app, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[{DateTime.Now}] Exception when updating module status: {ex}");
                                }

                                // need to send module alert reminder
                                outOfDateModules.Add(module);
                            }
                        }
                    }

                    if (outOfDateModules.Count > 0)
                    {
                        List<ApplicationSubscription> subs = await DBContextFactory.CreateInstance().ApplicationSubscriptions
                                                                                   .Where(sub => sub.ApplicationID == app.ApplicationID)
                                                                                   .AsNoTracking()
                                                                                   .ToListAsync();
                        if (subs.Count > 0)
                        {
                            foreach (Module module in outOfDateModules)
                            {
                                foreach (ApplicationSubscription sub in subs)
                                {
                                    User user = await userService.GetUserByUserID(sub.UserID);

                                    NotificationsQueue notificationsQueue = user.GetNotifications();
                                    Preferences pref = user.GetPreferences();

                                    if (pref == null || notificationsQueue == null) continue;

                                    // check if module alert already exists to avoid spam
                                    if (notificationsQueue.notifications.Any(notif =>
                                    {
                                        if (notif is not ModuleAlert al) return false;
                                        return al.nextUpdate == module.NextUpdate && al.applicationID == app.ApplicationID && al.moduleType == (int)module.GetModuleType();
                                    }))
                                    {
                                        // this alert is already in this user's notification queue, so we don't enqueue another one
                                        Console.WriteLine($"[{DateTime.Now}] Skipping alert for {user.Name} for {app.Identification.Name} ({module.GetModuleType()})");
                                        continue;
                                    }

                                    ModuleAlert alert = new()
                                    {
                                        timestamp = now,
                                        type = (int)NotificationTypeEnum.ModuleAlert,
                                        applicationID = app.ApplicationID,
                                        moduleType = (int)module.GetModuleType(),
                                        nextUpdate = module.NextUpdate
                                    };

                                    Console.WriteLine($"[{DateTime.Now}] Queueing alert for {app.Identification.Name} ({module.GetModuleType()}) to {user.Name}");
                                    await notifService.EnqueueNotification(user.UserID, alert);

                                    if (!userReminderDict.ContainsKey(user.UserID))
                                    {
                                        userReminderDict[user.UserID] = new();
                                        langs.ForEach(lang =>
                                        {
                                            userReminderDict[user.UserID][lang] = new();
                                        });
                                    }

                                    // localize for each language
                                    langs.ForEach(lang =>
                                    {
                                        CultureInfo.CurrentUICulture = new CultureInfo(lang);
                                        userReminderDict[user.UserID][lang].Add(new
                                        {
                                            Link = global.GetModuleDetailsURL(app.ShortID, module.GetModuleType().ToString()),
                                            AppName = $"{app.Identification.Name} ({app.Identification.APMID})",
                                            Module = localizer[module.GetModuleType().ToString()]
                                        }
                                        );
                                    });
                                }
                            }
                        }
                    }
                }

                watch.Stop();
                Console.WriteLine($"[{DateTime.Now}] Took {watch.ElapsedMilliseconds} ms to scan database");

                if (sendMail)
                {
                    watch.Restart();

                    // send out emails
                    foreach (KeyValuePair<int, Dictionary<string, List<object>>> pair in userReminderDict)
                    {
                        User user = await userService.GetUserByUserID(pair.Key);
                        if (user == null) continue;

                        Preferences pref = user.GetPreferences();
                        if (pref == null || pref.notificationPreferences.emailOff) continue;

                        object model = new
                        {
                            Name = user.Name,
                            UpdateDate = now.AddDays(7).ToString("MMMM dd, yyyy"),
                            Reminders = pair.Value,
                            Env = context.JobDetail.JobDataMap.GetString("env")
                        };

                        try
                        {
                            await emailService.SendTemplatedEmail(user.Email, "Alert", model);
                            Console.WriteLine($"[{DateTime.Now}] Sent Module Alert email to {user.Name}");
                        }
                        catch
                        {
                            /* No email for this person ¯\_(ツ)_/¯ */
                            Console.WriteLine($"[{DateTime.Now}] Failed to send Module Alert email to {user.Name}");
                        }
                    }

                    watch.Stop();
                    Console.WriteLine($"[{DateTime.Now}] Took {watch.ElapsedMilliseconds} ms to send out emails");
                }
            }
        }
    }
}
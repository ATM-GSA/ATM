using Eccc.Sali;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TABS.Audit;

namespace TABS.Data
{
    public class NotificationsService : INotificationsService
    {
        private const int MAX_NOTIFICATION_COUNT = 40;

        private readonly AuditLogService _auditLogService;

        public NotificationsService(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task<bool> EnqueueNotification(int UserID, Notification notif)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            User user = await dbContext.Users.Where(u => u.UserID == UserID).AsNoTracking().FirstOrDefaultAsync();

            if (user == null)
            {
                return false;
            }

            NotificationsQueue NotificationsQueue = user.GetNotifications();
            NotificationsQueue.notifications.Insert(0, notif);
            if (NotificationsQueue.notifications.Count > MAX_NOTIFICATION_COUNT)
            {
                // remove the last notification
                NotificationsQueue.notifications.RemoveAt(NotificationsQueue.notifications.Count - 1);
            }

            user.Notifications = JsonConvert.SerializeObject(NotificationsQueue, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            // Save changes
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
            // TODO: when the module alert job runs this, it doesn't have an auth state so this errors
            await (_auditLogService?.LogAsync(user, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);

            Console.WriteLine($"Sent notification to: {user.Name} ({user.UserID})");

            return true;
        }

        public async Task<bool> RemoveNotifications(int UserID, List<Notification> notifs)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            User user = await dbContext.Users.Where(u => u.UserID == UserID).AsNoTracking().FirstOrDefaultAsync();

            if (user == null)
            {
                return false;
            }

            NotificationsQueue NotificationsQueue = user.GetNotifications();
            List<string> notifToRemoveIds = notifs.Select(notifToRemove => notifToRemove.id).ToList();
            NotificationsQueue.notifications.RemoveAll(notif => notifToRemoveIds.Contains(notif.id));
            user.Notifications = JsonConvert.SerializeObject(NotificationsQueue, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            // Save changes
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
            // TODO: when the module alert job runs this, it doesn't have an auth state so this errors
            await (_auditLogService?.LogAsync(user, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);

            return true;
        }

        public async Task<bool> RewriteNotifications(int UserID, NotificationsQueue NotificationsQueue)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            User user = await dbContext.Users.Where(u => u.UserID == UserID).AsNoTracking().FirstOrDefaultAsync();

            if (user == null)
            {
                return false;
            }


            user.Notifications = JsonConvert.SerializeObject(NotificationsQueue, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            // Save changes
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();
            
            // TODO: when the module alert job runs this, it doesn't have an auth state so this errors
            await (_auditLogService?.LogAsync(user, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);

            return true;
        }

        public async Task<NotificationsQueue> GetNotificationsQueue(int UserID)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            User user = await dbContext.Users.Where(u => u.UserID == UserID).AsNoTracking().FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            return user.GetNotifications();
        }
    }
}
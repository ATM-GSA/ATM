using Eccc.Sali;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TABS.Audit;

namespace TABS.Data
{
    public class UserService : IUserService
    {

        private readonly AuditLogService _auditLogService;

        public UserService(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        #region Get approved users
        /// <summary>
        /// Retrieve all of the approved users
        /// </summary>
        /// <returns>List of approved users</returns>
        public async Task<List<User>> GetApprovedUsers()
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.Set<User>().Include(u => u.Role).Where(user => user.Approved).ToListAsync();
        }
        #endregion

        #region Get users needing approval
        /// <summary>
        /// Retrieve all of the users needing approval
        /// </summary>
        /// <returns>List of users pending approval</returns>
        public async Task<List<User>> GetNeedsApproval()
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.Set<User>().Include(u => u.Role).Where(user => !user.Approved && !user.IsDeactivated).ToListAsync();
        }
        #endregion

        #region Get deactivated users
        /// <summary>
        /// Retrieve all of the users that are deactivated
        /// </summary>
        /// <returns>List of deactivated users</returns>
        public async Task<List<User>> GetDeactivatedUsers()
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.Set<User>().Include(u => u.Role).Where(user => user.IsDeactivated).ToListAsync();
        }
        #endregion

        #region Get user by AdID
        /// <summary>
        /// Retrieve a specific user by their Ad ID
        /// </summary>
        /// <returns>User object</returns>
        public async Task<User> GetUserByAdID(string AdID)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.Users.Include(u => u.Role).Where(user => user.AdID == AdID).FirstOrDefaultAsync();
        }
        #endregion

        #region Get user by UserID
        /// <summary>
        /// Retrieve a specific user by their UserID
        /// </summary>
        /// <returns>User object</returns>
        public async Task<User> GetUserByUserID(int UserID)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.Users.Include(u => u.Role).Where(user => user.UserID == UserID).FirstOrDefaultAsync();
        }
        #endregion

        #region Get users like name
        /// <summary>
        /// Retrieve a list of users whose name is like the given name
        /// </summary>
        /// <returns>List of user objects</returns>
        public async Task<List<User>> SearchUsersByName(string Name)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.Users.Include(u => u.Role).Where(user => EF.Functions.Like(user.Name, string.Format("%{0}%", Name))).ToListAsync();
        }
        #endregion

        #region Get pending app
        /// <summary>
        /// Get this users's app that is pending completion if there is one
        /// </summary>
        /// <returns>List of user objects</returns>
        public async Task<Application> GetPendingApplication(int UserID)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.Applications.Include(a => a.Identification).Where(app => app.CreateByUserID == UserID && !app.IsComplete).FirstOrDefaultAsync();
        }
        #endregion

        #region Approve user
        /// <summary>
        /// Approve user
        /// </summary>
        /// <param name="user">User to be approved</param>
        /// <param name="_admin">Admin that is approving this user</param>
        /// <returns>bool</returns>
        public async Task<bool> ApproveUser(User user, User _admin)
        {
            using var dbContext = DBContextFactory.CreateInstance();

            user.Approved = true;
            user.Role.Expires = DateTime.Now.AddYears(1);

            dbContext.Update(user);
            await dbContext.SaveChangesAsync();
            await (_auditLogService?.LogAsync(user, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);

            dbContext.Update(user.Role);
            await dbContext.SaveChangesAsync();
            await (_auditLogService?.LogAsync(user.Role, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);

            return true;
        }
        #endregion

        #region Delete user
        /// <summary>
        /// Delete user completely from the database
        /// </summary>
        /// <param name="user">User to be deleted</param>
        /// <param name="_admin">Admin that is deleting this user</param>
        /// <returns>bool</returns>
        public async Task<bool> DeleteUser(User user, User _admin)
        {
            using var dbContext = DBContextFactory.CreateInstance();

            // If user is an admin, call DeleteAdmin instead
            bool isAdmin = user.Role.PermissionLevel == (int)RoleEnums.Roles.Admin;
            user.Role.PermissionLevelBeforeDelete = user.Role.PermissionLevel;

            // Deactivate user if not already
            if (!user.IsDeactivated)
            {
                await DeactivateUser(user, _admin);
            }

            // Only completely delete the user if they are not admin
            if (!isAdmin)
            {
                // Clean up contacts
                List<Contact> contacts = await dbContext.Set<Contact>().ToListAsync();
                foreach (Contact contact in contacts)
                {
                    List<ContactInfo> contactInfos = new List<ContactInfo> { contact.GetManager(), contact.GetTeamLead(), contact.GetTechLead(), contact.GetClientLead(), contact.GetClientManager() };
                    bool modified = false;
                    for (int i = 0; i < contactInfos.Count; i++)
                    {
                        ContactInfo contactInfo = contactInfos[i];
                        if (contactInfo.userID == user.UserID)
                        {
                            switch (i)
                            {
                                case 0: // Manager
                                    contact.Manager = DefaultContactInfo.GetSerializedString();
                                    break;
                                case 1: // Team Lead
                                    contact.TeamLead = DefaultContactInfo.GetSerializedString();
                                    break;
                                case 2: // Tech Lead
                                    contact.TechLead = DefaultContactInfo.GetSerializedString();
                                    break;
                                case 3: // Client Lead
                                    contact.ClientLead = DefaultContactInfo.GetSerializedString();
                                    break;
                                case 4: // Client Manager
                                    contact.ClientManager = DefaultContactInfo.GetSerializedString();
                                    break;
                            }
                            modified = true;
                        }
                    }
                    if (modified)
                    {
                        // Save changes
                        dbContext.Update(contact);
                        await dbContext.SaveChangesAsync();
                        await (_auditLogService?.LogAsync(contact, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
                    }
                }

                // clean up in progress applications
                List<Application> inProgressApps = await dbContext.Applications.Where(app => !app.IsComplete && app.CreateByUserID == user.UserID).ToListAsync();
                ApplicationService appService = new ApplicationService(_auditLogService);
                foreach (Application app in inProgressApps)
                {
                    await appService.DeleteAsync(app, true);
                }

                dbContext.Remove(user);
                await dbContext.SaveChangesAsync();
                await (_auditLogService?.LogAsync(user, AuditLog.ActionCategory.DataDelete, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
            }
            return true;
        }
        #endregion

        #region Deactivate user
        /// <summary>
        /// Deactivate user
        /// </summary>
        /// <param name="user">User to be deactivated</param>
        /// <param name="_admin">Admin that is deactivating this user</param>
        /// <returns>bool</returns>
        public async Task<bool> DeactivateUser(User user, User _admin)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            user.Role.PermissionLevelBeforeDelete = user.Role.PermissionLevel;
            user.Role.PermissionLevel = (int)RoleEnums.Roles.Deactivated;
            user.Approved = false;
            user.IsDeactivated = true;

            // Save changes
            dbContext.Update(user);
            await dbContext.SaveChangesAsync();
            await (_auditLogService?.LogAsync(user, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);

            // Unsubscribe the user from all app updates
            List<ApplicationSubscription> subs = await dbContext.ApplicationSubscriptions
                                                                .Include(sub => sub.Application)
                                                                .Include(sub => sub.User)
                                                                .Where(sub => sub.UserID == user.UserID)
                                                                .ToListAsync();
            foreach (ApplicationSubscription sub in subs)
            {
                sub.User = null;
                sub.Application = null;
                dbContext.Remove(sub);
                await dbContext.SaveChangesAsync();
                await (_auditLogService?.LogAsync(sub, AuditLog.ActionCategory.DataDelete, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
            }
            return true;
        }
        #endregion

        #region Activate user
        /// <summary>
        /// Activate user
        /// </summary>
        /// <param name="user">User to be activated</param>
        /// <param name="_admin">Admin that is activating this user</param>
        /// <returns>bool</returns>
        public async Task<bool> ActivateUser(User user, User _admin)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            user.Role.PermissionLevel = user.Role.PermissionLevelBeforeDelete;
            user.Approved = true;
            user.IsDeactivated = false;

            // Save changes
            dbContext.Update(user);
            await dbContext.SaveChangesAsync();
            await (_auditLogService?.LogAsync(user, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
            return true;
        }
        #endregion
    }
}
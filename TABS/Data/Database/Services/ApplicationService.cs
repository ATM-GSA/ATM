using Eccc.Sali;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TABS.Audit;

namespace TABS.Data
{
    /// <summary>
    /// Used to fetch application entities from the database.
    /// Automatically populates the applications module fields and the module's sub-fields.
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        private readonly AuditLogService _auditLogService;

        public ApplicationService(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        /// <summary>
        /// Build an application query given a list of modules to include
        /// <param name="context">TABS DB context to be used</param>
        /// <param name="includeModules">Optional list of modules to include. If it is null, all modules will be included</param>
        /// </summary>
        /// <returns></returns>
        private IQueryable<Application> GenerateApplicationSet(TABSDBContext dbContext, List<ModuleTypeEnum> includeModules = null)
        {
            if (includeModules == null)
            {
                // if it is null, include all modules
                includeModules = ModuleTypeEnumExtensions.GetDisplayedModules().Cast<ModuleTypeEnum>().ToList();
                includeModules.Add(ModuleTypeEnum.Dependencies); // manually add dependency type
            }

            IQueryable<Application> appSet = dbContext.Applications.AsQueryable();

            appSet = appSet.Include(a => a.CreateByUser);

            foreach (ModuleTypeEnum moduleID in includeModules)
            {
                switch (moduleID)
                {
                    case ModuleTypeEnum.ApplicationIdentification:
                        appSet = appSet.Include(a => a.Identification);
                        break;
                    case ModuleTypeEnum.Architecture:
                        appSet = appSet.Include(a => a.Architecture);
                        break;
                    case ModuleTypeEnum.Contacts:
                        appSet = appSet.Include(a => a.Contact);
                        break;
                    case ModuleTypeEnum.Security:
                        appSet = appSet.Include(a => a.Security).ThenInclude(s => s.FortifyScans);
                        break;
                    case ModuleTypeEnum.DatabaseEnvironment:
                        appSet = appSet.Include(a => a.DatabaseEnvironment).ThenInclude(d => d.Databases);
                        break;
                    case ModuleTypeEnum.ServerEnvironment:
                        appSet = appSet.Include(a => a.ServerEnvironment).ThenInclude(s => s.Servers);
                        break;
                    case ModuleTypeEnum.BASMOnboarding:
                        appSet = appSet.Include(a => a.BASMOnboarding);
                        break;
                    case ModuleTypeEnum.Report:
                        appSet = appSet.Include(a => a.Report);
                        break;
                    case ModuleTypeEnum.Dependencies:
                        appSet = appSet.Include(a => a.Architecture).ThenInclude(a => a.Dependees)
                                       .Include(a => a.Architecture).ThenInclude(a => a.Dependents);
                        break;
                }
            }

            return appSet;
        }

        #region Get all applications
        /// <summary>
        /// Retrieve all applications
        /// </summary>
        /// <param name="includeModules">Optional list of modules to include</param>
        /// <returns>All applications list</returns>
        public async Task<List<Application>> GetAllApplications(List<ModuleTypeEnum> includeModules = null)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await GenerateApplicationSet(dbContext, includeModules).AsNoTracking().ToListAsync();
        }
        #endregion

        #region Get all applications IDs
        /// <summary>
        /// Retrieve all applications ids
        /// </summary>
        /// <returns>All applications ids list</returns>
        public async Task<List<int>> GetAllApplicationsIDs()
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await dbContext.Applications.Select(app => app.ApplicationID).ToListAsync();
        }
        #endregion

        #region Get specific application by id
        /// <summary>
        /// Retrieve a specific application by id
        /// </summary>
        /// <param name="ApplicationID">ID of desired record</param>
        /// <param name="includeModules">Optional list of modules to include</param>
        /// <returns>Desired application object</returns>
        public async Task<Application> GetApplicationByID(int ApplicationID, List<ModuleTypeEnum> includeModules = null)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await GenerateApplicationSet(dbContext, includeModules).Where(app => app.ApplicationID == ApplicationID).AsNoTracking().FirstOrDefaultAsync();
        }
        #endregion

        #region Get specific application by shortID
        /// <summary>
        /// Retrieve a specific application by shortID
        /// </summary>
        /// <param name="shortID">ShortID of desired record</param>
        /// <param name="includeModules">Optional list of modules to include</param>
        /// <returns>Desired application object</returns>
        /// 
        public async Task<Application> GetApplicationByShortID(string ShortID, List<ModuleTypeEnum> includeModules = null)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return await GenerateApplicationSet(dbContext, includeModules).Where(app => app.ShortID == ShortID).AsNoTracking().FirstOrDefaultAsync();
        }
        #endregion

        #region Restore record

        /// <summary>
        /// Restore application for supplied entity
        /// </summary>
        /// <param name="app">Object of application to be restored</param>
        /// <param name="hardDelete">Will restore the application</param>
        /// <returns>bool</returns>
        public async Task<bool> RestoreAsync(Application app)
        {
            using var dbContext = DBContextFactory.CreateInstance();

            app.IsDeleted = false;
            dbContext.Update(app);
            await (_auditLogService?.LogAsync(app, AuditLog.ActionCategory.DataWrite, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);

            await dbContext.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Check for duplicate application name
        /// <summary>
        /// Check if there is an application with the given name
        /// </summary>
        /// <param name="name">Name of the application</param>
        /// <param name="ignoreId">ApplicationID to be ignored in this check</param>
        /// <returns>Returns true if app with given name exists</returns>
        public bool IsDuplicateName(string name, int ignoreId = -1)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return dbContext.Applications.Include(a => a.Identification).Where(app => !app.IsDeleted && app.ApplicationID != ignoreId && app.Identification.Name.ToLower().Trim() == name.ToLower().Trim()).FirstOrDefault() != null;
        }
        #endregion

        #region Return application short ID given name
        /// <summary>
        /// Return an application's short ID given its name
        /// </summary>
        /// <param name="name">Name of the application</param>
        /// <returns>Returns an application's short ID</returns>
        public string GetApplicationShortIDByName(string name)
        {
            using var dbContext = DBContextFactory.CreateInstance();
            return dbContext.Applications.Include(a => a.Identification).Where(app => !app.IsDeleted && app.Identification.Name.ToLower().Trim() == name.ToLower().Trim()).FirstOrDefault().ShortID;
        }
        #endregion

        #region Delete record
        /// <summary>
        /// Delete application for supplied entity
        /// </summary>
        /// <param name="app">Object of application to be deleted</param>
        /// <param name="hardDelete">Will hard delete if true</param>
        /// <returns>bool</returns>
        public async Task<bool> DeleteAsync(Application app, bool hardDelete)
        {
            using var dbContext = DBContextFactory.CreateInstance();

            if (hardDelete)
            {
                // Hard delete application
                app.CreateByUserID = default;
                app.CreateByUser = null;

                Architecture arch = app.Architecture ?? await dbContext.Architectures.Include(a => a.Dependees)
                    .Include(a => a.Dependents)
                    .Where(arch => arch.ApplicationID == app.ApplicationID)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                // Delete dependees
                foreach (Dependency dep in arch.Dependees)
                {
                    dbContext.Remove(dep);
                    await (_auditLogService?.LogAsync(dep, AuditLog.ActionCategory.DataDelete, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
                }

                // Delete dependents
                foreach (Dependency dep in arch.Dependents)
                {
                    dbContext.Remove(dep);
                    await (_auditLogService?.LogAsync(dep, AuditLog.ActionCategory.DataDelete, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
                }

                // Remove app from all favorite projects (regardless of hard/soft delete)
                List<User> users = await dbContext.Users.AsNoTracking().ToListAsync();
                foreach (User user in users)
                {
                    Preferences preferences = user.GetPreferences();
                    preferences.favouriteApplications.Remove(app.ApplicationID);
                    user.Preferences = JsonConvert.SerializeObject(preferences);
                    dbContext.Users.Update(user);
                    await (_auditLogService?.LogAsync(user, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
                }

                // Unsubscribe users from this app's updates
                List<ApplicationSubscription> subs = await dbContext.ApplicationSubscriptions
                                                                    .Include(sub => sub.Application)
                                                                    .Include(sub => sub.User)
                                                                    .Where(sub => sub.ApplicationID == app.ApplicationID)
                                                                    .ToListAsync();
                foreach (ApplicationSubscription sub in subs)
                {
                    sub.User = null;
                    sub.Application = null;
                    dbContext.Remove(sub);
                    await (_auditLogService?.LogAsync(sub, AuditLog.ActionCategory.DataDelete, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
                }

                // remove app from db
                dbContext.Remove(app);
                await (_auditLogService?.LogAsync(app, AuditLog.ActionCategory.DataDelete, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
            }
            else
            {
                // Update the app as deleted
                app.IsDeleted = true;
                dbContext.Update(app);
                await (_auditLogService?.LogAsync(app, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);
            }

            await dbContext.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}


using Eccc.Sali;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using TABS.Audit;
using TABS.Data;

namespace TABS
{
    public class UserPreferenceService : IUserPreferenceService
    {
        public string AdID { get; set; }

        public Preferences Preferences;

        private readonly AuditLogService _auditLogService;

        public UserPreferenceService(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public void Initialize(string newADId)
        {
            AdID = newADId;
            using var dbContext = DBContextFactory.CreateInstance();
            User user = dbContext.Users.Include(u => u.Role).Where(u => u.AdID == AdID).FirstOrDefault();

            if (user != null)
            {
                Preferences = user.GetPreferences();
            }
            else
            {
                Preferences = DefaultPreferences.GetDefaultPreferences();
            }
        }

        public void SetPreferences(Preferences preferences)
        {
            Preferences = preferences;
        }

        public Preferences GetPreferences()
        {
            return Preferences;
        }

        public async Task<bool> SaveUserPreferences()
        {
            // Get the user
            using var dbContext = DBContextFactory.CreateInstance();
            User user = await dbContext.Users.Include(u => u.Role).Where(user => user.AdID == AdID).FirstOrDefaultAsync();

            if (user == null)
            {
                return false;
            }

            // Update the user
            user.Preferences = JsonConvert.SerializeObject(Preferences);

            // Save changes
            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            // Audit logging
            await (_auditLogService?.LogAsync(user, AuditLog.ActionCategory.DataWriteUpdate, AuditLog.SeverityCategory.Routine, AuditLog.ResultCategory.Success) ?? Task.CompletedTask);

            return true;
        }
    }
}

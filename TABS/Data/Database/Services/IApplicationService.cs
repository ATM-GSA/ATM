using System.Collections.Generic;
using System.Threading.Tasks;

namespace TABS.Data
{
    public interface IApplicationService
    {
        Task<bool> DeleteAsync(Application app, bool hardDelete);
        Task<Application> GetApplicationByID(int ApplicationID, List<ModuleTypeEnum> includeModules = null);
        Task<Application> GetApplicationByShortID(string ShortID, List<ModuleTypeEnum> includeModules = null);
    }
}
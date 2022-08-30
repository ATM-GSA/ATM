using System.Collections.Generic;

namespace TABS.Data
{
    public interface IApplicationModule
    {
        public Dictionary<string, ModuleProperty> GetProperties();

        public ModuleTypeEnum GetModuleType();
    }
}

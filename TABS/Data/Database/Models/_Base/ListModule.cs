using System.Collections.Generic;

namespace TABS.Data
{
    /// <summary>
    /// Application list module base class
    /// </summary>
    public abstract class ListModule : IApplicationModule
    {
        public abstract ModuleTypeEnum GetModuleType();

        public abstract Module GetParentModule();

        public abstract void SetParentModule(Module parentModule);

        public abstract Dictionary<string, ModuleProperty> GetProperties();
    }
}

using System.Collections.Generic;

namespace TABS.Data
{
    public class ServerEnvironment : Module
    {
        public List<Server> Servers { get; set; } = new();

        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.ServerEnvironment;
        }

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"Servers", new() { LocalizationKey = "Application.ServerEnvironment.Servers", Name = "Servers", Type = Servers?.GetType(), Value = Servers } },
            };
        }

        public override List<ModuleProperty> GetChildModules()
        {
            return new List<ModuleProperty>()
            {
                GetProperties()["Servers"]
            };
        }
    }
}

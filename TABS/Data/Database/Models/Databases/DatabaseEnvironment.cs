using System.Collections.Generic;

namespace TABS.Data
{
    public class DatabaseEnvironment : Module
    {
        public List<Database> Databases { get; set; } = new();

        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.DatabaseEnvironment;
        }

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"Databases", new() { LocalizationKey = "Application.DatabaseEnvironment.Databases", Name = "Databases", Type = Databases?.GetType(), Value = Databases } },
            };
        }

        public override List<ModuleProperty> GetChildModules()
        {
            return new List<ModuleProperty>()
            {
                GetProperties()["Databases"]
            };
        }
    }
}

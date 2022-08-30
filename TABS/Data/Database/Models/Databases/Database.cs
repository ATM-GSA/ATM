using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    public class Database : ListModule
    {
        public enum DatabaseType
        {
            [Display(Name = "Development", ResourceType = typeof(TABS.Resources.App))]
            Development,
            [Display(Name = "Production", ResourceType = typeof(TABS.Resources.App))]
            Production,
            [Display(Name = "QA", ResourceType = typeof(TABS.Resources.App))]
            QA,
            [Display(Name = "QC", ResourceType = typeof(TABS.Resources.App))]
            QC,
            [Display(Name = "Test", ResourceType = typeof(TABS.Resources.App))]
            Test
        }

        [Key]
        public int DatabaseID { get; set; }

        [StringLength(1000)]
        public string Name { get; set; } = "";

        [StringLength(1500)]
        public string URL { get; set; } = "";

        [StringLength(200)]
        public string Version { get; set; } = "";

        [StringLength(200)]
        public string Platform { get; set; } = "";

        public DatabaseType Type { get; set; } = DatabaseType.Development;

        [JsonIgnore]
        public DatabaseEnvironment DatabaseEnvironment { get; set; } // Inverse navigation property to DatabaseEnvironment

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"Name", new() { LocalizationKey = "Application.DatabaseEnvironment.Database.Name", Name = "Name", Type = Name?.GetType(), Value = Name } },
                {"URL", new() { LocalizationKey = "Application.DatabaseEnvironment.Database.URL", Name = "URL", Type = URL?.GetType(), Value = URL } },
                {"Version", new() { LocalizationKey = "Application.DatabaseEnvironment.Database.Version", Name = "Version", Type = Version?.GetType(), Value = Version } },
                {"Platform", new() { LocalizationKey = "Application.DatabaseEnvironment.Database.Platform", Name = "Platform", Type = Platform?.GetType(), Value = Platform } },
                {"Type", new() { LocalizationKey = "Application.DatabaseEnvironment.Database.Type", Name = "Type", Type = Type.GetType(), Value = Type } },
            };
        }
        public override Module GetParentModule()
        {
            return DatabaseEnvironment;
        }

        public override void SetParentModule(Module parentModule)
        {
            DatabaseEnvironment = (DatabaseEnvironment)parentModule;
        }

        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.Database;
        }
    }
}

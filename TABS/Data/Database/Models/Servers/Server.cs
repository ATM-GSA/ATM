using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    public class Server : ListModule
    {
        public enum ServerType
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
        public int ServerID { get; set; }

        [StringLength(1000)]
        public string Name { get; set; } = "";

        [StringLength(1500)]
        public string URL { get; set; } = "";

        public ServerType Type { get; set; } = ServerType.Development;

        [StringLength(200)]
        public string Version { get; set; } = "";

        [JsonIgnore]
        public ServerEnvironment ServerEnvironment { get; set; } // Inverse navigation property to ServerEnvironment

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"Name", new() { LocalizationKey = "Application.ServerEnvironment.Server.Name", Name = "Name", Type = Name?.GetType(), Value = Name } },
                {"URL", new() { LocalizationKey = "Application.ServerEnvironment.Server.URL", Name = "URL", Type = URL?.GetType(), Value = URL } },
                {"Type", new() { LocalizationKey = "Application.ServerEnvironment.Server.Type", Name = "Type", Type = Type.GetType(), Value = Type } },
                {"Version", new() { LocalizationKey = "Application.ServerEnvironment.Server.Version", Name = "Version", Type = Version?.GetType(), Value = Version } },
            };
        }

        public override Module GetParentModule()
        {
            return ServerEnvironment;
        }

        public override void SetParentModule(Module parentModule)
        {
            ServerEnvironment = (ServerEnvironment)parentModule;
        }
        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.Server;
        }
    }
}

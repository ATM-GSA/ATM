using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    public class Architecture : Module
    {
        [StringLength(200)]
        public string AppPlatform { get; set; } = "N/A";

        [StringLength(200)]
        public string SMARTFramework { get; set; } = "N/A";

        [StringLength(1000)]
        public string SMARTUpgradePlanning { get; set; } = "";

        public bool XmlBlob { get; set; }

        [StringLength(200)]
        public string CDTSVersion { get; set; } = "";

        [StringLength(200)]
        public string NETVersion { get; set; } = "";

        [StringLength(200)]
        public string SEASVersion { get; set; } = "";

        public virtual List<Dependency> Dependees { get; set; } = new();

        public virtual List<Dependency> Dependents { get; set; } = new();

        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.Architecture;
        }

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"AppPlatform", new() { LocalizationKey = "Application.Architecture.AppPlatform", Name = "AppPlatform", Type = AppPlatform?.GetType(), Value = AppPlatform } },
                {"SMARTFramework", new() { LocalizationKey = "Application.Architecture.SMARTFramework", Name = "SMARTFramework", Type = SMARTFramework?.GetType(), Value = SMARTFramework } },
                {"SMARTUpgradePlanning", new() { LocalizationKey = "Application.Architecture.SMARTUpgradePlanning", Name = "SMARTUpgradePlanning", Type = SMARTUpgradePlanning?.GetType(), Value = SMARTUpgradePlanning } },
                {"XmlBlob", new() { LocalizationKey = "Application.Architecture.XmlBlob", Name = "XmlBlob", Type = XmlBlob.GetType(), Value = XmlBlob } },
                {"CDTSVersion", new() { LocalizationKey = "Application.Architecture.CDTSVersion", Name = "CDTSVersion", Type = CDTSVersion?.GetType(), Value = CDTSVersion } },
                {"NETVersion", new() { LocalizationKey = "Application.Architecture.NETVersion", Name = "NETVersion", Type = NETVersion?.GetType(), Value = NETVersion } },
                {"SEASVersion", new() { LocalizationKey = "Application.Architecture.SEASVersion", Name = "SEASVersion", Type = SEASVersion?.GetType(), Value = SEASVersion } },
                {"Dependees", new() { LocalizationKey = "Application.Architecture.Dependees", Name = "Dependees", Type = Dependees?.GetType(), Value = Dependees } },
                {"Dependents", new() { LocalizationKey = "Application.Architecture.Dependents", Name = "Dependents", Type = Dependents?.GetType(), Value = Dependents } },
            };
        }
    }
}
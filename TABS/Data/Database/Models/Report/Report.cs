using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    public class Report : Module
    {
        [StringLength(1500)]
        public string Performance { get; set; } = "";

        [StringLength(1500)]
        public string Accessibility { get; set; } = "";

        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.Report;
        }

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"Performance", new() { LocalizationKey = "Application.Report.Performance", Name = "Performance", Type = Performance?.GetType(), Value = Performance } },
                {"Accessibility", new() { LocalizationKey = "Application.Report.Accessibility", Name = "Accessibility", Type = Accessibility?.GetType(), Value = Accessibility } },
            };
        }
    }
}

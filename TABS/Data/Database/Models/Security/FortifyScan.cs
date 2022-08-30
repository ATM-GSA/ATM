using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    public class FortifyScan : ListModule
    {
        public enum ScanTypeLookUp
        {
            [Display(Name = "Application_Security_FortifyScan_ScanType_StaticScan", ResourceType = typeof(TABS.Resources.App))]
            StaticScan,
            [Display(Name = "Application_Security_FortifyScan_ScanType_DynamicScan", ResourceType = typeof(TABS.Resources.App))]
            DynamicScan
        }

        [Key]
        public int FortifyScanId { get; set; } // Also acts as foreign key.

        [StringLength(200)]
        public string Name { get; set; } = "";

        public DateTime ScanDate { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; } = "";

        [StringLength(1500)]
        public string ReportLink { get; set; } = "";

        public ScanTypeLookUp ScanType { get; set; } = ScanTypeLookUp.StaticScan;

        public Security Security { get; set; } // Inverse navigation property to Security.FortifyScans.

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"Name", new() { LocalizationKey = "Application.Security.FortifyScan.Name", Name = "Name", Type = Name?.GetType(), Value = Name } },
                {"ScanDate", new() { LocalizationKey = "Application.Security.FortifyScan.ScanDate", Name = "ScanDate", Type = ScanDate.GetType(), Value = ScanDate } },
                {"Notes", new() { LocalizationKey = "Application.Security.FortifyScan.Notes", Name = "Notes", Type = Notes?.GetType(), Value = Notes } },
                {"ReportLink", new() { LocalizationKey = "Application.Security.FortifyScan.ReportLink", Name = "ReportLink", Type = ReportLink?.GetType(), Value = ReportLink } },
                {"ScanType", new() { LocalizationKey = "Application.Security.FortifyScan.ScanType", Name = "ScanType", Type = ScanType.GetType(), Value = ScanType } },
            };
        }

        public override Module GetParentModule()
        {
            return Security;
        }

        public override void SetParentModule(Module parentModule)
        {
            Security = (Security)parentModule;
        }
        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.FortifyScan;
        }
    }
}

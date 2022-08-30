using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    public class ApplicationIdentification : Module
    {
        [Required]
        public int APMID { get; set; }

        [StringLength(200)]
        public string Name { get; set; } = "";

        [StringLength(200)]
        public string Title { get; set; } = "";

        [StringLength(200)]
        public string Status { get; set; } = "N/A";

        [StringLength(200)]
        public string Visibility { get; set; } = "N/A";

        [StringLength(200)]
        public string WebAccessibilityAssessment { get; set; } = "NotComplete";

        [Range(0, 100)]
        public int AccessibilityComplianceScore { get; set; } = 0;

        public bool SWIM { get; set; }

        public bool CMPPortfolio { get; set; }

        [StringLength(1500)]
        public string WebURL { get; set; } = "";

        [StringLength(200)]
        public string ClientBranch { get; set; } = "";

        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.ApplicationIdentification;
        }

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"APMID", new() { LocalizationKey = "Application.Identification.APMID", Name = "APMID", Type = APMID.GetType(), Value = APMID } },
                {"Name", new() { LocalizationKey = "Application.Identification.Name", Name = "Name", Type = Name?.GetType(), Value = Name } },
                {"Title", new() { LocalizationKey = "Application.Identification.Title", Name = "Title", Type = Title?.GetType(), Value = Title } },
                {"Status", new() { LocalizationKey = "Application.Identification.Status", Name = "Status", Type = Status?.GetType(), Value = Status } },
                {"Visibility", new() { LocalizationKey = "Application.Identification.Visibility", Name = "Visibility", Type = Visibility?.GetType(), Value = Visibility } },
                {"WebAccessibilityAssessment", new() { LocalizationKey = "Application.Identification.WebAccessibilityAssessment", Name = "WebAccessibilityAssessment", Type = WebAccessibilityAssessment?.GetType(), Value = WebAccessibilityAssessment } },
                {"AccessibilityComplianceScore", new() { LocalizationKey = "Application.Identification.AccessibilityComplianceScore", Name = "AccessibilityComplianceScore", Type = AccessibilityComplianceScore.GetType(), Value = AccessibilityComplianceScore} },
                {"SWIM", new() { LocalizationKey = "Application.Identification.SWIM", Name = "SWIM", Type = SWIM.GetType(), Value = SWIM } },
                {"CMPPortfolio", new() { LocalizationKey = "Application.Identification.CMPPortfolio", Name = "CMPPortfolio", Type = CMPPortfolio.GetType(), Value = CMPPortfolio } },
                {"WebURL", new() { LocalizationKey = "Application.Identification.WebURL", Name = "WebURL", Type = WebURL?.GetType(), Value = WebURL } },
                {"ClientBranch", new() { LocalizationKey = "Application.Identification.ClientBranch", Name = "ClientBranch", Type = ClientBranch?.GetType(), Value = ClientBranch } },
            };
        }
    }
}
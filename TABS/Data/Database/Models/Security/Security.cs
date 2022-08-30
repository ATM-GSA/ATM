using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    public class Security : Module
    {
        public bool SQLSSLEncryption { get; set; }

        public bool AttachmentDropZone { get; set; }

        public bool AuditLogs { get; set; }

        public bool WebconfigEncrpted { get; set; }

        public DateTime? WAFTest { get; set; }

        public DateTime? WAFStatus { get; set; }

        public bool Exempt { get; set; }

        [StringLength(1000)]
        public string ExemptionReasoning { get; set; } = "";

        [StringLength(200)]
        public string SecurityLevel { get; set; } = "";

        public DateTime? SatoExpiry { get; set; }

        public bool SAD { get; set; }

        public bool ICAP { get; set; }

        public bool HSTS { get; set; }

        public bool StaticRemediationCompleted { get; set; }

        public bool DynamicRemediationCompleted { get; set; }

        public List<FortifyScan> FortifyScans { get; set; } = new();

        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.Security;
        }

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"SQLSSLEncryption", new() { LocalizationKey = "Application.Security.SQLSSLEncryption", Name = "SQLSSLEncryption", Type = SQLSSLEncryption.GetType(), Value = SQLSSLEncryption } },
                {"AttachmentDropZone", new() { LocalizationKey = "Application.Security.AttachmentDropZone", Name = "AttachmentDropZone", Type = AttachmentDropZone.GetType(), Value = AttachmentDropZone } },
                {"AuditLogs", new() { LocalizationKey = "Application.Security.AuditLogs", Name = "AuditLogs", Type = AuditLogs.GetType(), Value = AuditLogs } },
                {"WebconfigEncrpted", new() { LocalizationKey = "Application.Security.WebconfigEncrpted", Name = "WebconfigEncrpted", Type = WebconfigEncrpted.GetType(), Value = WebconfigEncrpted } },
                {"WAFTest", new() { LocalizationKey = "Application.Security.WAFTest", Name = "WAFTest", Type = WAFTest?.GetType(), Value = WAFTest } },
                {"WAFStatus", new() { LocalizationKey = "Application.Security.WAFStatus", Name = "WAFStatus", Type = WAFStatus?.GetType(), Value = WAFStatus } },
                {"Exempt", new() { LocalizationKey = "Application.Security.Exempt", Name = "Exempt", Type = Exempt.GetType(), Value = Exempt } },
                {"ExemptionReasoning", new() { LocalizationKey = "Application.Security.ExemptionReasoning", Name = "ExemptionReasoning", Type = ExemptionReasoning?.GetType(), Value = ExemptionReasoning } },
                {"SecurityLevel", new() { LocalizationKey = "Application.Security.SecurityLevel", Name = "SecurityLevel", Type = SecurityLevel?.GetType(), Value = SecurityLevel } },
                {"SatoExpiry", new() { LocalizationKey = "Application.Security.SatoExpiry", Name = "SatoExpiry", Type = SatoExpiry?.GetType(), Value = SatoExpiry } },
                {"SAD", new() { LocalizationKey = "Application.Security.SAD", Name = "SAD", Type = SAD.GetType(), Value = SAD } },
                {"ICAP", new() { LocalizationKey = "Application.Security.ICAP", Name = "ICAP", Type = ICAP.GetType(), Value = ICAP } },
                {"HSTS", new() { LocalizationKey = "Application.Security.HSTS", Name = "HSTS", Type = HSTS.GetType(), Value = HSTS } },
                {"StaticRemediationCompleted", new() { LocalizationKey = "Application.Security.StaticRemediationCompleted", Name = "StaticRemediationCompleted", Type = StaticRemediationCompleted.GetType(), Value = StaticRemediationCompleted } },
                {"DynamicRemediationCompleted", new() { LocalizationKey = "Application.Security.DynamicRemediationCompleted", Name = "DynamicRemediationCompleted", Type = DynamicRemediationCompleted.GetType(), Value = DynamicRemediationCompleted } },
                {"FortifyScans", new() { LocalizationKey = "Application.Security.FortifyScans", Name = "FortifyScans", Type = FortifyScans?.GetType(), Value = FortifyScans } },
            };
        }

        public override List<ModuleProperty> GetChildModules()
        {
            return new List<ModuleProperty>()
            {
                GetProperties()["FortifyScans"]
            };
        }
    }
}

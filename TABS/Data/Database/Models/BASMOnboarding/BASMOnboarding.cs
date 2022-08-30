using System.Collections.Generic;

namespace TABS.Data
{
    public class BASMOnboarding : Module
    {
        public bool AssystSetup { get; set; }

        public bool IntroEmail { get; set; }

        public bool PowerUserGuide { get; set; }

        public bool BASMGuide { get; set; }

        public bool DemoToBASM { get; set; }

        public bool ClientIntro { get; set; }

        public bool AppMonitoring { get; set; }

        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.BASMOnboarding;
        }

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"AssystSetup", new() { LocalizationKey = "Application.BASMOnboarding.AssystSetup", Name = "AssystSetup", Type = AssystSetup.GetType(), Value = AssystSetup } },
                {"IntroEmail", new() { LocalizationKey = "Application.BASMOnboarding.IntroEmail", Name = "IntroEmail", Type = IntroEmail.GetType(), Value = IntroEmail } },
                {"PowerUserGuide", new() { LocalizationKey = "Application.BASMOnboarding.PowerUserGuide", Name = "PowerUserGuide", Type = PowerUserGuide.GetType(), Value = PowerUserGuide } },
                {"BASMGuide", new() { LocalizationKey = "Application.BASMOnboarding.BASMGuide", Name = "BASMGuide", Type = BASMGuide.GetType(), Value = BASMGuide } },
                {"DemoToBASM", new() { LocalizationKey = "Application.BASMOnboarding.DemoToBASM", Name = "DemoToBASM", Type = DemoToBASM.GetType(), Value = DemoToBASM } },
                {"ClientIntro", new() { LocalizationKey = "Application.BASMOnboarding.ClientIntro", Name = "ClientIntro", Type = ClientIntro.GetType(), Value = ClientIntro } },
                {"AppMonitoring", new() { LocalizationKey = "Application.BASMOnboarding.AppMonitoring", Name = "AppMonitoring", Type = AppMonitoring.GetType(), Value = AppMonitoring } },

            };
        }
    }
}
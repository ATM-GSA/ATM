using Newtonsoft.Json;

namespace TABS.Data
{
    /// <summary>
    /// Default contact info
    /// </summary>
    public class DefaultApplicationProperties
    {
        public static ApplicationProperties GetDefaultApplicationProperties()
        {
            return new()
            {
                availableModules = new()
                {
                    (int)ModuleTypeEnum.ApplicationIdentification,
                    (int)ModuleTypeEnum.Architecture,
                    (int)ModuleTypeEnum.Contacts,
                    (int)ModuleTypeEnum.Security
                },
                featuredModules = new(),
            };
        }

        public static string GetSerializedString()
        {
            return JsonConvert.SerializeObject(GetDefaultApplicationProperties());
        }
    }
}


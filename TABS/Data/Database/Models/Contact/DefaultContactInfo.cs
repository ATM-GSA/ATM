using Newtonsoft.Json;

namespace TABS.Data
{
    /// <summary>
    /// Default contact info
    /// </summary>
    public class DefaultContactInfo
    {
        public static ContactInfo GetDefaultContactInfo()
        {
            return new()
            {
                name = "",
                email = "",
                itLevel = 0,
                userAdId = "",
                userID = -1
            };
        }

        public static string GetSerializedString()
        {
            return JsonConvert.SerializeObject(GetDefaultContactInfo());
        }
    }
}


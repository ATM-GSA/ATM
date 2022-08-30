using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    public class Contact : Module
    {
        [StringLength(1000)]
        public string Manager { get; set; } = DefaultContactInfo.GetSerializedString();

        [StringLength(1000)]
        public string TeamLead { get; set; } = DefaultContactInfo.GetSerializedString();

        [StringLength(1000)]
        public string TechLead { get; set; } = DefaultContactInfo.GetSerializedString();

        [StringLength(1000)]
        public string ClientManager { get; set; } = DefaultContactInfo.GetSerializedString();

        [StringLength(1000)]
        public string ClientLead { get; set; } = DefaultContactInfo.GetSerializedString();


        public ContactInfo GetManager()
        {
            return JsonConvert.DeserializeObject<ContactInfo>(Manager);
        }
        public ContactInfo GetTeamLead()
        {
            return JsonConvert.DeserializeObject<ContactInfo>(TeamLead);
        }
        public ContactInfo GetTechLead()
        {
            return JsonConvert.DeserializeObject<ContactInfo>(TechLead);
        }
        public ContactInfo GetClientManager()
        {
            return JsonConvert.DeserializeObject<ContactInfo>(ClientManager);
        }
        public ContactInfo GetClientLead()
        {
            return JsonConvert.DeserializeObject<ContactInfo>(ClientLead);
        }

        public override Dictionary<string, ModuleProperty> GetProperties()
        {
            return new Dictionary<string, ModuleProperty>
            {
                {"Manager", new() { LocalizationKey = "Application.Contacts.Manager", Name = "Manager", Type = typeof(ContactInfo), Value = GetManager() } },
                {"TeamLead", new() { LocalizationKey = "Application.Contacts.TeamLead", Name = "TeamLead", Type = typeof(ContactInfo), Value = GetTeamLead()  } },
                {"TechLead", new() { LocalizationKey = "Application.Contacts.TechLead", Name = "TechLead", Type = typeof(ContactInfo), Value = GetTechLead() } },
                {"ClientManager", new() { LocalizationKey = "Application.Contacts.ClientManager", Name = "ClientManager", Type = typeof(ContactInfo), Value = GetClientManager() } },
                {"ClientLead", new() { LocalizationKey = "Application.Contacts.ClientLead", Name = "ClientLead", Type = typeof(ContactInfo), Value = GetClientLead() } }
            };
        }

        public override ModuleTypeEnum GetModuleType()
        {
            return ModuleTypeEnum.Contacts;
        }
    }
}

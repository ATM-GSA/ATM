using Newtonsoft.Json;
using shortid;
using shortid.Configuration;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace TABS.Data
{
    public class Application : IEquatable<Application>
    {
        [Key]
        public int ApplicationID { get; set; }

        [Required]
        public string ShortID { get; set; } = ShortId.Generate(
            new GenerationOptions
            {
                UseNumbers = true,
                UseSpecialCharacters = false,
                Length = 12
            }
        );

        [Required]
        public string Properties { get; set; } = DefaultApplicationProperties.GetSerializedString();

        public bool IsArchived { get; set; } = false;

        public bool IsComplete { get; set; } = false; // Has this application completed the add process

        public bool IsDeleted { get; set; } = false; // Is this application marked for deletion

        public int? CreateByUserID { get; set; } // UserID of the user that created this app
        public User CreateByUser { get; set; } // User that created this app

        public ApplicationIdentification Identification { get; set; } = new();

        public ServerEnvironment ServerEnvironment { get; set; }

        public DatabaseEnvironment DatabaseEnvironment { get; set; }

        public Architecture Architecture { get; set; } = new();

        public Security Security { get; set; } = new();

        public Report Report { get; set; }

        public Contact Contact { get; set; } = new();

        public BASMOnboarding BASMOnboarding { get; set; }

        /// <summary>
        /// Deserializes the ApplicationProperties JSON of this Application
        /// </summary>
        /// <returns>ApplicationProperties</returns>
        public ApplicationProperties GetApplicationProperties()
        {
            return JsonConvert.DeserializeObject<ApplicationProperties>(Properties);
        }

        /// <summary>
        /// Get a list of all modules for this application
        /// </summary>
        /// <returns>List of all modules</returns>
        public Dictionary<ModuleTypeEnum, Module> GetModules()
        {
            Dictionary<ModuleTypeEnum, Module> moduleObject = new();

            foreach (ModuleTypeEnum mod in ModuleTypeEnumExtensions.GetDisplayedModules())
            {
                switch (mod)
                {
                    case ModuleTypeEnum.ApplicationIdentification:
                        moduleObject.Add(ModuleTypeEnum.ApplicationIdentification, Identification);
                        break;
                    case ModuleTypeEnum.Architecture:
                        moduleObject.Add(ModuleTypeEnum.Architecture, Architecture);
                        break;
                    case ModuleTypeEnum.Contacts:
                        moduleObject.Add(ModuleTypeEnum.Contacts, Contact);
                        break;
                    case ModuleTypeEnum.Security:
                        moduleObject.Add(ModuleTypeEnum.Security, Security);
                        break;
                    case ModuleTypeEnum.BASMOnboarding:
                        moduleObject.Add(ModuleTypeEnum.BASMOnboarding, BASMOnboarding);
                        break;
                    case ModuleTypeEnum.ServerEnvironment:
                        moduleObject.Add(ModuleTypeEnum.ServerEnvironment, ServerEnvironment);
                        break;
                    case ModuleTypeEnum.DatabaseEnvironment:
                        moduleObject.Add(ModuleTypeEnum.DatabaseEnvironment, DatabaseEnvironment);
                        break;
                    case ModuleTypeEnum.Report:
                        moduleObject.Add(ModuleTypeEnum.Report, Report);
                        break;
                }
            }
            return moduleObject;
        }
        public bool Equals(Application other)
        {
            if (other is null)
                return false;

            return this.ApplicationID == other.ApplicationID;
        }

        public override bool Equals(object obj) => Equals(obj as Application);
        public override int GetHashCode() => (ApplicationID).GetHashCode();
    }
}
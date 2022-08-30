
using System.Collections.Generic;

namespace TABS.Data
{
    public enum ModuleTypeEnum
    {
        ApplicationIdentification = 1,
        Architecture = 2,
        Contacts = 3,
        Security = 4,
        DatabaseEnvironment = 5,
        ServerEnvironment = 6,
        BASMOnboarding = 7,
        Report = 8,
        Dependencies = 9,

        // list modules have a value that is the negation of its parent value
        FortifyScan = -4,
        Server = -6,
        Database = -5,

        // undefined is 0
        Undefined = 0,
    }

    public static class ModuleTypeEnumExtensions
    {
        /// <summary>
        /// List of modules that are displayed to the user
        /// In other words, these are the "top-level" modules that the user can directly interact with
        /// </summary>
        /// <returns>List of displayed modules that the user can interact with</returns>
        public static List<ModuleTypeEnum> GetDisplayedModules()
        {
            return new()
            {
                ModuleTypeEnum.ApplicationIdentification,
                ModuleTypeEnum.Architecture,
                ModuleTypeEnum.Contacts,
                ModuleTypeEnum.Security,
                ModuleTypeEnum.DatabaseEnvironment,
                ModuleTypeEnum.ServerEnvironment,
                ModuleTypeEnum.BASMOnboarding,
                ModuleTypeEnum.Report
            };
        }

        /// <summary>
        /// Generates a module with the given module type.
        /// </summary>
        /// <param name="moduleType">Module type</param>
        /// <returns>A module object</returns>
        public static IApplicationModule ToModule(ModuleTypeEnum moduleType)
        {
            switch (moduleType)
            {
                case ModuleTypeEnum.ApplicationIdentification:
                    return new ApplicationIdentification();
                case ModuleTypeEnum.Architecture:
                    return new Architecture();
                case ModuleTypeEnum.Contacts:
                    return new Contact();
                case ModuleTypeEnum.Security:
                    return new Security();
                case ModuleTypeEnum.FortifyScan:
                    return new FortifyScan();
                case ModuleTypeEnum.DatabaseEnvironment:
                    return new DatabaseEnvironment();
                case ModuleTypeEnum.Database:
                    return new Database();
                case ModuleTypeEnum.ServerEnvironment:
                    return new ServerEnvironment();
                case ModuleTypeEnum.Server:
                    return new Server();
                case ModuleTypeEnum.BASMOnboarding:
                    return new BASMOnboarding();
                case ModuleTypeEnum.Report:
                    return new Report();
            }

            return null;
        }

        /// <summary>
        /// Returns true if module is required
        /// </summary>
        /// <param name="moduleType">The module type to check</param>
        /// <returns>True if module type is required</returns>
        public static bool IsModuleRequired(ModuleTypeEnum moduleType)
        {
            switch (moduleType)
            {
                case ModuleTypeEnum.ApplicationIdentification:
                    return true;
                case ModuleTypeEnum.Architecture:
                    return true;
                case ModuleTypeEnum.Contacts:
                    return true;
                case ModuleTypeEnum.Security:
                    return true;
                default: return false;
            }
        }
    }
}

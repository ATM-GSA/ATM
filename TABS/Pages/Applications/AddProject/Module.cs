using System.Collections.Generic;
using TABS.Data;

namespace TABS.Pages.Applications.AddProject
{
    public class Module
    {
        public ModuleTypeEnum Type { get; set; }

        public bool Required { get; set; }

        public string DisplayName { get; set; }

        public bool Added { get; set; }
    }

    public static class ModuleList
    {
        public static List<Module> AllModules = new List<Module>();
        public static List<Module> AddedModules = new List<Module>();
        public static int numAddedModules;

        public static void addToListOfAllModules(Module newModule)
        {
            AllModules.Add(newModule);
        }

        public static void addToListOfAddedModules(Module addedModule)
        {
            AddedModules.Add(addedModule);
            numAddedModules++;
        }

        public static void clearAddedModules()
        {
            AddedModules.Clear();
            numAddedModules = 0;
        }
    }
}

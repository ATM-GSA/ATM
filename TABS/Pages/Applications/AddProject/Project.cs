using System.Collections.Generic;

namespace TABS.Pages.Applications.AddProject
{
    public class Project
    {
        public string name { get; set; }
        public List<Module> modules { get; set; }
        public int numOfAddedModules { get; set; }

    }
}

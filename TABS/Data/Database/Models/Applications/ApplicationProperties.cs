using System.Collections.Generic;

namespace TABS.Data
{
    /// <summary>
    /// Represents the special properties of an Application
    /// </summary>
    public class ApplicationProperties
    {
        public List<int> availableModules { get; set; }
        public List<int> featuredModules { get; set; }
    }
}

using Newtonsoft.Json;

namespace TABS.Data
{
    public class Dependency
    {
        // Dependee -[Depends On]-> Dependent
        
        public int DependeeID { get; set; }
        public int DependentID { get; set; }

        [JsonIgnore]
        public virtual Architecture Dependee { get; set; } // Dependee, depends on dependent 

        [JsonIgnore]
        public virtual Architecture Dependent { get; set; } // Dependent, depended on by the dependee
    }
}

using System;

namespace TABS.Data
{
    public class ModuleProperty
    {
        public string LocalizationKey { get; set; } = "NAME NOT INITIALIZED";
        public string DescriptionKey { get; set; } = "DESCRIPTION NOT INITIALIZED";
        public string Name { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }
    }
}

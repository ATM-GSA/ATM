using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    /// <summary>
    /// Application module base class
    /// </summary>
    public abstract class Module : IApplicationModule
    {
        [Key]
        public int ApplicationID { get; set; } // Also acts as foreign key.
        public Application Application { get; set; } // Inverse navigation property to Application

        public int StatusFlags { get; set; } = 0;

        public string LastModifiedBy { get; set; } = null; // Nullable SID of the user who last edited this module

        public DateTime LastUpdate { get; set; } = DateTime.Now;
        public DateTime NextUpdate { get; set; } = DateTime.Now.AddMonths(1);

        public abstract Dictionary<string, ModuleProperty> GetProperties();

        public abstract ModuleTypeEnum GetModuleType();

        public virtual List<ModuleProperty> GetChildModules()
        {
            return new List<ModuleProperty>();
        }
    }
}

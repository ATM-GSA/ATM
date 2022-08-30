using System.ComponentModel.DataAnnotations;

namespace TABS.Data
{
    public class ModuleType
    {
        [Key]
        public int ModuleTypeID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public bool IsRequired { get; set; }

    }
}


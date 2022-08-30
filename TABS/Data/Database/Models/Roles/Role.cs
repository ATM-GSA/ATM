using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;


namespace TABS.Data
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }

        public int UserID { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        public int PermissionLevel { get; set; } = (int)RoleEnums.Roles.NeedsApproval;

        public int PermissionLevelBeforeDelete { get; set; } = -1;

        public DateTime Expires { get; set; } = DateTime.Now.AddYears(1);
    }
}

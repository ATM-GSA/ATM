
namespace TABS.Data
{
    public class ModuleUpdate : Notification
    {
        public int userID { get; set; }
        public int applicationID { get; set; }
        public int moduleType { get; set; }
    }
}

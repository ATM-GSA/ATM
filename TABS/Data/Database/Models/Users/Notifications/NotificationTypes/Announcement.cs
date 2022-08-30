
namespace TABS.Data
{
    public class Announcement : Notification
    {
        public int userID { get; set; }
        public string title { get; set; }
        public string message { get; set; }
    }
}

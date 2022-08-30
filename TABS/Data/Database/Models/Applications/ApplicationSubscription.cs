namespace TABS.Data
{
    public class ApplicationSubscription
    {
        public int ApplicationID { get; set; }
        public Application Application { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }
    }
}

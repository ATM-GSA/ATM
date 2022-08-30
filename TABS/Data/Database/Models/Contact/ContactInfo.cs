namespace TABS.Data
{
    public class ContactInfo
    {
        public int userID { get; set; }
        public string userAdId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public int itLevel { get; set; }

        public override string ToString()
        {
            string str = "";

            if (name != "")
            {
                str += $"{name}";
            }

            if (itLevel != 0)
            {
                str += $" (IT{itLevel})";
            }

            if (email != "")
            {
                str += $" - {email}";
            }

            return str;
        }
    }
}

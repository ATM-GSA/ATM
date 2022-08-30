namespace TABS.Data.Emails
{
    public interface ITemplate
    {
        public string GetSubject();
        public string GetTemplate();
    }
}

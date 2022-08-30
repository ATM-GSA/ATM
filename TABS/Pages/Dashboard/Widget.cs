namespace TABS.Pages.Dashboard
{
    public class Widget
    {
        public int id { get; set; }
        public int order { get; set; }
        public string widgetName { get; set; }
        public string description { get; set; }
        public bool hidden { get; set; }
        public bool fullWidth { get; set; }
        public bool hasWidthOption { get; set; }
        public bool hasHideOption { get; set; }
    }
}

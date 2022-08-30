using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace TABS
{
    /// <summary>
    /// Global variables for the TABS app
    /// </summary>
    public class Global
    {
        private IWebHostEnvironment Env { get; set; }

        public Global(IWebHostEnvironment webHostEnvironment)
        {
            Env = webHostEnvironment;
        }
        public string GetBaseURL()
        {
            return Env.IsDevelopment() ? "" : "/atm";
        }
        public string GetNewApplicationURL()
        {
            return Env.IsDevelopment() ? "/applications/add" : "/atm/applications/add";
        }

        public string GetWidgetSettingsURL()
        {
            return Env.IsDevelopment() ? "/settings/widgets" : "/atm/settings/widgets";
        }

        public string GetAppDetailsURL(string shortId)
        {
            return Env.IsDevelopment() ? "/applications/" + shortId : "/atm/applications/" + shortId;
        }

        public string GetModuleDetailsURL(string shortId, string module)
        {
            return Env.IsDevelopment() ? "/applications/" + shortId + "/" + module : "/atm/applications/" + shortId + "/" + module;
        }

        public string GetProfilePageURL()
        {
            return Env.IsDevelopment() ? "/settings" : "/atm/settings";
        }

        public string GetApplicationsTableURL()
        {
            return Env.IsDevelopment() ? "/applications" : "/atm/applications";
        }
    }
}

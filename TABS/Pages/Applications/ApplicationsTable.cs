using AntDesign;
using AntDesign.TableModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TABS.Data;
using Module = TABS.Data.Module;

namespace TABS.Pages.Applications
{
    public partial class ApplicationsTable
    {
        [Inject]
        public CrudService CrudService { get; set; }
        [Inject]
        public ApplicationService ApplicationService { get; set; }
        [Inject]
        public IStringLocalizer<App> Localizer { get; set; }

        [Parameter]
        public bool IsRecovery { get; set; }


        List<Application> allApplications;
        List<ApplicationInformation> originalApplicationsTable;
        List<ApplicationInformation> applicationsTable;

        int _total = 0;
        int _pageSize = 15;
        int _pageIndex = 1;

        bool loading = true;

        bool customizeColsVisible = false;
        bool showCloseBtn = false;

        IEnumerable<string> _selectedModuleFields;
        List<ModuleField> _moduleFields = new();
        List<string> _ignoredColumns = new() { "APMID", "Name", "Status", "WebURL", "Dependees", "Dependents", "Manager", "TeamLead", "FortifyScans" };
        List<Module> _allModules;

        // maybe populate keys through a db call
        Dictionary<string, Type> mapper = new();

        /* Custom filter values for project status
        */
        public TableFilter<string>[] statusFilters;

        public class ApplicationInformation
        {
            public Application Application { get; set; }
        }

        public class ModuleField
        {
            public string Value { get; set; }
            public string Label { get; set; }
            public string Module { get; set; }
        }

        /* Fires on page load
         */
        protected override async Task OnInitializedAsync()
        {
            _allModules = new()
            {
                new ApplicationIdentification(),
                new Architecture(),
                new BASMOnboarding(),
                new Contact(),
                new Report(),
                new Security(),
            };

            // Add columns to customizable column dropdown.
            // Ignore all specified columns
            foreach (Module module in _allModules)
            {
                Dictionary<string, ModuleProperty> allProperties = module.GetProperties();
                foreach (KeyValuePair<string, ModuleProperty> property in allProperties)
                {
                    if (!_ignoredColumns.Contains(property.Value.Name))
                    {
                        _moduleFields.Add(new ModuleField { Value = property.Value.Name, Label = Localizer[property.Value.LocalizationKey], Module = Localizer[module.GetModuleType().ToString()] });
                        mapper.Add(property.Value.Name, module.GetType());
                    }
                }
            }

            statusFilters = new TableFilter<string>[]
            {
            new TableFilter<string> { Text = Localizer["Active"], Value = "Active"},
            new TableFilter<string> { Text = Localizer["NewDevelopment"], Value = "New Development"},
            new TableFilter<string> { Text = Localizer["OnHold"], Value = "On Hold"},
            new TableFilter<string> { Text = Localizer["ToBeDecommissioned"], Value = "To Be Decommissioned"}
            };

            allApplications = new List<Application>();
            applicationsTable = new List<ApplicationInformation>();
            originalApplicationsTable = new List<ApplicationInformation>();
            await FetchApps();
        }

        /* Fetch application data and table column information
         */
        private async Task FetchApps()
        {
            loading = true;

            try
            {
                allApplications = await CrudService.GetDataAsync<Application>();

                allApplications = this.IsRecovery ?
                    allApplications.Where(app => app.IsDeleted).ToList()
                  : allApplications.Where(app => app.IsComplete && !app.IsDeleted).ToList();

                await UpdateTable(allApplications);
            }
            catch (Exception ex)
            {
                Error.ProcessError(ex);
            }

            loading = false;
        }

        private async Task UpdateTable(List<Application> apps)
        {
            List<Task<Application>> populateTasks;

            populateTasks = allApplications.Select(app => ApplicationService.GetApplicationByID(app.ApplicationID)).ToList();
            Application[] allPopulatedApps = await Task.WhenAll(populateTasks);

            originalApplicationsTable = allPopulatedApps.Select(populatedApp =>
                new ApplicationInformation()
                {
                    Application = populatedApp
                }).ToList();
            applicationsTable = originalApplicationsTable.ToList();
            _total = applicationsTable.Count;
        }

        private async Task RestoreApplication(Application app)
        {

            await ApplicationService.RestoreAsync(app);
        }

        private async Task DeleteApplication(Application app)
        {
            await ApplicationService.DeleteAsync(app, true);
        }

        /* Return true if an app contains a specified search value in any of 
         * its columns
         */
        public Task<bool> DoesApplicationContainValue(ApplicationInformation app, string searchTerm)
        {

            bool defaultColumns = false;

            if (app.Application != null && app.Application.Identification != null)
            {
                defaultColumns = defaultColumns
                    || app.Application.Identification.Name.ToLower().Contains(searchTerm)
                    || app.Application.Identification.APMID.ToString().ToLower().Contains(searchTerm)
                    || Localizer[app.Application.Identification.Status].ToString().ToLower().Contains(searchTerm)
                    || app.Application.Identification.WebURL.ToLower().Contains(searchTerm);
            }

            if (app.Application.Contact != null)
            {
                defaultColumns = defaultColumns || app.Application.Contact.GetManager().name.ToLower().Contains(searchTerm);
                defaultColumns = defaultColumns || app.Application.Contact.GetTeamLead().name.ToLower().Contains(searchTerm);
            }

            if (_selectedModuleFields != null)
            {
                foreach (var item in _selectedModuleFields)
                {
                    string field = item.Replace(" ", "");
                    string value = GetColumnValue(field, app.Application);
                    defaultColumns = defaultColumns || Localizer[value].ToString().ToLower().Contains(searchTerm);
                }
            }
            _pageIndex = 1;
            return Task.FromResult(defaultColumns);
        }

        /* Update the total and table datasource based on the search
         * results
         */
        private void GetFilteredList(List<ApplicationInformation> list)
        {
            applicationsTable = list.ToList();
            _total = applicationsTable.Count;
            StateHasChanged();
        }

        /* Change number of rows displayed on each
         * table page
         */
        private void PaginationChange(string number)
        {
            _pageSize = Int32.Parse(number);
            _pageIndex = 1;
            StateHasChanged();
        }

        /* Reset table page to the first page if any column
         * filters are applied
         */
        private void OnFilterChanges(QueryModel<ApplicationInformation> model)
        {
            if (model.FilterModel.Count > 0)
            {
                _pageIndex = 1;
                StateHasChanged();
            }
        }

        /* Given a field name (e.g. Visbility from the ApplicationIdentification module)
         * and an instance of Application, return the value for field from its correct module
         */
        private string GetColumnValue(string field, object obj)
        {
            try
            {
                string propertyName = mapper[field].Name;
                if (mapper[field].Name == "ApplicationIdentification")
                    propertyName = "Identification";

                // this returns the instance of a module (e.g. ApplicationIdentification)
                object module = obj.GetType().GetProperty(propertyName).GetValue(obj);

                if (mapper[field] == typeof(Contact))
                {
                    string contactInfo = module.GetType().GetProperty(field).GetValue(module).ToString();
                    return JsonConvert.DeserializeObject<ContactInfo>(contactInfo).name;
                }
                else
                {
                    // given the module instance, get the value from its field property
                    return module.GetType().GetProperty(field).GetValue(module).ToString();
                }
            }
            // return an empty string when certain modules don't exist 
            // for an application
            catch (Exception)
            {
                return "";
            }
        }
    }
}

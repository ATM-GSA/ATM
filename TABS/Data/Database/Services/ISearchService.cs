using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TABS.Data
{
    public interface ISearchService
    {
        Task<List<QuerySearchResult>> SearchAll(string query, IStringLocalizer<App> localizer, CancellationToken cancellationToken, bool isAdmin);
        Task<List<ModuleSearchResult>> SearchModuleFieldValues(string query, CancellationToken cancellationToken);
        Task<List<ModuleSearchResult>> SearchModuleName(string query, IStringLocalizer<App> localizer, CancellationToken cancellationToken);
        Task<List<ModuleSearchResult>> SearchModuleFieldName(string query, IStringLocalizer<App> localizer, CancellationToken cancellationToken);
    }
}
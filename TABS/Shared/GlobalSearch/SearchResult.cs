using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using TABS.Data;

namespace TABS.Shared.GlobalSearch
{
    public class SearchResult
    {
        public string PageTitle { get; set; }
        public string Breadcrumb { get; set; }
        public string Link { get; set; }
        public string Tags { get; set; }
        public List<(string, string)> Details { get; set; }

        public SearchResult(string pgTitle, string breadcrumb, string link, string tags = "")
        {
            PageTitle = pgTitle;
            Breadcrumb = breadcrumb;
            Link = link;
            Tags = tags;
        }

        public SearchResult(string pgTitle, string breadcrumb, string link, List<(string, string)> details, string tags = "")
        {
            PageTitle = pgTitle;
            Breadcrumb = breadcrumb;
            Link = link;
            Details = details;
            Tags = tags;
        }

        public static SearchResult FromQuerySearchResult(QuerySearchResult querySearchResult, IStringLocalizer<App> localizer)
        {
            if (querySearchResult is ModuleSearchResult moduleSearchResult)
            {
                return FromModuleSearchResult(moduleSearchResult, localizer);
            }

            throw new Exception($"Unsupported query search result type: {querySearchResult.GetType()}");
        }

        private static SearchResult FromModuleSearchResult(ModuleSearchResult moduleSearchResult, IStringLocalizer<App> localizer)
        {
            string moduleName = moduleSearchResult.Module.GetModuleType().ToString();
            string breadCrumb = string.Format("{0} / {1} / {2}", localizer["Application"], moduleSearchResult.Application.Identification.Name, localizer[moduleName]);
            string tags = "";

            if (moduleSearchResult.Application.IsArchived)
            {
                tags = localizer["Archived"];
            }

            if (moduleSearchResult.Application.IsDeleted)
            {
                tags = localizer["Deleted"];
                breadCrumb = string.Format("{0} / {1}", localizer["RecoverApplications"], moduleSearchResult.Application.Identification.Name);
            }


            List<(string, string)> details = new();
            Dictionary<string, int> nestedModuleDetailsIndex = new(); // maps the list prop with the index in details

            foreach (ModuleProperty prop in moduleSearchResult.Properties)
            {
                string display;
                if (prop.Value is IList && prop.Value.GetType().IsGenericType)
                {
                    display = localizer["GlobalSearch.ListItems", (prop.Value as IList).Count];
                    nestedModuleDetailsIndex[prop.Name] = details.Count; // this list prop's index in the details list
                }
                else
                {
                    display = prop.Value == null ? "" : localizer[prop.Value.ToString()];
                }

                details.Add(($"{localizer[prop.LocalizationKey]}: ", $"{display}"));
            }

            foreach (ModuleSearchResult childResult in moduleSearchResult.ChildResults)
            {
                string matchesString = localizer["GlobalSearch.NestedModuleMatches", childResult.Properties.Count];

                // check if we already displayed this child result as part of the parent result
                bool found = false;
                foreach(ModuleProperty prop in moduleSearchResult.Properties)
                {
                    if (prop.Value is IList && prop.Value.GetType().IsGenericType)
                    {
                        if (prop.Type.GetGenericArguments()[0] == childResult.Module.GetType())
                        {
                            // we found an existing details entry, so we append to it
                            found = true;
                            int idx = nestedModuleDetailsIndex[prop.Name];
                            details[idx] = ($"{localizer[prop.LocalizationKey]}: ", $"{matchesString} / {details[idx].Item2}");
                        }
                    }
                    else
                    {
                        // Handle this if we have nested modules that are not lists
                        // We don't have modules like this right now, so do nothing
                    }
                }

                // If we didn't an existing entry, then add it to the details list
                if (!found)
                {
                    details.Add(($"{localizer[childResult.Module.GetModuleType().ToString()]}: ", $"{matchesString}"));
                }
            }

            return new SearchResult(
                $"{localizer[moduleName]}",
                breadCrumb,
                $"applications/{moduleSearchResult.Application.ShortID}/{moduleName}",
                details,
                tags: tags
            );
        }
    }
}

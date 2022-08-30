using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TABS.Data
{
    /// <summary>
    /// Represents a boolean logic operator
    /// </summary>
    public class BooleanOperator
    {
        public string Pattern { get; }

        public static readonly BooleanOperator And = new("&&");
        public static readonly BooleanOperator Or = new("||");
        public static readonly List<BooleanOperator> AllOperators = new() { And, Or };

        public BooleanOperator(string pattern)
        {
            Pattern = pattern;
        }
    }

    /// <summary>
    /// Represents a search result type using bit flags
    /// </summary>
    public enum SearchResultType
    {
        Undefined = 0,      // 000
        FieldValue = 1,     // 001
        FieldName = 2,      // 010
        ModuleName = 4      // 100
    }

    /// <summary>
    /// Abstract class that models a generate search result from querying the database
    /// </summary>
    public abstract class QuerySearchResult
    {
        public SearchResultType MatchType { get; set; }

        public double MatchScore { get; set; }

        public bool IsLoaded { get; set; } = false; // indicates if this result has been fully loaded

        public virtual async Task<bool> LoadData(CancellationToken CancellationToken, Application app = null)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        public abstract void Merge(QuerySearchResult other);

        public abstract bool IsSimilar(QuerySearchResult other);
    }

    /// <summary>
    /// Models a search result for app modules
    /// </summary>
    public class ModuleSearchResult : QuerySearchResult
    {
        public Application Application { get; set; } = null;
        public IApplicationModule Module { get; set; }
        public List<ModuleProperty> Properties { get; set; } = new();
        public List<SearchResultType> PropertyResultMatchType { get; set; } = new();
        public List<ModuleSearchResult> ChildResults { get; set; } = new();

        public int GetApplicationId()
        {
            if (IsLoaded)
            {
                return Application.ApplicationID;
            }

            if (Module is Module m)
            {
                return m.ApplicationID;
            }
            else if (Module is ListModule lm)
            {
                return lm.GetParentModule()?.ApplicationID ?? -1;
            }

            return -1;
        }

        public override async Task<bool> LoadData(CancellationToken cancellationToken, Application providedApp = null)
        {
            if (providedApp != null)
            {
                // given an app to load, so no need to load from db
                Application = providedApp;
                IsLoaded = true;
                ChildResults.ForEach(async res => await res.LoadData(cancellationToken, providedApp));
                return IsLoaded;
            }

            if (Application != null || IsLoaded)
            {
                IsLoaded = true;
                return IsLoaded;
            }

            int appId = -1;

            // check application ids
            if (Module is Module m)
            {
                appId = m.ApplicationID;
            }
            else if (Module is ListModule lm)
            {
                if (lm.GetParentModule() == null)
                {
                    return false;
                }
                appId = lm.GetParentModule().ApplicationID;
            }

            try
            {
                Application = await DBContextFactory.CreateInstance().Applications.Include(app => app.Identification).Where(app => app.ApplicationID == appId).FirstOrDefaultAsync(cancellationToken);
                IsLoaded = !cancellationToken.IsCancellationRequested && Application != null;
                ChildResults.ForEach(async res => await res.LoadData(cancellationToken, providedApp));
            }
            catch
            {
                IsLoaded = false;
            }

            return IsLoaded;
        }

        public bool AddProperty(ModuleProperty moduleProperty, SearchResultType matchType, bool ignoreDuplicate = false)
        {
            // only add property if it doesn't duplicate
            if (ignoreDuplicate || Properties.Find(p => p.Name == moduleProperty.Name && (Module.GetModuleType() > 0 ? p.Value is IList || p.Value.Equals(moduleProperty.Value) : true)) == null)
            {
                // just add this property to the property list
                Properties.Add(moduleProperty);
                PropertyResultMatchType.Add(matchType);
                return true;
            }

            return false;
        }

        public override void Merge(QuerySearchResult otherQueryResult)
        {
            ModuleSearchResult other = otherQueryResult as ModuleSearchResult;
            if (other == null) return;

            if (other.Module.GetModuleType() != Module.GetModuleType())
            {
                // different types, can't be merged
                return;
            }

            // merge properties
            MatchType |= other.MatchType; // slap on the other result type
            for (int i = 0; i < other.Properties.Count; i++)
            {
                AddProperty(other.Properties[i], other.PropertyResultMatchType[i]);
            }

            // recursively merge children
            foreach (ModuleSearchResult otherChildResult in other.ChildResults)
            {
                ModuleSearchResult childResult = ChildResults.Find(cr => cr.Module.GetModuleType() == otherChildResult.Module.GetModuleType());
                if (childResult != null)
                {
                    childResult.Merge(otherChildResult);
                }
            }
        }

        public override bool IsSimilar(QuerySearchResult other)
        {
            if (other is ModuleSearchResult msr)
            {
                bool isSimilar = (!IsLoaded || msr.Application?.ApplicationID == Application?.ApplicationID);
                isSimilar = isSimilar && GetApplicationId() == msr.GetApplicationId();

                return isSimilar;
            }

            return false;
        }
    }

    /// <summary>
    /// Models a search result for app modules in the detailed app view
    /// </summary>
    public class DetailedModuleSearchResult : ModuleSearchResult
    {
        public override bool IsSimilar(QuerySearchResult other)
        {
            if (other is DetailedModuleSearchResult dmsr)
            {
                bool isSimilar = Module.GetModuleType() == dmsr.Module.GetModuleType();
                isSimilar = isSimilar && (!IsLoaded || dmsr.Application?.ApplicationID == Application?.ApplicationID);
                isSimilar = isSimilar && GetApplicationId() == dmsr.GetApplicationId();
                return isSimilar;
            }

            return false;
        }
    }

    /// <summary>
    /// Used to search the database for keywords
    /// </summary>
    public class SearchService : ISearchService
    {

        #region Search with all method with pattern matching
        /// <summary>
        /// General purpose search function that searches everything (in this order):
        /// - Module Names
        /// - Module Field names
        /// - Module Field values
        /// Also checks if the given search has any boolean patterns (e.g. "||" or "&&") and processes the results accordingly
        /// </summary>
        /// <param name="searchQuery">The search query</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <param name="isAdmin">Optional boolean to allow admin search results</param>
        /// <returns>List of query search results</returns>
        public async Task<List<QuerySearchResult>> SearchAll(string searchQuery, IStringLocalizer<App> localizer = null, CancellationToken cancellationToken = default, bool isAdmin = false)
        {
            if (searchQuery.Length == 0)
            {
                Console.WriteLine("Empty search query");
                return new List<QuerySearchResult>();
            }

            Tuple<List<BooleanOperator>, List<string>> processedQuery = PatternMatch(searchQuery);
            List<BooleanOperator> ops = processedQuery.Item1;
            List<string> searches = processedQuery.Item2;

            List<List<QuerySearchResult>> allResults = new();
            foreach (string search in searches)
            {
                List<ModuleSearchResult> results = new();
                List<ModuleSearchResult> moduleSearchResults = new();
                List<ModuleSearchResult> fieldNameSearchResults = new();
                List<ModuleSearchResult> fieldSearchResults = new();

                List<Task> taskList = new()
                {
                    Task.Run(async () =>
                    {
                        moduleSearchResults = await SearchModuleName(search, localizer);
                    }, cancellationToken),
                    Task.Run(async () =>
                    {
                        fieldNameSearchResults = await SearchModuleFieldName(search, localizer);
                    }, cancellationToken),
                    Task.Run(async () =>
                    {
                        fieldSearchResults = await SearchModuleFieldValues(search);
                    }, cancellationToken),
                };

                await Task.WhenAll(taskList);

                results.AddRange(fieldSearchResults);
                results.AddRange(fieldNameSearchResults);
                results.AddRange(moduleSearchResults);

                // sort it again
                results = results.OrderByDescending(x => x.MatchScore).ToList();

                // remove duplicates and merge results
                Dictionary<Tuple<int, ModuleTypeEnum>, ModuleSearchResult> seenResults = new();
                foreach (ModuleSearchResult result in results)
                {
                    Tuple<int, ModuleTypeEnum> key = new((result.Module as Module).ApplicationID, result.Module.GetModuleType());

                    if (seenResults.ContainsKey(key))
                    {
                        // update existing result
                        seenResults[key].Merge(result);
                    }
                    else
                    {
                        // add as a new result
                        seenResults[key] = result;
                    }
                }

                // filter out deleted or archived apps
                // TODO: maybe we take in extra flags so archived/deleted apps appear in the results for admins
                List<int> invalidAppIds = (await DBContextFactory.CreateInstance().Applications.Where(app => (app.IsDeleted && !isAdmin) || !app.IsComplete).ToListAsync(cancellationToken)).Select(app => app.ApplicationID).ToList();
                foreach (int invalidId in invalidAppIds)
                {
                    List<Tuple<int, ModuleTypeEnum>> keysToDelete = seenResults.Where(pair => pair.Key.Item1 == invalidId).Select(pair => pair.Key).ToList();
                    // remove these results
                    keysToDelete.ForEach(key =>
                    {
                        seenResults.Remove(key);
                    });
                }
                allResults.Add(seenResults.Select(pair => pair.Value).Cast<QuerySearchResult>().ToList());
            }

            return CombineResults(allResults, ops);
        }
        #endregion

        #region Search all application module fields with search
        /// <summary>
        /// Search all module fields with the given search string
        /// </summary>
        /// <param name="search">The search term</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of field search results</returns>
        /// 
        public async Task<List<ModuleSearchResult>> SearchModuleFieldValues(string search, CancellationToken cancellationToken = default)
        {
            if (search.Length == 0)
            {
                Console.WriteLine("Empty search string");
                return new List<ModuleSearchResult>();
            }

            // TODO: Sanitize the search!
            string queryString = $"%{search}%";

            // Fetch all modules with matches from the database
            // (creates a task for each module type)
            List<Task<IEnumerable<IApplicationModule>>> taskList = new()
            {
                Task<IEnumerable<IApplicationModule>>.Factory.StartNew(() =>
                {
                    Console.WriteLine("Scanning Application Identifications...");
                    return DBContextFactory.CreateInstance().ApplicationIdentifications.Where(entity => entity.APMID.ToString().Equals(search)
                                                                                                        || EF.Functions.Like(entity.Name, queryString)
                                                                                                        || EF.Functions.Like(entity.Title, queryString)
                                                                                                        || EF.Functions.Like(entity.Status, queryString)
                                                                                                        || EF.Functions.Like(entity.Visibility, queryString)
                                                                                                        || EF.Functions.Like(entity.WebURL, queryString)
                                                                                                        || EF.Functions.Like(entity.ClientBranch, queryString)
                    ).AsNoTracking().ToList();
                }, cancellationToken: cancellationToken),
                Task<IEnumerable<IApplicationModule>>.Factory.StartNew(() =>
                {
                    Console.WriteLine("Scanning Architectures...");
                    return DBContextFactory.CreateInstance().Architectures.Where(entity => EF.Functions.Like(entity.AppPlatform, queryString)
                                                                                           || EF.Functions.Like(entity.SMARTFramework, queryString)
                                                                                           || EF.Functions.Like(entity.SMARTUpgradePlanning, queryString)
                                                                                           || EF.Functions.Like(entity.CDTSVersion, queryString)
                                                                                           || EF.Functions.Like(entity.NETVersion, queryString)
                                                                                           || EF.Functions.Like(entity.SEASVersion, queryString)
                    ).AsNoTracking().ToList();
                }, cancellationToken: cancellationToken),
                Task<IEnumerable<IApplicationModule>>.Factory.StartNew(() =>
                {
                    Console.WriteLine("Scanning Contacts...");
                    return DBContextFactory.CreateInstance().Contacts.Where(entity => EF.Functions.Like(entity.Manager, queryString)
                                                                                      || EF.Functions.Like(entity.TeamLead, queryString)
                                                                                      || EF.Functions.Like(entity.TechLead, queryString)
                                                                                      || EF.Functions.Like(entity.ClientManager, queryString)
                                                                                      || EF.Functions.Like(entity.ClientLead, queryString)
                    ).AsNoTracking().ToList();
                }, cancellationToken: cancellationToken),
                Task<IEnumerable<IApplicationModule>>.Factory.StartNew(() =>
                {
                    Console.WriteLine("Scanning Securities...");
                    return DBContextFactory.CreateInstance().Securities.Where(entity => EF.Functions.Like(entity.ExemptionReasoning, queryString)
                                                                                        || EF.Functions.Like(entity.SecurityLevel, queryString)
                    ).AsNoTracking().ToList();
                }, cancellationToken: cancellationToken),
                Task<IEnumerable<IApplicationModule>>.Factory.StartNew(() =>
                {
                    Console.WriteLine("Scanning Reports...");
                    return DBContextFactory.CreateInstance().Reports.Where(entity => EF.Functions.Like(entity.Accessibility, queryString)
                                                                                     || EF.Functions.Like(entity.Performance, queryString)
                    ).AsNoTracking().ToList();
                }, cancellationToken: cancellationToken),
                Task<IEnumerable<IApplicationModule>>.Factory.StartNew(() =>
                {
                    Console.WriteLine("Scanning Fortify Scans...");
                    return DBContextFactory.CreateInstance().FortifyScans.Include(fs => fs.Security).Where(fs => EF.Functions.Like(fs.Name, queryString)
                                                                                                                 || EF.Functions.Like(fs.Notes, queryString)
                                                                                                                 || EF.Functions.Like(fs.ReportLink, queryString)
                    ).AsNoTracking().ToList();
                }, cancellationToken: cancellationToken),
                Task<IEnumerable<IApplicationModule>>.Factory.StartNew(() =>
                {
                    Console.WriteLine("Scanning Servers...");
                    return DBContextFactory.CreateInstance().Servers.Include(server => server.ServerEnvironment).Where(server => EF.Functions.Like(server.Name, queryString)
                                                                                                                                 || EF.Functions.Like(server.URL, queryString)
                                                                                                                                 || EF.Functions.Like(server.Version, queryString)
                    ).AsNoTracking().ToList();
                }, cancellationToken: cancellationToken),
                Task<IEnumerable<IApplicationModule>>.Factory.StartNew(() =>
                {
                    Console.WriteLine("Scanning Databases...");
                    return DBContextFactory.CreateInstance().Databases.Include(db => db.DatabaseEnvironment).Where(db => EF.Functions.Like(db.Name, queryString)
                                                                                                                         || EF.Functions.Like(db.URL, queryString)
                                                                                                                         || EF.Functions.Like(db.Version, queryString)
                                                                                                                         || EF.Functions.Like(db.Platform, queryString)
                    ).AsNoTracking().ToList();
                }, cancellationToken: cancellationToken),
            };

            await Task.WhenAll(taskList.ToArray());

            List<IApplicationModule> rawResults = new List<IApplicationModule>();
            taskList.ForEach(task => rawResults.AddRange(task.Result));

            // check for cancellation
            if (cancellationToken.IsCancellationRequested)
            {
                return new List<ModuleSearchResult>();
            }

            Console.WriteLine("Processing results...");

            // local function for walking the module fields and its nested modules
            Dictionary<Tuple<int, ModuleTypeEnum>, ModuleSearchResult> seenResults = new();
            void ModuleWalker(IApplicationModule module)
            {
                int appId = -1;
                if (module is Module m)
                {
                    appId = m.ApplicationID;
                }
                else if (module is ListModule lm)
                {
                    appId = lm.GetParentModule() != null ? lm.GetParentModule().ApplicationID : -1;
                }

                if (appId == -1) return;

                Dictionary<string, ModuleProperty> properties = module.GetProperties();
                foreach (KeyValuePair<string, ModuleProperty> property in properties)
                {
                    ModuleProperty propertyToAdd = property.Value;
                    bool isList = propertyToAdd.Value is IList && propertyToAdd.Type.IsGenericType;

                    if (!isList)
                    {

                        double score = GetMatchScore(search, property.Value.Value == null ? "" : property.Value.Value.ToString());
                        if (score == 0) continue;

                        Tuple<int, ModuleTypeEnum> key = new(appId, module.GetModuleType());
                        ModuleSearchResult newResult = null;

                        if (seenResults.ContainsKey(key))
                        {
                            seenResults[key].AddProperty(propertyToAdd, SearchResultType.FieldValue, ignoreDuplicate: true);
                            seenResults[key].MatchScore += score;
                        }
                        else
                        {
                            newResult = new()
                            {
                                Module = module,
                                Properties = new() { propertyToAdd },
                                PropertyResultMatchType = new() { SearchResultType.FieldValue },
                                MatchType = SearchResultType.FieldValue,
                                MatchScore = score
                            };
                            seenResults.Add(key, newResult);
                        }

                        // check for cancellation
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                    else
                    {
                        // check if list is a nested module list
                        if (propertyToAdd.Type.GetGenericArguments().Single().IsSubclassOf(typeof(ListModule)))
                        {
                            foreach (ListModule lm in (propertyToAdd.Value as IList).Cast<ListModule>().ToList())
                            {
                                ModuleWalker(lm); // recurse on nested modules
                            }
                        }
                    }
                }
            }

            foreach (IApplicationModule module in rawResults)
            {
                ModuleWalker(module);
            }

            Console.WriteLine("Combining and sorting results...");

            // combine the results
            Dictionary<Tuple<int, ModuleTypeEnum>, ModuleSearchResult> combinedResultsDict = new();
            foreach (KeyValuePair<Tuple<int, ModuleTypeEnum>, ModuleSearchResult> pair in seenResults)
            {
                if (((int)pair.Key.Item2) < 0)
                {
                    // is a list module
                    ModuleTypeEnum parentType = (ModuleTypeEnum)(-(int)pair.Key.Item2);
                    Tuple<int, ModuleTypeEnum> key = new(pair.Key.Item1, parentType);
                    ModuleSearchResult nestedSearchResult = pair.Value;
                    if (combinedResultsDict.ContainsKey(key))
                    {
                        combinedResultsDict[key].ChildResults.Add(nestedSearchResult);
                        combinedResultsDict[key].MatchScore += nestedSearchResult.MatchScore;
                    }
                    else
                    {
                        combinedResultsDict[key] = new()
                        {
                            Module = (nestedSearchResult.Module as ListModule).GetParentModule(),
                            MatchScore = nestedSearchResult.MatchScore,
                            ChildResults = new() { nestedSearchResult },
                            MatchType = SearchResultType.FieldValue
                        };
                    }
                }
                else
                {
                    combinedResultsDict[pair.Key] = pair.Value;
                }
            }

            return combinedResultsDict.OrderByDescending(pair => pair.Value.MatchScore).Select(pair => pair.Value).ToList();
        }
        #endregion

        #region Search for module field name
        /// <summary>
        /// Search for a field in a module with the given name
        /// </summary>
        /// <param name="search">Search input</param>
        /// <param name="localizer">Optional localizer</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of module field name search results</returns>
        /// 
        public async Task<List<ModuleSearchResult>> SearchModuleFieldName(string search, IStringLocalizer<App> localizer = null, CancellationToken cancellationToken = default)
        {
            if (search.Length == 0)
            {
                Console.WriteLine("Empty search string");
                return new List<ModuleSearchResult>();
            }

            Console.WriteLine($"Searching for modules fields with name \"{search}\"");

            // find the fields that match the search
            List<Tuple<double, ModuleTypeEnum, ModuleProperty>> matchingModules = new();
            foreach (ModuleTypeEnum moduleType in Enum.GetValues(typeof(ModuleTypeEnum)))
            {
                if (moduleType == ModuleTypeEnum.Undefined) continue;

                IApplicationModule module = ModuleTypeEnumExtensions.ToModule(moduleType);

                if (module == null) continue;

                Dictionary<string, ModuleProperty> fields = module.GetProperties();

                foreach (KeyValuePair<string, ModuleProperty> field in fields)
                {
                    double score = GetMatchScore(search, localizer == null ? field.Value.Name : localizer[field.Value.LocalizationKey]);
                    if (score > 0)
                    {
                        matchingModules.Add(new Tuple<double, ModuleTypeEnum, ModuleProperty>(score, moduleType, field.Value));
                    }
                }
            }

            if (matchingModules.Count == 0) return new List<ModuleSearchResult>(); // return empty results

            // sort by score
            List<Tuple<double, ModuleTypeEnum, ModuleProperty>> sortedMatchingModules = matchingModules.OrderByDescending(x => x.Item1).ToList();

            // get all records of this module type
            List<Tuple<Module, ModuleProperty>> rawModules = new();
            Dictionary<Tuple<int, ModuleTypeEnum>, ModuleSearchResult> seenResults = new();

            foreach (Tuple<double, ModuleTypeEnum, ModuleProperty> matchingModule in sortedMatchingModules)
            {
                double score = matchingModule.Item1;
                ModuleTypeEnum moduleType = matchingModule.Item2;
                ModuleProperty matchedProperty = matchingModule.Item3;
                bool isListModule = moduleType < 0; // is a list module field
                ModuleTypeEnum parentType = isListModule ? ((ModuleTypeEnum)(-(int)moduleType)) : moduleType;

                List<Module> dbModules = await GetAllModuleOfType(parentType, cancellationToken);

                foreach (Module module in dbModules)
                {
                    Tuple<int, ModuleTypeEnum> key = new(module.ApplicationID, moduleType);

                    if (isListModule)
                    {
                        // process the list module
                        List<ModuleProperty> childModules = module.GetChildModules();
                        foreach (ModuleProperty childModuleProp in childModules)
                        {
                            if (childModuleProp.Value is IList && childModuleProp.Type.IsGenericType && childModuleProp.Type.GetGenericArguments().Single().IsSubclassOf(typeof(ListModule)))
                            {
                                List<ListModule> listModules = (childModuleProp.Value as IList).Cast<ListModule>().ToList();
                                if (listModules.Count() > 0)
                                {
                                    // there are modules in the list, so we use them
                                    foreach (ListModule lm in listModules)
                                    {
                                        if (lm.GetModuleType() != moduleType)
                                        {
                                            // not this one
                                            break;
                                        }

                                        if (seenResults.ContainsKey(key))
                                        {
                                            seenResults[key].AddProperty(lm.GetProperties()[matchedProperty.Name], SearchResultType.FieldName);
                                        }
                                        else
                                        {
                                            seenResults[key] = new()
                                            {
                                                Module = lm,
                                                MatchScore = score,
                                                Properties = new() { lm.GetProperties()[matchedProperty.Name] },
                                                PropertyResultMatchType = new() { SearchResultType.FieldName },
                                                MatchType = SearchResultType.FieldName
                                            };
                                        }
                                    }
                                }
                                else
                                {
                                    // there are no modules in the list, so we make a dummy one
                                    ListModule lm = (ListModule)ModuleTypeEnumExtensions.ToModule(moduleType);
                                    lm.SetParentModule(module);
                                    seenResults[key] = new()
                                    {
                                        Module = lm,
                                        MatchScore = score,
                                        Properties = new() { lm.GetProperties()[matchedProperty.Name] },
                                        PropertyResultMatchType = new() { SearchResultType.FieldName },
                                        MatchType = SearchResultType.FieldName
                                    };
                                }
                            }
                        }
                    }
                    else
                    {
                        // process the module
                        if (seenResults.ContainsKey(key))
                        {
                            seenResults[key].AddProperty(module.GetProperties()[matchedProperty.Name], SearchResultType.FieldValue);
                            seenResults[key].MatchScore += score;
                        }
                        else
                        {
                            seenResults[key] = new()
                            {
                                Module = module,
                                MatchScore = score,
                                Properties = new() { module.GetProperties()[matchedProperty.Name] },
                                PropertyResultMatchType = new() { SearchResultType.FieldName },
                                MatchType = SearchResultType.FieldName,
                            };
                        }
                    }
                }
            }

            // combine the results
            Dictionary<Tuple<int, ModuleTypeEnum>, ModuleSearchResult> combinedResultsDict = new();
            foreach (KeyValuePair<Tuple<int, ModuleTypeEnum>, ModuleSearchResult> pair in seenResults)
            {
                if (((int)pair.Key.Item2) < 0)
                {
                    // is a list module
                    ModuleTypeEnum parentType = (ModuleTypeEnum)(-(int)pair.Key.Item2);
                    Tuple<int, ModuleTypeEnum> key = new(pair.Key.Item1, parentType);
                    ModuleSearchResult nestedSearchResult = pair.Value;
                    if (combinedResultsDict.ContainsKey(key))
                    {
                        combinedResultsDict[key].ChildResults.Add(nestedSearchResult);
                        combinedResultsDict[key].MatchScore += nestedSearchResult.MatchScore;
                    }
                    else
                    {
                        combinedResultsDict[key] = new()
                        {
                            Module = (nestedSearchResult.Module as ListModule).GetParentModule(),
                            MatchScore = nestedSearchResult.MatchScore,
                            ChildResults = new() { nestedSearchResult },
                            MatchType = SearchResultType.FieldName
                        };
                    }
                }
                else
                {
                    combinedResultsDict[pair.Key] = pair.Value;
                }
            }


            return combinedResultsDict.OrderByDescending(pair => pair.Value.MatchScore).Select(pair => pair.Value).ToList();
        }
        #endregion

        #region Search for modules given a module name
        /// <summary>
        /// Search for modules with the given name
        /// </summary>
        /// <param name="search">Search input</param>
        /// <param name="localizer">Optional localizer</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of module name search results</returns>
        /// 
        public async Task<List<ModuleSearchResult>> SearchModuleName(string search, IStringLocalizer<App> localizer = null, CancellationToken cancellationToken = default)
        {
            if (search.Length == 0)
            {
                Console.WriteLine("Empty search string");
                return new List<ModuleSearchResult>();
            }

            Console.WriteLine($"Searching for modules with name {search}");


            // find the closest module
            List<Tuple<double, ModuleTypeEnum>> matchingModules = new();
            foreach (ModuleTypeEnum module in ModuleTypeEnumExtensions.GetDisplayedModules())
            {
                double score = GetMatchScore(search, localizer == null ? module.ToString() : localizer[module.ToString()]);
                if (score > 0)
                {
                    matchingModules.Add(new Tuple<double, ModuleTypeEnum>(score, module));
                }
            }

            if (matchingModules.Count == 0) return new(); // return empty results

            List<Tuple<double, ModuleTypeEnum>> sortedMatchingModules = matchingModules.OrderByDescending(x => x.Item1).ToList();

            Console.WriteLine("Gathering results...");

            // get all records of this module type
            Dictionary<Tuple<int, ModuleTypeEnum>, ModuleSearchResult> processedResultDict = new();
            foreach (Tuple<double, ModuleTypeEnum> sortedModule in sortedMatchingModules)
            {
                double score = sortedModule.Item1;
                ModuleTypeEnum moduleType = sortedModule.Item2;

                // is a regular module
                List<Module> allModules = await GetAllModuleOfType(moduleType, cancellationToken);
                foreach (Module module in allModules)
                {
                    Tuple<int, ModuleTypeEnum> key = new(module.ApplicationID, moduleType);
                    if (!processedResultDict.ContainsKey(key))
                    {
                        processedResultDict[key] = new()
                        {
                            Module = module,
                            MatchScore = score,
                            MatchType = SearchResultType.ModuleName
                        };
                    }
                }
            }

            Console.WriteLine("Sorting results...");

            return processedResultDict.OrderByDescending(pair => pair.Value.MatchScore).Select(pair => pair.Value).ToList();
        }
        #endregion

        #region Search application
        /// <summary>
        /// Search the given application
        /// Also checks if the given search has any boolean patterns (e.g. "||" or "&&") and processes the results accordingly
        /// </summary>
        /// <param name="app">The app to be searched</param>
        /// <param name="searchQuery">Search query</param>
        /// <param name="localizer">Optional localizer</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A list of module search results</returns>
        /// 
        public List<DetailedModuleSearchResult> SearchApplication(Application app, string searchQuery, IStringLocalizer<App> localizer = null, CancellationToken cancellationToken = default)
        {
            if (searchQuery.Length == 0)
            {
                Console.WriteLine("Empty search string");
                return new();
            }

            Console.WriteLine($"Searching application \"{app.Identification?.Name}\" for \"{searchQuery}\"");

            Tuple<List<BooleanOperator>, List<string>> processedQuery = PatternMatch(searchQuery);
            List<BooleanOperator> ops = processedQuery.Item1;
            List<string> searches = processedQuery.Item2;

            List<List<QuerySearchResult>> allResults = new();
            foreach (string search in searches)
            {
                Dictionary<ModuleTypeEnum, Module> appModules = app.GetModules();
                Dictionary<ModuleTypeEnum, DetailedModuleSearchResult> resultsDict = new();

                // local function for walking the module fields and its nested modules
                void ModuleWalker(IApplicationModule module)
                {
                    List<ModuleProperty> matchingProperties = new();
                    Dictionary<string, ModuleProperty> moduleProperties = module.GetProperties();

                    // match the module name
                    double moduleNameScore = GetMatchScore(search, localizer == null ? module.GetModuleType().ToString() : localizer[module.GetModuleType().ToString()]);
                    if (moduleNameScore > 0)
                    {
                        resultsDict[module.GetModuleType()] = new()
                        {
                            Application = app,
                            Module = module,
                            MatchScore = moduleNameScore,
                            IsLoaded = true,
                            MatchType = SearchResultType.ModuleName
                        };
                    }

                    foreach (KeyValuePair<string, ModuleProperty> pair in moduleProperties)
                    {
                        ModuleProperty prop = pair.Value;
                        bool isList = prop.Value is IList && prop.Type.IsGenericType;

                        // match field name and value
                        double fieldNameScore = GetMatchScore(search, localizer == null ? prop.Name : localizer[prop.LocalizationKey]);
                        double fieldValScore = isList ? 0 : GetMatchScore(search, prop.Value == null ? "" : localizer[prop.Value.ToString()]); // don't score list values

                        if ((fieldNameScore + fieldValScore) > 0)
                        {
                            SearchResultType resultType = SearchResultType.Undefined;

                            if (fieldNameScore > 0)
                                resultType |= SearchResultType.FieldName;
                            if (fieldValScore > 0)
                                resultType |= SearchResultType.FieldValue;

                            if (resultsDict.ContainsKey(module.GetModuleType()))
                            {
                                // update the result
                                resultsDict[module.GetModuleType()].AddProperty(prop, resultType);
                                resultsDict[module.GetModuleType()].MatchScore += (fieldNameScore + fieldValScore);
                                resultsDict[module.GetModuleType()].MatchType |= resultType;
                            }
                            else
                            {
                                // create new result
                                resultsDict[module.GetModuleType()] = new()
                                {
                                    Application = app,
                                    Module = module,
                                    Properties = new() { prop },
                                    PropertyResultMatchType = new() { resultType },
                                    MatchType = resultType,
                                    MatchScore = (fieldNameScore + fieldValScore),
                                    IsLoaded = true,
                                };
                            }
                        }

                        if (isList)
                        {
                            // check if list is a nested module list
                            if (prop.Value.GetType().GetGenericArguments().Single().IsSubclassOf(typeof(ListModule)))
                            {
                                foreach (ListModule lm in (prop.Value as IList).Cast<ListModule>().ToList())
                                {
                                    ModuleWalker(lm); // recurse on nested modules
                                }
                            }
                        }
                    }
                }

                // walk all modules in this app (this will also recursively walk the nested modules)
                foreach (KeyValuePair<ModuleTypeEnum, Module> pair in appModules)
                {
                    if (pair.Value == null) continue;
                    ModuleWalker(pair.Value);
                }

                // combine the results
                Dictionary<ModuleTypeEnum, DetailedModuleSearchResult> combinedResultsDict = new();
                foreach (KeyValuePair<ModuleTypeEnum, DetailedModuleSearchResult> pair in resultsDict)
                {
                    if (((int)pair.Key) < 0)
                    {
                        // is a list module
                        ModuleTypeEnum parentType = (ModuleTypeEnum)(-(int)pair.Key);
                        ModuleSearchResult nestedSearchResult = pair.Value;
                        if (combinedResultsDict.ContainsKey(parentType))
                        {
                            combinedResultsDict[parentType].ChildResults.Add(nestedSearchResult);
                            combinedResultsDict[parentType].MatchScore += nestedSearchResult.MatchScore;
                            combinedResultsDict[parentType].MatchType |= nestedSearchResult.MatchType;
                        }
                        else
                        {
                            combinedResultsDict[parentType] = new()
                            {
                                Application = app,
                                Module = (nestedSearchResult.Module as ListModule).GetParentModule(),
                                MatchScore = nestedSearchResult.MatchScore,
                                ChildResults = new() { nestedSearchResult },
                                IsLoaded = true,
                                MatchType = nestedSearchResult.MatchType
                            };
                        }
                    }
                    else
                    {
                        combinedResultsDict[pair.Key] = pair.Value;
                    }
                }

                allResults.Add(combinedResultsDict.Select(pair => pair.Value).Cast<QuerySearchResult>().ToList());
            }

            return CombineResults(allResults, ops).Cast<DetailedModuleSearchResult>().ToList();
        }
        #endregion

        /// <summary>
        /// General purpose combine function that will merge results by applying the given boolean operators
        /// Note that the order of operations is left to right (there's no boolean BEDMAS/BODMAS)
        /// 
        /// Precondition: allResults.Count == ops.Count + 1
        /// 
        /// </summary>
        /// <param name="allResults">Set of results</param>
        /// <param name="ops">Set of boolean operators to apply</param>
        /// <returns>The merged search results</returns>
        private List<QuerySearchResult> CombineResults(List<List<QuerySearchResult>> allResults, List<BooleanOperator> ops)
        {
            if (allResults.Count == 0)
            {
                return new();
            }

            List<QuerySearchResult> currResults = allResults[0];
            for (int i = 1; i < allResults.Count(); i++)
            {
                List<QuerySearchResult> results = allResults[i];
                BooleanOperator op = ops[i - 1];

                if (op == BooleanOperator.And)
                {
                    // add only the similar ones
                    // need to use a new list to keep operator symmetry (i.e. A && B == B && A)
                    List<QuerySearchResult> newResults = new();
                    foreach (QuerySearchResult currRes in currResults)
                    {
                        {
                            foreach (QuerySearchResult result in results)
                                if (currRes.IsSimilar(result))
                                {
                                    if (!newResults.Contains(currRes)) newResults.Add(currRes);
                                    if (!newResults.Contains(result)) newResults.Add(result);
                                }
                        }
                    }
                    currResults = newResults;
                }
                else
                {
                    // default logic - includes OR logic as well
                    currResults.AddRange(results);
                }

                // sort and merge results
                currResults = currResults.OrderByDescending(res => res.MatchScore).ToList();
                Dictionary<object, QuerySearchResult> combinedResults = new();
                List<QuerySearchResult> resultsToRemove = new();
                foreach (QuerySearchResult result in currResults)
                {
                    if (result is ModuleSearchResult msr)
                    {
                        // handle module search results
                        Tuple<int, ModuleTypeEnum> key = new Tuple<int, ModuleTypeEnum>(msr.GetApplicationId(), msr.Module.GetModuleType());
                        if (combinedResults.ContainsKey(key))
                        {
                            combinedResults[key].Merge(msr);
                            resultsToRemove.Add(result);
                        }
                        else
                        {
                            combinedResults[key] = msr;
                        }
                    }
                }
                foreach (QuerySearchResult removedResult in resultsToRemove)
                {
                    currResults.Remove(removedResult);
                }
            }

            return currResults.OrderByDescending(res => res.MatchScore).ToList();
        }

        /// <summary>
        /// Extracts the boolean operators from the given query and builds a list of search terms along with the operators used
        /// </summary>
        /// <param name="query">The query to be processed</param>
        /// <returns>A tuple where item1 is all the operators used from left to right, and item 2 is the list of search terms</returns>
        public Tuple<List<BooleanOperator>, List<string>> PatternMatch(string query)
        {
            List<BooleanOperator> ops = new();
            List<string> searches = new();
            string currSubstring = query;

            while (true) // don't try this at home
            {
                BooleanOperator opUsed = null;
                int bestIdx = int.MaxValue;
                foreach (BooleanOperator op in BooleanOperator.AllOperators)
                {
                    int idx = currSubstring.IndexOf(op.Pattern);
                    if (idx != -1 && idx < bestIdx)
                    {
                        bestIdx = idx;
                        opUsed = op;
                    }
                }

                if (opUsed == null)
                {
                    // no more ops found
                    searches.Add(currSubstring);
                    break;
                };

                List<string> split = currSubstring.Split(opUsed.Pattern, 2).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

                if (split.Count() < 2)
                {
                    if (split.Count() == 1) searches.Add(split[0]);
                    break;
                }
                else
                {
                    searches.Add(split[0]);
                }

                ops.Add(opUsed);
                currSubstring = split[1];
            }

            return new Tuple<List<BooleanOperator>, List<string>>(ops, searches);
        }

        /// <summary>
        /// Gets all modules given a module type
        /// </summary>
        /// <param name="moduleType">The module type</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>All modules of the given type</returns>
        private async Task<List<Module>> GetAllModuleOfType(ModuleTypeEnum moduleType, CancellationToken cancellationToken = default)
        {
            switch (moduleType)
            {
                case ModuleTypeEnum.ApplicationIdentification:
                    return (await DBContextFactory.CreateInstance().ApplicationIdentifications.AsNoTracking().ToListAsync(cancellationToken)).Cast<Module>().ToList();
                case ModuleTypeEnum.Architecture:
                    return (await DBContextFactory.CreateInstance().Architectures.Include(a => a.Dependees).Include(a => a.Dependents).AsNoTracking().ToListAsync(cancellationToken)).Cast<Module>().ToList();
                case ModuleTypeEnum.Contacts:
                    return (await DBContextFactory.CreateInstance().Contacts.AsNoTracking().ToListAsync(cancellationToken)).Cast<Module>().ToList();
                case ModuleTypeEnum.Security:
                    return (await DBContextFactory.CreateInstance().Securities.Include(senv => senv.FortifyScans).AsNoTracking().ToListAsync(cancellationToken)).Cast<Module>().ToList();
                case ModuleTypeEnum.DatabaseEnvironment:
                    return (await DBContextFactory.CreateInstance().DatabaseEnvironments.Include(dbenv => dbenv.Databases).AsNoTracking().ToListAsync(cancellationToken)).Cast<Module>().ToList();
                case ModuleTypeEnum.ServerEnvironment:
                    return (await DBContextFactory.CreateInstance().ServerEnvironments.Include(senv => senv.Servers).AsNoTracking().ToListAsync(cancellationToken)).Cast<Module>().ToList();
                case ModuleTypeEnum.BASMOnboarding:
                    return (await DBContextFactory.CreateInstance().BASMOnboardings.AsNoTracking().ToListAsync(cancellationToken)).Cast<Module>().ToList();
                case ModuleTypeEnum.Report:
                    return (await DBContextFactory.CreateInstance().Reports.AsNoTracking().ToListAsync(cancellationToken)).Cast<Module>().ToList();
            }

            return new List<Module>();
        }

        /// <summary>
        /// Replaces all diacritic characters with the base character
        /// 
        /// Examples:
        /// français -> francais
        /// gâteau -> gateau
        /// 
        /// </summary>
        /// <param name="text">Input text</param>
        /// <returns>Input text with all diacritic characters replaced</returns>
        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Checks if the given text has diacritic characters.
        /// </summary>
        /// <param name="text">Input text</param>
        /// <returns>Return value indicates if text has diacritic characters</returns>
        private static bool DiacriticCheck(string text)
        {
            return Regex.IsMatch(text, @"[À-ÖØ-öø-įĴ-őŔ-žǍ-ǰǴ-ǵǸ-țȞ-ȟȤ-ȳɃɆ-ɏḀ-ẞƀ-ƓƗ-ƚƝ-ơƤ-ƥƫ-ưƲ-ƶẠ-ỿ]");
        }

        /// <summary>
        /// Returns a numerical heuristic on the match given the query and the value being compared
        /// Heuristic uses an exponential scale to give higher weightage for finer matches
        /// 
        /// A return value of 0 means no match
        /// 
        /// </summary>
        /// <param name="query">The search query</param>
        /// <param name="value">The value being compared</param>
        /// <returns></returns>
        private double GetMatchScore(string query, string value)
        {
            double score = 0;

            if (value.Length > 0)
            {
                // case insensitive word occurence: count the occurences of the string with case insensitivity
                // use a small scaling factor of 3
                score += ((double)Regex.Matches(value.ToLower(), Regex.Escape(query.ToLower())).Count / value.Length) * 3d;

                // case insensitive index relevance check: check if the query is closer to the starting index of the value with case insensitivity
                int idx = value.IndexOf(query, StringComparison.CurrentCultureIgnoreCase);
                score += idx != -1 ? (1d - ((double)idx / value.Length)) * 2d : 0;
            }

            // case insensitive word match: check if query is a word in value (not substring!) with case insensitivity
            score += Regex.IsMatch(value.ToLower(), $"(?<!\\S){Regex.Escape(query.ToLower())}(\\s|\\.)") ? 4 : 0;

            // case insensitive exact match: check if value is exact match of query with case insensitivity
            score += value.Equals(query, StringComparison.CurrentCultureIgnoreCase) ? 6 : 0;

            // case sensitive exact match: check if value is exact match of query with case sensitivity
            score += value.Equals(query) ? 8 : 0;


            if (DiacriticCheck(value) || DiacriticCheck(query))
            {
                // test the query without diacritics
                string cleanedQuery = RemoveDiacritics(query);
                string cleanedValue = RemoveDiacritics(value);

                if (value.Length > 0)
                {
                    // case insensitive word occurence: count the occurences of the string with case insensitivity
                    // use a small scaling factor of 3
                    score += ((double)Regex.Matches(cleanedValue.ToLower(), Regex.Escape(cleanedQuery.ToLower())).Count / cleanedValue.Length) * 3;

                    // case insensitive index relevance check: check if the query is closer to the starting index of the value with case insensitivity
                    int idx = cleanedValue.IndexOf(cleanedQuery, StringComparison.CurrentCultureIgnoreCase);
                    score += idx != -1 ? (1d - ((double)idx / value.Length)) * 2d : 0;
                }

                // case insensitive word match: check if query is a word in value (not substring!) with case insensitivity
                score += Regex.IsMatch(cleanedValue.ToLower(), $"(?<!\\S){Regex.Escape(cleanedQuery.ToLower())}(\\s|\\.)") ? 4 : 0;

                // case insensitive exact match: check if value is exact match of query with case insensitivity
                score += cleanedValue.Equals(cleanedQuery, StringComparison.CurrentCultureIgnoreCase) ? 6 : 0;

                // case sensitive exact match: check if value is exact match of query with case sensitivity
                score += cleanedValue.Equals(cleanedQuery) ? 8 : 0;
            }

            return score;
        }
    }
}

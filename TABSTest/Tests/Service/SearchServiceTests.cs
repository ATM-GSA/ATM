using TABS.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Eccc.Sali;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Diagnostics;
using System.Collections;

namespace Tests
{
    [TestClass]
    public class SearchServiceTests
    {
        TABSDBContext context;
        static SearchService searchService;
        static ApplicationService appService;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            DBContextFactory.SetConnectionString("");
            searchService = new SearchService();
            appService = new ApplicationService(null);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // Seed test db
            try
            {
                context = DBContextFactory.CreateInstance();
                DBInitializer.Initialize(context, false, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test DB initalization failed: " + ex);
                return;
            }
        }

        private async Task PrintModuleSearchResults(List<QuerySearchResult> results, CancellationToken cancellationToken)
        {
            int MAX_PRINT_COUNT = 100;
            int i = 0;

            async Task PrintResult(List<QuerySearchResult> results, string tabs = "")
            {
                if (results.Count == 0)
                {
                    return;
                }

                foreach (QuerySearchResult queryResult in results)
                {
                    if (i == MAX_PRINT_COUNT)
                    {
                        Console.WriteLine("<MAX PRINT COUNT REACHED>");
                        return;
                    }
                    i++;

                    if (queryResult is ModuleSearchResult result)
                    {
                        await result.LoadData(cancellationToken);
                        Console.WriteLine($"{tabs}\t[\"{result.Application.Identification.Name}\" : ({result.Module.GetModuleType().ToString()})]  <Score: {result.MatchScore}>");
                        foreach (ModuleProperty prop in result.Properties)
                        {
                            Console.WriteLine($"{tabs}\t\t- [{prop.Name}] {prop.Value}");
                        }
                        await PrintResult(result.ChildResults.Cast<QuerySearchResult>().ToList(), tabs: "\t");
                    }

                    if (tabs == "")
                        Console.WriteLine("");
                }
            }

            await PrintResult(results, tabs: "");

            Console.WriteLine("<END OF RESULTS>");
        }

        [TestMethod]
        public async Task Test_Search_All()
        {
            const string query = "project-0 || security";
            var watch = new Stopwatch();
            Console.WriteLine($"Starting search with query: \"{query}\"");
            watch.Start();

            CancellationToken cancellationToken = new();
            List<QuerySearchResult> results = await searchService.SearchAll(query, cancellationToken: cancellationToken);

            watch.Stop();
            Console.WriteLine($"Found {results.Count} results in {watch.ElapsedMilliseconds} ms");

            await PrintModuleSearchResults(results, cancellationToken);
        }

        [TestMethod]
        public async Task Test_Module_Field_Search()
        {
            const string query = "|";
            var watch = new Stopwatch();
            Console.WriteLine($"Starting search with query: \"{query}\"");
            watch.Start();

            CancellationToken cancellationToken = new();
            List<ModuleSearchResult> results = await searchService.SearchModuleFieldValues(query, cancellationToken);

            watch.Stop();
            Console.WriteLine($"Found {results.Count} results in {watch.ElapsedMilliseconds} ms");

            await PrintModuleSearchResults(results.Cast<QuerySearchResult>().ToList(), cancellationToken);
        }

        [TestMethod]
        public async Task Test_Module_Name_Search()
        {
            const string query = "|";
            var watch = new Stopwatch();
            Console.WriteLine($"Starting search with query: \"{query}\"");
            watch.Start();

            CancellationToken cancellationToken = new();
            List<ModuleSearchResult> results = await searchService.SearchModuleName(query, cancellationToken: cancellationToken);

            watch.Stop();
            Console.WriteLine($"Found {results.Count} results in {watch.ElapsedMilliseconds} ms");

            await PrintModuleSearchResults(results.Cast<QuerySearchResult>().ToList(), cancellationToken);
        }

        [TestMethod]
        public async Task Test_Module_Field_Name_Search()
        {
            const string query = "for";
            var watch = new Stopwatch();
            Console.WriteLine($"Starting search with query: \"{query}\"");
            watch.Start();

            CancellationToken cancellationToken = new();
            List<ModuleSearchResult> results = await searchService.SearchModuleFieldName(query, cancellationToken: cancellationToken);

            watch.Stop();
            Console.WriteLine($"Found {results.Count} results in {watch.ElapsedMilliseconds} ms");

            await PrintModuleSearchResults(results.Cast<QuerySearchResult>().ToList(), cancellationToken);
        }

        [TestMethod]
        public async Task Test_App_Search()
        {
            Application app = await appService.GetApplicationByID(3);

            if (app == null)
            {
                Console.WriteLine("App not found.");
                return;
            }

            const string query = "for";
            var watch = new Stopwatch();
            Console.WriteLine($"Starting search with query: \"{query}\"");
            watch.Start();

            CancellationToken cancellationToken = new();
            List<DetailedModuleSearchResult> results = searchService.SearchApplication(app, query, cancellationToken: cancellationToken);

            watch.Stop();
            Console.WriteLine($"Found {results.Count} results in {watch.ElapsedMilliseconds} ms");

            await PrintModuleSearchResults(results.Cast<QuerySearchResult>().ToList(), cancellationToken);
        }
    }
}

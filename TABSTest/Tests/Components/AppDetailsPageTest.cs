using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using Telerik.JustMock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using TABS.Data;
using TABS.Shared.ModuleTables;
using static Bunit.ComponentParameterFactory;
using AntDesign;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Localization;

namespace TABS.Pages.AppDetails
{
    [TestClass]
    public class AppDetailsPageTest
    {
        Bunit.TestContext ctx;
        IUserPreferenceService mockUserPreferenceService;
        IStringLocalizer mockLocalizer;
        IApplicationService mockApplicationService;
        ICrudService mockCrudService;
        IAuthService mockAuthService;
        IUserService mockUserService;


        [TestInitialize]
        public void Setup()
        {
            // Set up Bunit Test context
            ctx = new Bunit.TestContext();
            ctx.JSInterop.Mode = JSRuntimeMode.Loose;
            ctx.Services.AddAntDesign();
            ctx.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            var localizedString = new LocalizedString("Value", "Value");
            Mock.Arrange(() => mockLocalizer["Value"]).Returns(localizedString);

            mockLocalizer = Mock.Create<IStringLocalizer>();
            mockUserPreferenceService = Mock.Create<IUserPreferenceService>();
            mockCrudService = Mock.Create<ICrudService>();
            mockApplicationService = Mock.Create<IApplicationService>();
            mockAuthService = Mock.Create<IAuthService>();
            mockUserService = Mock.Create<IUserService>();

            Mock.Arrange(() => mockUserPreferenceService.Initialize(Arg.IsAny<String>())).DoNothing();
        }

        [TestCleanup]
        public void TearDown()
        {
            ctx?.Dispose();
        }

        //[TestMethod]
        // The Featured section should be empty. The "There are no featured modules." placeholder should be showing
        //public void TestNoFeatured()
        //{
        //    // Data setup
        //    Preferences testPrefs = new()
        //    {
        //        favouriteApplications = new List<int>(),
        //        language = "en-US",
        //        gridView = false,
        //        theme = "Light"
        //    };
        //    // list for Featured modules
        //    List<Dictionary<string, object>> testMods = new List<Dictionary<string, object>>();

        //    Mock.Arrange(() => mockUserPreferenceService.SaveUserPreferences())
        //        .DoNothing();

        //    ctx.Services.AddSingleton<IUserPreferenceService>(mockUserPreferenceService);

        //    // The next sibling child of the h2 should be a placeholder-content
        //    var cut = ctx.RenderComponent<ModuleSection>(parameters => parameters
        //        .Add(p => p.IsFeature, true) // Featured modules
        //        .Add(p => p.Modules, testMods)
        //        .Add(p => p.UserPreference, testPrefs));
        //    var title = cut.Find(".featured-title");
        //    Assert.IsTrue(title.NextElementSibling.ClassList.Contains("placeholder-content"));
        //}

        //[TestMethod]
        //// The All Modules section should be in list view
        //public void TestListView()
        //{
        //    // Data setup
        //    Preferences testPrefs = new()
        //    {
        //        favouriteApplications = new List<int>(),
        //        language = "en-US",
        //        gridView = false,
        //        theme = "Light"
        //    };
        //    List<Dictionary<string, object>> testMods = new List<Dictionary<string, object>>();
        //    testMods.Add(new Dictionary<string, object>() {
        //            { "Name", "Test Mod" },
        //            { "RelativeURL", "TestMod" },
        //            { "LastUpdated", new DateTime() },
        //            { "StatusFlags", 0 },
        //            { "IsPinned", false },
        //            { "Fields", new List<string>() }});

        //    Mock.Arrange(() => mockUserPreferenceService.SaveUserPreferences())
        //        .DoNothing();

        //    ctx.Services.AddSingleton<IUserPreferenceService>(mockUserPreferenceService);

        //    var cut = ctx.RenderComponent<ModuleSection>(parameters => parameters
        //        .Add(p => p.IsFeature, false) // All Modules section
        //        .Add(p => p.Modules, testMods)
        //        .Add(p => p.UserPreference, testPrefs));
        //    var heading = cut.Find(".all-mod-body");
        //    Assert.IsTrue(heading.NextElementSibling.TagName.Equals("ModuleList"));
        //}


        //[TestMethod]
        // The All Modules section should be in grid view
        //public void TestGridView()
        //{
        //    // Additional setup
        //    Preferences testPreferences = new()
        //    {
        //        favouriteApplications = new List<int>() { 1, 2 },
        //        language = "en-US",
        //        gridView = true, // for grid view
        //        theme = "Light"
        //    };
        //    Mock.Arrange(() => mockUserPreferenceService.GetPreferences())
        //        .Returns(testPreferences);
        //}

        [TestMethod]
        public void TestModuleCardBasic()
        {
            // Data setup
            var testFields = new List<string>() { "test field 1", "test field 2", "test field 3", "test field 4", "test field 5", "test field 6" };
            Dictionary<string, object> testMod = new Dictionary<string, object>() {
                { "Name", "Test Mod" },
                { "RelativeURL", "TestMod" },
                { "LastUpdated", new DateTime() },
                { "StatusFlags", 0 }, // 0 = "Up To Date" status
                { "IsPinned", false },
                { "Fields", testFields }
            };

            // Check to see that the data on the Card is correct
            var cut = ctx.RenderComponent<ModuleCard>(p => p.Add(p => p.Module, testMod));

            var title = cut.Find(".card-title-text");
            Assert.IsTrue(title.TextContent.Equals("Test Mod"));
            var tags = cut.Find(".card-tags");
            Assert.IsTrue(tags.ChildElementCount.Equals(1));
            //Assert.IsTrue(tags.FirstElementChild.TextContent.Equals("Up To Date")); // leaving this one out for now since the text itself might change
            var fields = cut.FindAll(".card-field-text");
            Assert.IsTrue(fields.Count.Equals(5));
            foreach (var field in fields)
            {
                Assert.IsTrue(testFields.Contains(field.TextContent));
            }
            var fieldList = cut.Find(".fields-list");
            Assert.IsNotNull(fieldList.NextElementSibling); // has the +x other fields text
        }

        [TestMethod]
        public void TestModuleListBasic()
        {
            // Data setup
            var testMods = new List<Dictionary<string, object>>() {
                new Dictionary<string, object>() {
                    { "Name", "Test Mod" },
                    { "RelativeURL", "TestMod" },
                    { "LastUpdated", new DateTime() },
                    { "StatusFlags", 0 }, // 0 = "Up To Date" status
                    { "IsPinned", false },
                    { "Fields", new List<string>() } // no fields
                }
            };

            // Check to see that the data on the List is correct
            var cut = ctx.RenderComponent<ModuleList>(p => p.Add(p => p.Modules, testMods));

            var items = cut.FindAll(".list-item");
            Assert.IsTrue(items.Count.Equals(1));
            var title = cut.Find(".list-item-title");
            Assert.IsTrue(title.TextContent.Equals("Test Mod"));
            var tags = cut.Find(".list-item-last-updated").NextElementSibling;
            Assert.IsTrue(tags.ChildElementCount.Equals(1));
        }

    }
}

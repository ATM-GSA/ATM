using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using Telerik.JustMock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using TABS.Data;
using TABS.Shared.Layout;
using static Bunit.ComponentParameterFactory;
using AntDesign;

namespace TABS.Shared
{
    [TestClass]
    public class TabsLayoutTest
    {
        [TestMethod]
        public async Task TestSideBarLaunchesProperly()
        {
            // Set up Bunit Test context
            using var ctx = new Bunit.TestContext();

            // Set up required services for TabsLayout component

            // Mock auth service, make it return the string "Mock User ID" when the GetUserSID() is called
            var mockAuthService = Mock.Create<IAuthService>();
            Mock.Arrange(() => mockAuthService.GetUserSID())
                .Returns(Task.FromResult("Mock User ID"));

            // Mock user service, make it return fake testUser I created when calling GetUserByAdID
            var mockUserService = Mock.Create<IUserService>();
            User testUser = new User();
            testUser.Approved = true;
            testUser.IsDeactivated = false;
            Mock.Arrange(() => mockUserService.GetUserByAdID(Arg.IsAny<String>()))
                .Returns(Task.FromResult<User>(testUser));

            // Mock user preference service,
            // make the Initialize function do nothing when it is called
            // and make the GetPreferences function return testPreference object I created
            var mockUserPreferenceService = Mock.Create<IUserPreferenceService>();
            Mock.Arrange(() => mockUserPreferenceService.Initialize(Arg.IsAny<String>()))
                .DoNothing();
            Preferences testPreferences = new()
            {
                favouriteApplications = new List<int>() { 1, 2 },
                language = "en-US",
                gridView = false,
                theme = "Light"
            };
            Mock.Arrange(() => mockUserPreferenceService.GetPreferences())
                .Returns(testPreferences);

            // Mock application service,
            // make the GetApplicationByID function return application1 object when argument is the integer 1
            // and return application2 object when argument is the integer 2
            var mockApplicationService = Mock.Create<IApplicationService>();
            Application application1 = new();
            application1.ShortID = "1";
            application1.Identification = new();
            application1.Identification.Name = "Test app 1";
            Application application2 = new();
            application2.ShortID = "2";
            application2.Identification = new();
            application2.Identification.Name = "Test app 2";
            Mock.Arrange(() => mockApplicationService.GetApplicationByID(Arg.Matches<int>(x => x == 1), Arg.IsNull<List<ModuleTypeEnum>>()))
                .Returns(Task.FromResult(application1));
            Mock.Arrange(() => mockApplicationService.GetApplicationByID(Arg.Matches<int>(x => x == 2), Arg.IsNull<List<ModuleTypeEnum>>()))
                .Returns(Task.FromResult(application2));


            // Register all services in our test context
            ctx.Services.AddSingleton<IAuthService>(mockAuthService);
            ctx.Services.AddSingleton<IUserService>(mockUserService);
            ctx.Services.AddSingleton<IUserPreferenceService>(mockUserPreferenceService);
            ctx.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            ctx.Services.AddAntDesign();

            // Set authorization state to authenticated and authorized
            var authContext = ctx.AddTestAuthorization();
            authContext.SetAuthorized("TEST USER");

            // Create expected side bar 
            bool textChangedEventTriggered = false;
            var expectedSideBar = ctx.RenderComponent<TabsSideBar>(
                (nameof(TabsSideBar.favouriteApplications), new List<Application>() { application1, application2 }), 
                (nameof(TabsSideBar.siderTheme), SiderTheme.Light),
                (nameof(TabsSideBar.isAdmin), true),
                EventCallback<bool>(nameof(TabsSideBar.collapsedChanged),
                (e) =>
                {
                    textChangedEventTriggered = e;
                })
            );

            await Task.Delay(5000);

            // Render TabsLayout with the services and mock services previously registered
            var cut = ctx.RenderComponent<TabsLayout>();

            await Task.Delay(5000);
            // Find the sidebar in TabsLayout
            var actualSideBar = cut.FindComponent<TabsSideBar>(); // find the component

            await Task.Delay(5000);

            // Verify that the actual sidebar is identical to our expected sidebar
            actualSideBar.MarkupMatches(expectedSideBar.Markup);
            
        }
    }
}



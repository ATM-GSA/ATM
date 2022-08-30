using TABS.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Eccc.Sali;
using Microsoft.Extensions.Configuration;

namespace Tests
{
    [TestClass]
    public class ApplicationServiceTests
    {
        TABSDBContext context;
        static ApplicationService appService;
        static CrudService crudService;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            DBContextFactory.SetConnectionString("");
            appService = new ApplicationService(null);
            crudService = new CrudService(null);
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

        [TestMethod]
        public async Task Get_Application_By_ID()
        {
            Application app = await appService.GetApplicationByID(1);

            Assert.IsNotNull(app);

            Assert.IsNotNull(app.Architecture);
            Assert.IsNotNull(app.BASMOnboarding);
            Assert.IsNotNull(app.Contact);
            Assert.IsNotNull(app.DatabaseEnvironment);
            Assert.IsTrue(app.DatabaseEnvironment.Databases.Count > 1);
            Assert.IsNotNull(app.Report);
            Assert.IsNotNull(app.Security);
            Assert.IsTrue(app.Security.FortifyScans.Count > 1);
            Assert.IsNotNull(app.ServerEnvironment);
            Assert.IsTrue(app.ServerEnvironment.Servers.Count > 1);

            Assert.AreEqual(app.GetApplicationProperties().availableModules.Count, 7);
            Assert.AreEqual(app.GetApplicationProperties().featuredModules.Count, 1);
        }

        [TestMethod]
        public async Task Get_Application_By_ID_Include()
        {
            Application app = await appService.GetApplicationByID(1, new List<ModuleTypeEnum> { ModuleTypeEnum.Architecture });

            Assert.IsNotNull(app);

            Assert.IsNotNull(app.Architecture);
            Assert.IsNotNull(app.CreateByUser);
            Assert.IsNull(app.BASMOnboarding);
            Assert.IsNull(app.Contact);
            Assert.IsNull(app.DatabaseEnvironment);
            Assert.IsNull(app.Report);
            Assert.IsNull(app.Security);
            Assert.IsNull(app.ServerEnvironment);
        }

        [TestMethod]
        public async Task Get_Application_By_ID_Get_Module_Properties()
        {
            Application app = await appService.GetApplicationByID(1);

            Assert.IsNotNull(app);
            Assert.IsNotNull(app.Report);
            Assert.IsNotNull(app.Security);
            Assert.IsNotNull(app.DatabaseEnvironment);

            Dictionary<string, ModuleProperty> reportProperties = app.Report.GetProperties();
            Assert.AreEqual("Accessibility", reportProperties["Accessibility"].Name);
            Assert.AreEqual("https://ec.gc.ca/reports/project-0/reports/accessibility/report.pdf", reportProperties["Accessibility"].Value);
            Assert.AreEqual(typeof(string), reportProperties["Performance"].Type);

            Dictionary<string, ModuleProperty> securityProperties = app.Security.GetProperties();
            Assert.AreEqual(typeof(bool), securityProperties["WebconfigEncrpted"].Type);
            Assert.IsTrue(securityProperties["FortifyScans"].Type.IsGenericType && (securityProperties["FortifyScans"].Type.GetGenericTypeDefinition() == typeof(List<>)));
            Assert.AreEqual(typeof(FortifyScan.ScanTypeLookUp), ((List<FortifyScan>)securityProperties["FortifyScans"].Value)[0].GetProperties()["ScanType"].Type);
            Assert.AreEqual(FortifyScan.ScanTypeLookUp.StaticScan, ((List<FortifyScan>)securityProperties["FortifyScans"].Value)[0].GetProperties()["ScanType"].Value);
        }

        [TestMethod]
        public async Task Get_Application_By_ID_Not_Found()
        {
            Application app = await appService.GetApplicationByID(-1);
            Assert.IsNull(app);
        }

        [TestMethod]
        public async Task Get_Application_By_ShortID()
        {
            // "abcdefghijkl" is a hard-coded short-id in the test db
            Application app = await appService.GetApplicationByShortID("abcdefghijkl");

            Assert.IsNotNull(app);

            Assert.IsNotNull(app.Architecture);
            Assert.IsNotNull(app.BASMOnboarding);
            Assert.IsNotNull(app.Contact);
            Assert.IsNotNull(app.DatabaseEnvironment);
            Assert.IsTrue(app.DatabaseEnvironment.Databases.Count > 1);
            Assert.IsNotNull(app.Report);
            Assert.IsNotNull(app.Security);
            Assert.IsTrue(app.Security.FortifyScans.Count > 1);
            Assert.IsNotNull(app.ServerEnvironment);
            Assert.IsTrue(app.ServerEnvironment.Servers.Count > 1);
        }

        [TestMethod]
        public async Task Get_Application_By_ShortID_Not_Found()
        {
            Application app = await appService.GetApplicationByShortID("NOTEXISTENTSHORTIDdrjsldsdtl");
            Assert.IsNull(app);
        }

        [TestMethod]
        public async Task Delete_Application()
        {
            // ******************************************************* //
            // --                 TEST WITH CAUTION!                -- //
            // --               DATA WILL BE DELETED!               -- //
            // ******************************************************* //

            Application app = await appService.GetApplicationByID(5);
            Assert.IsNotNull(app);

            bool result = await appService.DeleteAsync(app, true);
            Assert.IsTrue(result);

            Application deletedApp = await appService.GetApplicationByID(5);
            Assert.IsNull(deletedApp);

            List<User> users = await crudService.GetDataAsync<User>();
            bool deletedFromFavorite = true;
            foreach (User user in users)
            {
                Preferences preferences = user.GetPreferences();
                deletedFromFavorite = deletedFromFavorite && !preferences.favouriteApplications.Contains(5); 
            }
            Assert.IsTrue(deletedFromFavorite);
        }

        [TestMethod]
        public async Task Delete_Fortify_Scan_Module()
        {
            // ******************************************************* //
            // --                 TEST WITH CAUTION!                -- //
            // --               DATA WILL BE DELETED!               -- //
            // ******************************************************* //

            Application app = await appService.GetApplicationByID(1);
            int id = app.Security.FortifyScans[3].FortifyScanId;
            await crudService.DeleteAsync(app.Security.FortifyScans[3]);
            await crudService.UpdateAsync(app);

            app = await appService.GetApplicationByID(1);

            FortifyScan fs = await crudService.GetRecordAsync<FortifyScan>(id);
            Assert.IsNull(fs);
            Assert.AreEqual(app.Security.FortifyScans.Count, 3);
        }

        [TestMethod]
        public async Task Soft_Delete_Application()
        {
            // ******************************************************* //
            // --                 TEST WITH CAUTION!                -- //
            // --               DATA WILL BE DELETED!               -- //
            // ******************************************************* //
            Application app = await appService.GetApplicationByID(1);
            
            await appService.DeleteAsync(app, false);
            app = await appService.GetApplicationByID(1);

            Assert.IsNotNull(app);
            Assert.IsTrue(app.IsDeleted);
        }

        [TestMethod]
        public async Task Hard_Delete_Application()
        {
            // ******************************************************* //
            // --                 TEST WITH CAUTION!                -- //
            // --               DATA WILL BE DELETED!               -- //
            // ******************************************************* //
            Application app = await appService.GetApplicationByID(1);

            await appService.DeleteAsync(app, true);
            app = await appService.GetApplicationByID(1);

            Assert.IsNull(app);
        }

        [TestMethod]
        public void Check_Duplicate_App_Name()
        {
            // duplicate app name
            Assert.IsTrue(appService.IsDuplicateName("Project - 0", ignoreId: -1));
        }

        [TestMethod]
        public void Check_Duplicate_App_Name_Case_Insensitive()
        {
            // duplicate app name
            Assert.IsTrue(appService.IsDuplicateName("project - 0", ignoreId: -1));
        }

        [TestMethod]
        public void Check_Original_App_Name()
        {
            // original app name
            Assert.IsFalse(appService.IsDuplicateName("Project - 312321", ignoreId: -1));
        }

        [TestMethod]
        public void Check_Duplicate_App_Name_Exclude_App()
        {
            // duplicate app name, but ignore app with id 1
            Assert.IsFalse(appService.IsDuplicateName("Project - 0", ignoreId: 1));
            Assert.IsFalse(appService.IsDuplicateName("Project - 34423", ignoreId: -1));
        }
    }
}

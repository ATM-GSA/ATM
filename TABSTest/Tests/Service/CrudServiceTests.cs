using TABS.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class CrudServiceTests
    {
        TABSDBContext context;
        static CrudService crudService;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            DBContextFactory.SetConnectionString("");
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
        public async Task Get_Application_With_ID()
        {
            Application app = await crudService.GetRecordAsync<Application>(1);
            Assert.IsNotNull(app);
        }

        [TestMethod]
        public async Task Get_Multiple_Applications()
        {
            List<Application> apps = await crudService.GetMultipleRecordsAsync<Application>(new List<int>(new int[] {1, 2, 3, 4, 5, 6}));
            Assert.AreEqual(5, apps.Count);

            apps = await crudService.GetMultipleRecordsAsync<Application>(new List<int>(new int[] { -12 }));
            Assert.AreEqual(0, apps.Count);
        }

        [TestMethod]
        public async Task Get_All_Applications()
        {
            List<Application> apps = await crudService.GetDataAsync<Application>();
            Assert.IsTrue(apps.Count == 5);
        }

        [TestMethod]
        public async Task Get_Application_Architecture()
        {
            Application app = await crudService.GetRecordAsync<Application>(1);
            Architecture arch = await crudService.GetRecordAsync<Architecture>(app.ApplicationID);
            Assert.IsNotNull(arch);
        }
    }
}

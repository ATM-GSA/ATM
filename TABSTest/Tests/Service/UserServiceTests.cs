using TABS.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Eccc.Sali;

namespace Tests
{
    [TestClass]
    public class UserServiceTests
    {
        TABSDBContext context;
        static UserService userService;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            DBContextFactory.SetConnectionString("");
            userService = new UserService(null);
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
        public async Task Get_User_By_ADID()
        {
            // assumes you are in the user database
            string adId = WindowsIdentity.GetCurrent().User.Value;
            User user = await userService.GetUserByAdID(adId);

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public async Task Get_User_Preferences()
        {
            // assumes you are in the user database
            string adId = WindowsIdentity.GetCurrent().User.Value;
            User user = await userService.GetUserByAdID(adId);

            Assert.IsNotNull(user);

            Preferences preferences = user.GetPreferences();
            Assert.IsNotNull(preferences);
            Assert.AreEqual("en-US", preferences.language);
        }

        [TestMethod]
        public async Task Search_User_By_Name_Contains()
        {
            // assumes Matthew is in the database
            List<User> users = await userService.SearchUsersByName("a");
            Assert.IsNotNull(users);
            Assert.IsTrue(users.Count > 0);
        }

        [TestMethod]
        public async Task Search_User_By_First_Name()
        {
            // assumes Matthew is in the database
            List<User> users = await userService.SearchUsersByName("matt");
            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count);
            Assert.AreEqual("Matt Szczerba", users[0].Name);

        }

        [TestMethod]
        public async Task Search_User_By_Last_Name()
        {
            // assumes Matthew is in the database
            List<User> users = await userService.SearchUsersByName("szczerba");
            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count);
            Assert.AreEqual("Matt Szczerba", users[0].Name);
        }

        [TestMethod]
        public async Task Search_User_By_Name_Not_Found()
        {
            // assumes Matthew is in the database
            List<User> users = await userService.SearchUsersByName("thisnamedoesnotexist,trustme");
            Assert.AreEqual(0, users.Count);
        }
    }
}

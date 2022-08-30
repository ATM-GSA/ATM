using TABS.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using AngleSharp.Html.Dom;

namespace TABS.Pages.Admin
{
    [TestClass]
    public class AdminLandingTest
    {
        TABSDBContext context;
        CrudService crudService;
        Bunit.TestContext renderContext;

        [TestInitialize]
        public void TestInitialize()
        {
            DBContextFactory.SetConnectionString("");
            context = DBContextFactory.CreateInstance();

            // Seed test db
            try
            {
                DBInitializer.Initialize(context, false, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test DB initalization failed: " + ex);
                return;
            }

            crudService = new CrudService(null);

            renderContext = new Bunit.TestContext();
            renderContext.Services.AddSingleton(crudService);
        }

        [TestMethod]
        public async Task Cards_Rendered_Test()
        {
            var renderedComponent = renderContext.RenderComponent<AdminLanding>();

            await Task.Delay(1000);

            // Stubbed test. I'll complete this when service interfaces are implemented
        }
    }
}

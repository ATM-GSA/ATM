using TABS.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using AngleSharp.Html.Dom;

namespace TABS.Pages.Applications
{
    [TestClass]
    public class ApplicationsPageTest
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
        public async Task TestApplicationsPage()
        {
            var renderedComponent = renderContext.RenderComponent<ApplicationsTable>();

            // Wait for the page to load because there are some async DB calls
            // Can also use this delay to test responsiveness as well (i.e. 5 seconds max for the page to load)
            await Task.Delay(5000);

            // Find the table body and go test the rendering of the first project
            var tableBody = renderedComponent.Find("tbody");
            IHtmlTableRowElement row = (IHtmlTableRowElement)tableBody.FirstChild;

            // Assert if the test data is correct
            Assert.IsTrue(row.Cells[0].TextContent.Equals("0"));
            Assert.IsTrue(row.Cells[1].TextContent.Equals("Project - 0"));
            Assert.IsTrue(row.Cells[2].TextContent.Equals("BeSD Project 2020"));
            Assert.IsTrue(row.Cells[3].TextContent.Equals("Active"));
            Assert.IsTrue(row.Cells[4].TextContent.Equals("Internal"));
            Assert.IsTrue(row.Cells[5].TextContent.Contains("Disabled"));
            Assert.IsTrue(row.Cells[6].TextContent.Equals("https://ec.gc.ca/project-0"));
        }
    }
}

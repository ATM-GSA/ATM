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

namespace TABS.Shared.ModuleTables
{
    [TestClass]
    public class DataTableTest
    {
        Bunit.TestContext ctx;
        // IStringLocalizer mockLocalizer;

        [TestInitialize]
        public void Setup()
        {
            // Set up Bunit Test context
            ctx = new Bunit.TestContext();
            ctx.JSInterop.Mode = JSRuntimeMode.Loose;

            ctx.JSInterop.Setup<AntDesign.JsInterop.DomRect>(JSInteropConstants.GetBoundingClientRect, _ => true)
                   .SetResult(new AntDesign.JsInterop.DomRect());

            ctx.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            ctx.Services.AddAntDesign();
        }

        [TestCleanup]
        public void TearDown()
        {
            ctx?.Dispose();
        }

        [TestMethod]
        public void TestRendersEmptyCorrectly()
        {
            // Additional setup
            var mockLocalizer = Mock.Create<IStringLocalizer>();
            var localizedString = new LocalizedString("Value", "Value");
            Mock.Arrange(() => mockLocalizer["Value"]).Returns(localizedString);

            ctx.Services.AddSingleton<IStringLocalizer>(mockLocalizer);
            var cut = ctx.RenderComponent<DataTable>();

            var tableHeader = cut.Find("thead");
            IHtmlTableRowElement row = (IHtmlTableRowElement)tableHeader.FirstChild;

            Assert.IsTrue(row.Cells.Length.Equals(3));
            Assert.IsTrue(row.Cells[0].TextContent.Equals(""));
            //Assert.IsTrue(row.Cells[1].TextContent.Equals(mockLocalizer["Field"]));
            Assert.IsTrue(row.Cells[2].TextContent.Equals("Value"));
        }

        [TestMethod]
        public async Task TestShortTextInputWithSave()
        {
            var mockLocalizer = Mock.Create<IStringLocalizer>();
            ctx.Services.AddSingleton<IStringLocalizer>(mockLocalizer);

            DataDisplay shortTextField = new DataDisplay()
            {
                Field = new FieldDisplay() { Name = "Title", LocalizationKey = "Title", Type = FieldDisplay.InputType.ShortText, Description = "Testing 123" },
                Value = new ValueDisplay("")
            };

            List<DataDisplay> testDisplayConfig = new List<DataDisplay>()
            {
                shortTextField
            };

            var cut = ctx.RenderComponent<DataTable>(parameters => parameters
                .Add(p => p.ModuleData, testDisplayConfig)
                .Add(p => p.ModuleDataChanged, (updatedConfig) => testDisplayConfig = updatedConfig)
                .Add(p => p.IsLoading, false)
                .Add(p => p.IsInEditMode, true)
            );

            string expected = "New Non-empty Title";

            Assert.AreEqual(1, cut.FindComponents<AntDesign.Input<string>>().Count);
            var inputCmp = cut.FindComponent<AntDesign.Input<string>>(); //This finds the input component
            // inputCmp.SetParametersAndRender(("DebounceMilliseconds", 0)); <--- Not necessary so long as we wait, i.e. Task.Delay

            inputCmp.Find("input").Input(expected); //Enter data
            inputCmp.Find("input").KeyUp(expected);
            await Task.Delay(1000); // Wait for the debounce to finish and set the input

            Assert.AreEqual("", testDisplayConfig[0].Value.Value); // This should not be updated just from typing in the input field

            await cut.Instance.saveModuleData(); 
            Assert.AreEqual(expected, testDisplayConfig[0].Value.Value);

            // If this test passes, it means we can save inputted text into the testDisplayConfig object that was passed as the ModuleData param
            // via the specific saveModuleData method

        }

        [TestMethod]
        public async Task TestLongTextInputWithSave()
        {
            var mockLocalizer = Mock.Create<IStringLocalizer>();
            ctx.Services.AddSingleton<IStringLocalizer>(mockLocalizer);

            DataDisplay longTextField = new DataDisplay()
            {
                Field = new FieldDisplay() { Name = "LongTextField", LocalizationKey = "LongTextField", Type = FieldDisplay.InputType.LongText, Description = "Testing 123" },
                Value = new ValueDisplay("")
            };

            List<DataDisplay> testDisplayConfig = new List<DataDisplay>()
            {
                longTextField
            };

            var cut = ctx.RenderComponent<DataTable>(parameters => parameters
                .Add(p => p.ModuleData, testDisplayConfig)
                .Add(p => p.ModuleDataChanged, (updatedConfig) => testDisplayConfig = updatedConfig)
                .Add(p => p.IsLoading, false)
                .Add(p => p.IsInEditMode, true)
            );

            bool modified = await cut.Instance.saveModuleData();
            Assert.IsFalse(modified);

            string expected = "New long content that is super long long long! \n";

            Assert.AreEqual(1, cut.FindComponents<AntDesign.TextArea>().Count);
            var textAreaCmp = cut.FindComponent<AntDesign.TextArea>(); //This finds the textarea component

            textAreaCmp.Find("textarea").Input(expected); //Enter data
            textAreaCmp.Find("textarea").KeyUp(expected);
            await Task.Delay(1000); // Wait for the debounce to finish and set the input

            Assert.AreEqual("", testDisplayConfig[0].Value.Value); // This should not be updated just from typing in the input field

            modified = await cut.Instance.saveModuleData();
            Assert.IsTrue(modified); 
            Assert.AreEqual(expected, testDisplayConfig[0].Value.Value);

            // If this test passes, it means we can save inputted text into the testDisplayConfig object that was passed as the ModuleData param
            // via the specific saveModuleData method

        }

        [TestMethod]
        public async Task TestBooleanInputWithSave()
        {
            var mockLocalizer = Mock.Create<IStringLocalizer>();
            ctx.Services.AddSingleton<IStringLocalizer>(mockLocalizer);

            DataDisplay testField = new DataDisplay()
            {
                Field = new FieldDisplay() { Name = "Test", LocalizationKey = "Test", Type = FieldDisplay.InputType.Boolean, Description = "Testing 123" },
                Value = new ValueDisplay("False")
            };

            List<DataDisplay> testDisplayConfig = new List<DataDisplay>()
            {
                testField
            };

            var cut = ctx.RenderComponent<DataTable>(parameters => parameters
                .Add(p => p.ModuleData, testDisplayConfig)
                .Add(p => p.ModuleDataChanged, (updatedConfig) => testDisplayConfig = updatedConfig)
                .Add(p => p.IsLoading, false)
                .Add(p => p.IsInEditMode, true)
            );

            bool modified = await cut.Instance.saveModuleData();
            Assert.IsFalse(modified);

            Assert.AreEqual(1, cut.FindComponents<AntDesign.SimpleSelect>().Count);
            var tableCmp = cut.FindComponent<AntDesign.Table<DataDisplay>>(); //This finds the table component
            var selectCmp = cut.FindComponent<AntDesign.SimpleSelect>(); //This finds the select component

            Assert.IsNotNull(selectCmp.Find("input"));
            Assert.AreEqual("False", selectCmp.Instance.Value);

            var dropdownCmp = cut.Find(".ant-select-dropdown");
            Assert.IsNotNull(dropdownCmp);

            var dropdownItems = cut.FindAll(".ant-select-item-option-state");
            Assert.AreEqual(2, dropdownItems.Count);

            dropdownItems[1].Click(); // Selecting the option "True" from the dropdown

            Assert.AreEqual("True", selectCmp.Instance.Value);

            Assert.AreEqual("False", testDisplayConfig[0].Value.Value); // This should not be updated just from typing in the input field

            modified = await cut.Instance.saveModuleData();
            Assert.IsTrue(modified);
            Assert.AreEqual("True", testDisplayConfig[0].Value.Value);
        }

    }
}

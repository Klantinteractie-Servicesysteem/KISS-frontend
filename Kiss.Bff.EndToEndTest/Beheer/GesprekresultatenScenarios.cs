using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using Kiss.Bff.EndToEndTest.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;

namespace Kiss.Bff.EndToEndTest.Beheer
{
    [TestClass]
    public class Gesprekresultaten : KissPlaywrightTest
    {
        private string baseName;
        private string updatedName;

        [TestInitialize]
        public async Task TestInit()
        {
            baseName = $"Automation Gespreksresultaten {DateTime.Now:yyyyMMddHHmmss}";
            updatedName = $"Automation Gesprekresultaten Updated {DateTime.Now:yyyyMMddHHmmss}";

            // Cleanup old data from previous runs
            await DeleteAllTestGespreksresultaten();
        }

        [TestCleanup]
        public async Task TestClean()
        {
            // Cleanup new data from this run
            await DeleteAllTestGespreksresultaten();
        }

        [TestMethod("1. Navigation to Gesprekresultaten page")]
        public async Task NavigationGespreksresultaten()
        {
            await Step("Given the user navigates to the Beheer tab");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();

            await Step("When the user clicks on Gespreksresultaten tab");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();

            await Step("Then list of Gespreksresultaten are displayed");

            await Expect(Page.GetByRole(AriaRole.Listitem)).ToBeVisibleAsync();
        }

        [TestMethod("2. Adding a Gespreksresultaat")]
        public async Task AddGesprekresultaten()
        {
            await AddGespreksresultaatHelper(baseName);
            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = baseName })).ToBeVisibleAsync();
        }

        [TestMethod("3. Editing an existing Gesprekresultaten")]
        public async Task EditGespreksresultaat()
        {
            await AddGespreksresultaatHelper(baseName);

            await Step($"When the user clicks on the list item named '{baseName}'");
            await Page.GetByRole(AriaRole.Link, new() { Name = baseName }).ClickAsync();

            await Step($"And the user updates the title to '{updatedName}'");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(updatedName);

            await Step("And the user clicks on Opslaan");
            await Page.GetOpslaanButton().ClickAsync();

            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var updatedItem = Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = updatedName });
            await updatedItem.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
            await Expect(updatedItem).ToBeVisibleAsync();
        }

        [TestMethod("4. Deleting a Gespreksresultaten")]
        public async Task Deletegespreksresultaat()
        {
            await AddGespreksresultaatHelper(baseName);
            await DeleteGespreksresultaatHelper(baseName);

            var isStillVisible = await Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = baseName }).IsVisibleAsync();
            Assert.IsFalse(isStillVisible, "Gespreksresultaat should be deleted but is still visible.");
        }

        // ================= Helper methods =================

        private async Task AddGespreksresultaatHelper(string name)
        {
            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();
            await Page.WaitForSelectorAsync("li", new() { State = WaitForSelectorState.Visible });

            await Page.GetByRole(AriaRole.Button, new() { Name = "toevoegen" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(name);
            await Page.GetOpslaanButton().ClickAsync();

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = name })).ToBeVisibleAsync();
        }

        private async Task DeleteGespreksresultaatHelper(string name)
        {
            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();
            await Page.WaitForSelectorAsync("li", new() { State = WaitForSelectorState.Visible });

            var deleteButtonLocator = Page.GetByRole(AriaRole.Listitem)
                .Filter(new() { HasText = name }).GetByRole(AriaRole.Button);

            using (var _ = Page.AcceptAllDialogs())
            {
                await deleteButtonLocator.First.ClickAsync();
            }

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = name })).ToHaveCountAsync(0);
        }

        /// Deletes all test data items matching Automation Gespreksresultaten* or Automation Gesprekresultaten Updated*
        /// Handles pagination / lazy loading by scrolling and refreshing until none are left.

        private async Task DeleteAllTestGespreksresultaten()
        {
            string[] patterns = { "Automation Gesprek" };

            foreach (var pattern in patterns)
            {
                bool found = true;
                while (found)
                {
                    await Page.GotoAsync("/");
                    await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
                    await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();
                    await Page.WaitForSelectorAsync("li", new() { State = WaitForSelectorState.Visible });

                    // Keep scrolling until no new items
                    int prevCount, newCount;
                    do
                    {
                        prevCount = await Page.GetByRole(AriaRole.Listitem).CountAsync();
                        await Page.Mouse.WheelAsync(0, 5000);
                        await Page.WaitForTimeoutAsync(500);
                        newCount = await Page.GetByRole(AriaRole.Listitem).CountAsync();
                    } while (newCount > prevCount);

                    var matchingItems = Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = pattern });
                    int count = await matchingItems.CountAsync();

                    if (count == 0)
                    {
                        found = false;
                    }
                    else
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var deleteButton = matchingItems.Nth(0).GetByRole(AriaRole.Button);
                            using (var _ = Page.AcceptAllDialogs())
                            {
                                await deleteButton.ClickAsync();
                            }
                            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                        }
                    }
                }
            }
        }

    }
}

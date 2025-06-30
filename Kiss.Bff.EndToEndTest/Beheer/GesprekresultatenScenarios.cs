using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;
using Kiss.Bff.EndToEndTest.Helpers;

namespace Kiss.Bff.EndToEndTest.Beheer
{
    [TestClass]
    public class Gesprekresultaten : KissPlaywrightTest
    {
        [TestMethod("1. Navigation to Gesprekresultaten page")]
        public async Task NavigationGespreksresultaten()
        {
            await Step("Given the user navigates to the Beheer tab ");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();

            await Step("When the user clicks on Gespreksresultaten tab ");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();

            await Step("Then list of Gespreksresultaten are displayed ");

            await Expect(Page.GetByRole(AriaRole.Listitem)).ToBeVisibleAsync();

        }

        [TestMethod("2. Adding a Gespreksresultaat")]
        public async Task AddGesprekresultaten()
        {
            string title = "Automation Gespreksresultaten";

            try
            {
                await Step("Given user navigates to 'Gespreksresultaten' section");
                await AddGespreksresultaatHelper(title);

                await Step("Then the newly created Gespreksresultaat is displayed");
                await Expect(Page.GetByRole(AriaRole.Listitem)
                    .Filter(new() { HasText = title }))
                    .ToBeVisibleAsync();
            }
            finally
            {
                if (await Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = title }).IsVisibleAsync())
                {
                    await DeleteGespreksresultaatHelper(title);
                }
            }
        }


        [TestMethod("3. Editing an existing Gesprekresultaten")]
        public async Task Editgespreksresultaat()
        {
            // Precondition: Add the Gesprekresultaten
            string originalGesprekresultaat = "Automation Gesprekresultaten edit";
            string updatedGesprekresultaat = "Automation Gesprekresultaten Updated";


            try
            {
                // ✅ Precondition: Clean up test data if already present
                await DeleteGespreksresultaatIfExists(updatedGesprekresultaat); // In case previous run was interrupted
                await AddGespreksresultaatHelper(originalGesprekresultaat);
                await Step("Given the user is on the 'Gesprekresultaten' section of the 'Beheer' tab");

                await Page.GotoAsync("/");
                await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
                await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();

                await Step($"When user clicks on channel list with name as '{"Automation Gesprekresultaten edit"}'");

                await Page.GetByRole(AriaRole.Link, new() { Name = "Automation Gesprekresultaten edit" }).ClickAsync();

                await Step($"And user updates title to '{updatedGesprekresultaat}'");

                await Page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(updatedGesprekresultaat);

                await Step("And user clicks on Opslaan");

                await Page.GetOpslaanButton().ClickAsync();

                await Step($"And updated Gesprekresultaten '{updatedGesprekresultaat}' is added to the list of gespreksresultaten");

                await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = updatedGesprekresultaat })).ToBeVisibleAsync();
            }
            finally
            {
                await DeleteGespreksresultaatHelper(updatedGesprekresultaat);
            }
        }

        [TestMethod("4. Deleting a Gespreksresultaten")]
        public async Task Deletegespreksresultaat()
        {
            string testItemName = "Automation Gesprekresultaten delete";

            try
            {
                await Step("Check if the Gespreksresultaat already exists");
                var exists = await Page.GetByRole(AriaRole.Listitem)
                    .Filter(new() { HasText = testItemName })
                    .IsVisibleAsync();

                if (exists)
                {
                    await Step("It exists, so delete it directly");
                    await DeleteGespreksresultaatHelper(testItemName);
                }
                else
                {
                    await Step("It does not exist, so create and delete it");
                    await AddGespreksresultaatHelper(testItemName);
                    await DeleteGespreksresultaatHelper(testItemName);
                }

                var isStillVisible = await Page.GetByRole(AriaRole.Listitem)
                    .Filter(new() { HasText = testItemName })
                    .IsVisibleAsync();

                Assert.IsFalse(isStillVisible, "Gespreksresultaat should be deleted but is still visible.");
            }
            finally
            {
                if (await Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = testItemName }).IsVisibleAsync())
                {
                    await DeleteGespreksresultaatHelper(testItemName);
                }
            }
        }


        // Helper method to add a new Gesprekresultaten
        private async Task AddGespreksresultaatHelper(string Gespreksresultaat)
        {
            await Step("Given user navigates to 'Gesprekresultaten' section of Beheer tab");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();

            // Check if the Gesprekresultaten already exists
            var existinggespreksresultaat = Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = Gespreksresultaat });
            if (await existinggespreksresultaat.CountAsync() > 0)
            {
                await Step($"gespreksresultaat '{Gespreksresultaat}' already exists. Skipping creation.");
                return; // Skip creating if it already exists
            }

            await Step("When user clicks on the add icon present at the bottom of the list");
            await Page.GetByRole(AriaRole.Button, new() { Name = "toevoegen" }).ClickAsync();

            await Step("And enters the Gesprekresultaat name in the 'Naam' field");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(Gespreksresultaat);

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then the newly created Gesprekresultaat is displayed in the Gesprekresultaten list");
            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = Gespreksresultaat })).ToBeVisibleAsync();
        }

        // Helper method to delete a Gesprekresultaten
        private async Task DeleteGespreksresultaatHelper(string Gespreksresultaat)
        {
            await Step("Given the user is on the 'Gesprekresultaten' section of the 'Beheer' tab");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();

            await Step("When user clicks on the delete icon of the Gesprekresultaten in the list");

            var deleteButtonLocator = Page.GetByRole(AriaRole.Listitem)
                .Filter(new() { HasText = Gespreksresultaat }).GetByRole(AriaRole.Button);

            await deleteButtonLocator.First.ClickAsync();

            await Step("And confirms a pop-up window with the message ‘Weet u zeker dat u dit Gesprekresultaten wilt verwijderen?’");

            using (var _ = Page.AcceptAllDialogs())
            {
                await deleteButtonLocator.ClickAsync();
            }

            await Step("Then the Gesprekresultaten is removed from the Gesprekresultaten list");

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = Gespreksresultaat })).ToHaveCountAsync(0);
        }

        // / Helper method to delete a Gesprekresultaten if at all it exists
        private async Task DeleteGespreksresultaatIfExists(string gesprekresultaat)
        {
            var item = Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = gesprekresultaat });
            if (await item.IsVisibleAsync())
            {
                await DeleteGespreksresultaatHelper(gesprekresultaat);
            }
        }


    }
}
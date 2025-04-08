using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
using Kiss.Bff.EndToEndTest.Helpers;

namespace Kiss.Bff.EndToEndTest.Beheer
{
    [TestClass]
    public class Gesprekresultaten : KissPlaywrightTest
    {
        [TestMethod("1. Navigation to Gesprekresultaten page")]
        public async Task NavigationKanalen()
        {
            await Step("Given the user navigates to the Beheer tab ");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();

            await Step("When the user clicks on Kanalen tab ");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();

            await Step("Then list of Gespreksresultaten are displayed ");

            await Expect(Page.GetByRole(AriaRole.Listitem)).ToBeVisibleAsync();

        }

        [TestMethod("2. Adding a Gespreksresultaten")]
        [DataRow("Automation Gespreksresultaten")]
        public async Task AddGesprekresultaten(string Gesprekresultaten)

        {
            await Step("Given user navigates to 'Gesprekresultaten' section of Beheer tab");

            await AddGesprekresultatenHelper(Gesprekresultaten);

            await Step("Then the newly created Gesprekresultaten is displayed in the Gesprekresultaten list");

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = Gesprekresultaten })).ToBeVisibleAsync();

            await DeleteGesprekresultatenHelper(Gesprekresultaten);
        }

        [TestMethod("3. Editing an existing Gesprekresultaten")]
        [DataRow("Automation Gesprekresultaten edit")]
        public async Task EditKanaal(string originalGesprekresultaten)
        {
            // Precondition: Add the Gesprekresultaten
            await AddGesprekresultatenHelper(originalGesprekresultaten);

            string updatedGesprekresultaten = "Automation Gesprekresultaten Updated";


            try
            {
                await Step("Given the user is on the 'Gesprekresultaten' section of the 'Beheer' tab");

                await Page.GotoAsync("/");
                await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
                await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();

                await Step($"When user clicks on channel list with name as '{originalGesprekresultaten}'");

                await Page.GetByRole(AriaRole.Link, new() { Name = originalGesprekresultaten }).ClickAsync();

                await Step($"And user updates title to '{updatedGesprekresultaten}'");

                await Page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(updatedGesprekresultaten);

                await Step("And user clicks on Opslaan");

                await Page.GetOpslaanButton().ClickAsync();

                await Step($"And updated channel '{updatedGesprekresultaten}' is added to the list of Kanalen");

                await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = updatedGesprekresultaten })).ToBeVisibleAsync();
            }
            finally
            {
                await DeleteGesprekresultatenHelper(updatedGesprekresultaten);
            }
        }

        [TestMethod("4. Deleting a Gespreksresultaten")]
        [DataRow("Automation Gesprekresultaten delete")]
        public async Task DeleteKanaal(string deleteGesprekresultaten)
        {
            await Step("Precondition: Automation Gesprekresultaten is created");
            await AddGesprekresultatenHelper(deleteGesprekresultaten);

            try
            {
                await Step("When the user deletes the Gesprekresultaten");
                await DeleteGesprekresultatenHelper(deleteGesprekresultaten);
            }
            finally
            {

                var GespreksresultatenExists = await Page.GetByRole(AriaRole.Listitem)
                    .Filter(new() { HasText = deleteGesprekresultaten })
                    .IsVisibleAsync();

                if (GespreksresultatenExists)
                {
                    await DeleteGesprekresultatenHelper(deleteGesprekresultaten);
                }
            }
        }


        // Helper method to add a new Gesprekresultaten
        private async Task AddGesprekresultatenHelper(string Gesprekresultaten)
        {
            await Step("Given user navigates to 'Gesprekresultaten' section of Beheer tab");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();

            // Check if the Gesprekresultaten already exists
            var existingKanaal = Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = Gesprekresultaten });
            if (await existingKanaal.CountAsync() > 0)
            {
                await Step($"Channel '{Gesprekresultaten}' already exists. Skipping creation.");
                return; // Skip creating if it already exists
            }

            await Step("When user clicks on the add icon present at the bottom of the list");
            await Page.GetByRole(AriaRole.Button, new() { Name = "toevoegen" }).ClickAsync();

            await Step("And enters the channel name in the 'Naam' field");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(Gesprekresultaten);

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then the newly created channel is displayed in the channel list");
            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = Gesprekresultaten })).ToBeVisibleAsync();
        }

        // Helper method to delete a Gesprekresultaten
        private async Task DeleteGesprekresultatenHelper(string Gesprekresultaten)
        {
            await Step("Given the user is on the 'Gesprekresultaten' section of the 'Beheer' tab");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();

            await Step("When user clicks on the delete icon of the Gesprekresultaten in the list");

            var deleteButtonLocator = Page.GetByRole(AriaRole.Listitem)
                .Filter(new() { HasText = Gesprekresultaten }).GetByRole(AriaRole.Button);

            await deleteButtonLocator.First.ClickAsync();

            await Step("And confirms a pop-up window with the message ‘Weet u zeker dat u dit Gesprekresultaten wilt verwijderen?’");

            using (var _ = Page.AcceptAllDialogs())
            {
                await deleteButtonLocator.ClickAsync();
            }

            await Step("Then the Gesprekresultaten is removed from the Gesprekresultaten list");

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = Gesprekresultaten })).ToHaveCountAsync(0);
        }

    }
}
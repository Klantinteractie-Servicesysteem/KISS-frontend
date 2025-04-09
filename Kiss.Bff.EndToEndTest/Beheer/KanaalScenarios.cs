
using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
using Kiss.Bff.EndToEndTest.Helpers;

namespace Kiss.Bff.EndToEndTest.Beheer
{
    [TestClass]
    public class Beheer : KissPlaywrightTest
    {
        [TestMethod("1. Navigation to Kanalen page")]
        public async Task NavigationKanalen()
        {
            await Step("Given the user navigates to the Beheer tab ");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();

            await Step("When the user clicks on Kanalen tab ");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Kanalen" }).ClickAsync();

            await Step("Then list of channels are displayed ");

            await Expect(Page.GetByRole(AriaRole.Listitem)).ToBeVisibleAsync();

        }

        [TestMethod("2. Adding a Kanalen")]
        [DataRow("Automation Channel")]
        public async Task AddKanaal(string kanaalName)

        {
            await Step("Given user navigates to 'Kanalen' section of Beheer tab");

            await AddKanaalHelper(kanaalName);

            await Step("Then the newly created channel is displayed in the channel list");

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = kanaalName })).ToBeVisibleAsync();

            await DeleteKanaalHelper(kanaalName);
        }

        [TestMethod("3. Editing an existing Kanaal")]
        [DataRow("Automation Channel edit")]
        public async Task EditKanaal(string originalkanaalName)
        {
            // Precondition: Add the channel
            await AddKanaalHelper(originalkanaalName);

            string updatedKanaalName = "Automation Channel Updated";

            try
            {
                await Step("Given the user is on the 'Kanalen' section of the 'Beheer' tab");

                await Page.GotoAsync("/");
                await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
                await Page.GetByRole(AriaRole.Link, new() { Name = "Kanalen" }).ClickAsync();

                await Step($"When user clicks on channel list with name as '{originalkanaalName}'");

                await Page.GetByRole(AriaRole.Link, new() { Name = originalkanaalName }).ClickAsync();

                await Step($"And user updates title to '{updatedKanaalName}'");

                await Page.GetByRole(AriaRole.Textbox, new() { Name = "Naam" }).FillAsync(updatedKanaalName);

                await Step("And user clicks on Opslaan");

                await Page.GetOpslaanButton().ClickAsync();

                await Step($"And updated channel '{updatedKanaalName}' is added to the list of Kanalen");

                await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = updatedKanaalName })).ToBeVisibleAsync();
            }
            finally
            {
                await DeleteKanaalHelper(updatedKanaalName);
            }
        }

        [TestMethod("4. Deleting a Kanaal")]
        [DataRow("Automation Channel delete")]
        public async Task DeleteKanaal(string deleteKanaal)
        {
            await Step("Precondition: Automation channel is created");
            await AddKanaalHelper(deleteKanaal);

            try
            {
                await Step("When the user deletes the channel");
                await DeleteKanaalHelper(deleteKanaal);
            }
            finally
            {

                var kanaalExists = await Page.GetByRole(AriaRole.Listitem)
                    .Filter(new() { HasText = deleteKanaal })
                    .IsVisibleAsync();

                if (kanaalExists)
                {
                    await DeleteKanaalHelper(deleteKanaal);
                }
            }
        }


        // Helper method to add a new channel
        private async Task AddKanaalHelper(string kanaalName)
        {
            await Step("Given user navigates to 'Kanalen' section of Beheer tab");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Kanalen" }).ClickAsync();

            // Check if the kanaal already exists
            var existingKanaal = Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = kanaalName });
            if (await existingKanaal.CountAsync() > 0)
            {
                await Step($"Channel '{kanaalName}' already exists. Skipping creation.");
                return; // Skip creating if it already exists
            }

            await Step("When user clicks on the add icon present at the bottom of the list");
            await Page.GetByRole(AriaRole.Link, new() { Name = "toevoegen" }).ClickAsync();

            await Step("And enters the channel name in the 'Naam' field");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Naam" }).FillAsync(kanaalName);

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then the newly created channel is displayed in the channel list");
            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = kanaalName })).ToBeVisibleAsync();
        }

        // Helper method to delete a channel
        private async Task DeleteKanaalHelper(string kanaalName)
        {
            await Step("Given the user is on the 'Kanalen' section of the 'Beheer' tab");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Kanalen" }).ClickAsync();

            await Step("When user clicks on the delete icon of the channel in the list");

            var deleteButtonLocator = Page.GetByRole(AriaRole.Listitem)
                .Filter(new() { HasText = kanaalName }).GetByRole(AriaRole.Button);

            await deleteButtonLocator.First.ClickAsync();

            await Step("And confirms a pop-up window with the message ‘Weet u zeker dat u dit bericht wilt verwijderen?’");

            using (var _ = Page.AcceptAllDialogs())
            {
                await deleteButtonLocator.ClickAsync();
            }

            await Step("Then the channel is removed from the channel list");

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = kanaalName })).ToHaveCountAsync(0);
        }

    }
}



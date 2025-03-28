




using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
using Kiss.Bff.EndToEndTest.Helpers;

namespace Kiss.Bff.EndToEndTest.Beheer
{
    [TestClass]
    public class Beheer : KissPlaywrightTest
    {
        // Helper method to add a new channel
        public async Task AddKanelHelper(string channelName)
        {
            await Step("Given user navigates to 'Kanelen' section of Beheer tab");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Kanalen" }).ClickAsync();

            await Step("When user clicks on the add icon present at the bottom of the list");

            await Page.GetByRole(AriaRole.Link, new() { Name = "toevoegen" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Naam" }).ClickAsync();

            await Step("And enters the channel name in the 'Naam' field");

            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Naam" }).FillAsync(channelName);

            await Step("And user clicks on Opslaan button");

            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then the newly created channel is displayed in the channel list");

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = channelName })).ToBeVisibleAsync();
        }

        // Helper method to delete a channel
        public async Task DeleteKanelHelper(string channelName)
        {
            await Step("Given the user is on the 'Kanalen' section of the 'Beheer' tab");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Kanalen" }).ClickAsync();

            await Step("When user clicks on the delete icon of the channel in the list");

            var deleteButtonLocator = Page.GetByRole(AriaRole.Listitem)
                .Filter(new() { HasText = channelName }).GetByRole(AriaRole.Button);

            await deleteButtonLocator.First.ClickAsync();

            await Step("And confirms a pop-up window with the message ‘Weet u zeker dat u dit bericht wilt verwijderen?’");

            using (var _ = Page.AcceptAllDialogs())
            {
                await deleteButtonLocator.ClickAsync();
            }

            await Step("Then the channel is removed from the channel list");

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = channelName })).ToHaveCountAsync(0);
        }


        [TestMethod("1. Navigation to Kanelen page")]
        public async Task NavigationKanelen()
        {
            await Step("Given the user navigates to the Beheer tab ");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();

            await Step("When the user clicks on Kanelen tab ");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Kanalen" }).ClickAsync();

            await Step("Then list of channels are displayed ");

            var listItems = await Page.GetByRole(AriaRole.Listitem).AllAsync();
            await Task.WhenAll(listItems.Select(item => Expect(item).ToBeVisibleAsync())); // Assert visibility for each item
        }

        [TestMethod("2. Adding a Kanelen")]
        [DataRow("Automation Channel")]
        public async Task AddKanel(string channelName)
        {
            await Step("Given user navigates to 'Kanelen' section of Beheer tab");

            await AddKanelHelper(channelName);

            await Step("Then the newly created channel is displayed in the channel list");

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = channelName })).ToBeVisibleAsync();

            await DeleteKanelHelper(channelName);
        }

        [TestMethod("3. Editing an existing Kanelen")]
        [DataRow("Automation Channel edit")]
        public async Task EditKanel(string originalChannelName)
        {
            // Precondition: Add the channel
            await AddKanelHelper(originalChannelName);

            await Step("Given the user is on the 'Kanalen' section of the 'Beheer' tab");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Kanalen" }).ClickAsync();

            await Step($"When user clicks on channel list with name as '{originalChannelName}'");

            await Page.GetByRole(AriaRole.Link, new() { Name = originalChannelName }).ClickAsync();

            await Step("And user updates title to 'Automation Channel Update'");

            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Naam" }).FillAsync("Automation Edit");

            await Step("And user clicks on Opslaan");

            await Page.GetOpslaanButton().ClickAsync();

            await Step("And updated channel is added to the list of Kanalen");

            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = "Automation Edit" })).ToBeVisibleAsync();

            await DeleteKanelHelper("Automation Edit");
        }

        [TestMethod("4. Deleting a Kanelen")]
        [DataRow("Automation Channel delete")]
        public async Task DeleteKanel(String deleteChannel)
        {
            await Step("Precondition: Automation channel is created");

            await AddKanelHelper(deleteChannel);

            await Step("Given the user is on the 'Kanalen' section of the 'Beheer' tab");

            await DeleteKanelHelper(deleteChannel);

        }
    }
}


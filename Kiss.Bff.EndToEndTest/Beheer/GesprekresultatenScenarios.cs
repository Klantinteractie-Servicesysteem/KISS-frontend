using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
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

            await Step("Given user navigates to 'Gespreksresultaten' section");
            await AddGespreksresultaatHelper(title);

            await Step("Then the newly created Gespreksresultaat is displayed");
            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = title })).ToBeVisibleAsync();

            await DeleteGespreksresultaatHelper(title);
        }


        [TestMethod("3. Editing an existing Gesprekresultaten")]
        public async Task Editgespreksresultaat()
        {
            // Precondition: Add the Gesprekresultaten
            String OriginalGesprekresultaat = "Automation Gesprekresultaten edit";
            await AddGespreksresultaatHelper(OriginalGesprekresultaat);

            string updatedGesprekresultaat = "Automation Gesprekresultaten Updated";


            try
            {
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

                await Step($"And updated channel '{updatedGesprekresultaat}' is added to the list of Kanalen");

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
            await Step("Precondition: Automation Gesprekresultaten is created");
            String Deletegespreksresultaat = "Automation Gesprekresultaten delete";
            await AddGespreksresultaatHelper(Deletegespreksresultaat);

            try
            {
                await Step("When the user deletes the Gesprekresultaten");
                await DeleteGespreksresultaatHelper(Deletegespreksresultaat);
            }
            finally
            {

                var GespreksresultaatExists = await Page.GetByRole(AriaRole.Listitem)
                    .Filter(new() { HasText = Deletegespreksresultaat })
                    .IsVisibleAsync();

                if (GespreksresultaatExists)
                {
                    await DeleteGespreksresultaatHelper(Deletegespreksresultaat);
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
                await Step($"Channel '{Gespreksresultaat}' already exists. Skipping creation.");
                return; // Skip creating if it already exists
            }

            await Step("When user clicks on the add icon present at the bottom of the list");
            await Page.GetByRole(AriaRole.Button, new() { Name = "toevoegen" }).ClickAsync();

            await Step("And enters the channel name in the 'Naam' field");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(Gespreksresultaat);

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then the newly created channel is displayed in the channel list");
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

    }
}
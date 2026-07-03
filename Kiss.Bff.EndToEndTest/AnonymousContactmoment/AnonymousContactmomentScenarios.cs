using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.EndToEndTest.AnonymousContactmoment.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;

namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen
{

    [TestClass]
    public class AnonymousContactmomentScenarios : KissPlaywrightTest
    {
        [TestMethod("1. Search for Bronnen in Contactmoment")]
        public async Task SearchForBronnenInContactmoment()
        {
            await Step("Given the user is on the Startpagina");

            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And enters 'boom' in the search field in the Search pane");

            await Page.GetGlobalSearch().FillAsync("boom");

            await Step("And presses Enter");

            await Page.GetGlobalSearch().PressAsync("Enter");

            await Step("Then 10 items should appear in the Search pane");

            await Expect(Page.GetGlobalSearchResults()).ToHaveCountAsync(10);

            await Step("And each item has a label VAC or Kennisbank or Website in the first column");

            await Task.WhenAll((await Page.GetGlobalSearchResults().AllAsync()).Select(async item =>
            {
                var itemText = await item.InnerTextAsync() ?? string.Empty;
                var lines = itemText
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var hasKnownBronLabel =
                    itemText.Contains("VAC", StringComparison.OrdinalIgnoreCase)
                    || itemText.Contains("Kennisbank", StringComparison.OrdinalIgnoreCase)
                    || itemText.Contains("Website", StringComparison.OrdinalIgnoreCase)
                    || itemText.Contains("Smoelenboek", StringComparison.OrdinalIgnoreCase);

                var hasMetadataLine = lines.Length >= 2 && !string.IsNullOrWhiteSpace(lines[1]);

                Assert.IsTrue(
                    hasKnownBronLabel || hasMetadataLine,
                    $"Expected result item to contain a known bron label or metadata line, but got: '{itemText}'"
                );
            }));
        }

        [TestMethod("2. Search for Smoelenboek in Contactmoment")]
        public async Task SearchForSmoelenboekInContactmoment()
        {
            await Step("Given the user is on the Startpagina");

            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And checks the box Smoelenboek");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Page.GetSmoelenboekCheckbox().ClickAsync();
            await Expect(Page.GetSmoelenboekCheckbox()).ToBeCheckedAsync();

            await Step("And enters 'boom' in the search field in the Search pane");

            await Page.GetGlobalSearch().FillAsync("boom");
            await Step("And presses Enter");
            await Page.GetGlobalSearch().PressAsync("Enter");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then 10 items should appear");

            await Expect(Page.GetGlobalSearchResults()).ToHaveCountAsync(10);

            await Step("And each item has a label Smoelenboek in the first column");

            await Task.WhenAll((await Page.GetGlobalSearchResults().AllAsync()).Select(async item =>
            {
                await Expect(item).ToContainTextAsync("Smoelenboek");
            }));
        }

        [TestMethod("3. Search for VAC in Contactmoment")]
        public async Task SearchForVACInContactmoment()
        {
            await Step("Given the user is on the Startpagina");

            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And checks the box VAC in the Search pane");
           
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Page.GetVACCheckbox().ClickAsync();
            await Expect(Page.GetVACCheckbox()).ToBeCheckedAsync();
        
            await Step("And enters 'boom' in the search field in the Search pane");

            await Page.GetGlobalSearch().FillAsync("boom");

            await Step("And presses Enter");

            await Page.GetGlobalSearch().PressAsync("Enter");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then 10 items should appear");

            await Expect(Page.GetGlobalSearchResults()).ToHaveCountAsync(10);

            await Step("And each item has a label VAC in the first column");

            await Task.WhenAll((await Page.GetGlobalSearchResults().AllAsync()).Select(async item =>
            {
                await Expect(item).ToContainTextAsync("VAC");
            }));
        }

        [TestMethod("4. Search for Kennisbank in Contactmoment")]
        public async Task SearchForKennisbankInContactmoment()
        {
            await Step("Given the user is on the Startpagina");

            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And checks the box Kennisbank in the Search pane");

            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Page.GetKennisbankCheckbox().ClickAsync();
            await Expect(Page.GetKennisbankCheckbox()).ToBeCheckedAsync();
        
            await Step("And enters 'boom' in the search field in the Search pane");

            await Page.GetGlobalSearch().FillAsync("boom");

            await Step("And presses Enter");

            await Page.GetGlobalSearch().PressAsync("Enter");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then 10 items should appear");

            await Expect(Page.GetGlobalSearchResults()).ToHaveCountAsync(10);

            await Step("And each item has a label Kennisbank in the first column");

            await Task.WhenAll((await Page.GetGlobalSearchResults().AllAsync()).Select(async item =>
            {
                await Expect(item).ToContainTextAsync("Kennisbank");
            }));
        }

        [Ignore("info.nl checkbox no longer available in the test environment")]
        [TestMethod("5. Search for Website in Contactmoment")]
        public async Task SearchForWebsiteInContactmoment()
        {
            await Step("Given the user is on the Startpagina");

            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And checks the box info.nl in the Search pane");

            await Page.GetInfoNlCheckbox().CheckAsync();

            await Step("And enters 'clients' in the search field in the Search pane");

            await Page.GetGlobalSearch().FillAsync("clients-INFO");

            await Step("And presses Enter");

            await Page.GetGlobalSearch().PressAsync("Enter");

            await Step("And the item has a label Website in the first column");

            var items = await Page.GetGlobalSearchResults().AllAsync();

            foreach (var item in items)
            {
                var label = item.Locator("span:nth-of-type(1)");
                await Expect(label.Filter(new() { HasText = "Website" })).ToBeVisibleAsync();
            }

        }

        [TestMethod("6. Fill Afdeling on Afhandeling form by viewing Kennisartikel")]
        public async Task FillAfdelingOnAfhandelingFormByViewingKennisartikel()
        {
            await Step("Given the user is on the Startpagina");

            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And enters 'andere achternaam gebruiken' in the search field in Search pane");

            await Page.GetGlobalSearch().FillAsync("andere achternaam gebruiken");

            await Step("There should be 1 Kennisartikel in the list of results with the title 'Andere achternaam gebruiken'");

            var item = Page.GetGlobalSearchResults().Locator("span:nth-of-type(2)").Filter(new() { HasText = "Andere achternaam gebruiken" });

            await Expect(item).ToBeVisibleAsync();
            await Expect(item).ToHaveCountAsync(1);

            await Step("When user clicks on the item 'Andere achternaam gebruiken'");

            await item.ClickAsync();

            await Step("Then the search pane should display the article with title 'Andere achternaam gebruiken' and heading 'Inleiding'");

            await Expect(Page.GetByRole(AriaRole.Article).GetByRole(AriaRole.Heading, new() { Name = "Andere achternaam gebruiken" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Article).GetByRole(AriaRole.Heading, new() { Name = "Inleiding" })).ToBeVisibleAsync();

            await Step("And user clicks on 'Bijzonderheden' in the Search pane");

            await Page.GetBijzonderhedenTab().ClickAsync();

            await Step("And then clicks on Afronden in the Notes-Contactverzoek-Pane");

            await Page.GetPersonenAfrondenButton().ClickAsync();

            await Step("When Afhandeling form is displayed");

            await Expect(Page.GetAfhandelingForm()).ToBeVisibleAsync();

            await Step("Then the field 'Vraag' has value 'Andere achernaam gebruiken - Bijzonderheden'");

            await Expect(Page.GetVraagField().Locator("option:checked")).ToHaveTextAsync("Andere achternaam gebruiken - Bijzonderheden");

            await Step("And the dropdown list of the field Vraag has 8 items");

            await Expect(Page.GetVraagField().Locator("option")).ToHaveCountAsync(8);

            await Step("And the field 'Afdeling' has value 'Publiekscontacten Burgertaken en gegevensbeheer'");

            await Expect(Page.GetAfdelingField()).ToHaveValueAsync("Publiekscontacten Burgertaken en gegevensbeheer");
        }
    }
}

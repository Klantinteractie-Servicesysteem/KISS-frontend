

using Kiss.Bff.EndToEndTest.AnonymousContactmomentZaak.Helpers;
using Kiss.Bff.EndToEndTest.Common.Helpers;

namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentZaak
{
    [TestClass]
    public class AnonymousContactmomentZaakScenarios : KissPlaywrightTest
    {
        [TestMethod("1. Search for Zaak in Contactmoment")]
        public async Task SearchForZaakInContactmoment()
        {
            await Step("Given the user has started a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When the user clicks on 'Zaken' in the menu");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Zaken" }).ClickAsync();

            await Step("And enters 'ZAAK-2023-001' in the search field");

            await Page.GetByPlaceholder("Zoek op zaaknummer").FillAsync("ZAAK-2023-001");

            await Step("And clicks zoeken (magnifying glass-icon)");

            await Page.GetByTitle("Zoeken").ClickAsync();

            await Step("Then the user will navigate to the screen 'Zaak ZAAK-2023-001'");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Zaak ZAAK-2023-001" })).ToBeVisibleAsync();
        }

        [TestMethod("2. Register Contactmoment bij Zaak - I")]
        public async Task RegisterContactmomentBijZaakI()
        {
            await Step("Precondition: The scenario 'Search for Zaak in Contactmoment' has been executed");

            await SearchForZaakInContactmoment();

            await Step("Given the user is on the 'Zaak ZAAK-2023-001' screen");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Zaak ZAAK-2023-001" })).ToBeVisibleAsync();

            await Step("When the user clicks Afronden in the Notes-Contactverzoek-Pane");

            await Page.GetByRole(AriaRole.Button, new() { Name = "Afronden" }).ClickAsync();

            await Step("And the Afhandeling form is displayed");

            // This locator does not use an ARIA role. Consider updating the locator to use an ARIA role for better accessibility.
            await Expect(Page.Locator("form", new() { Has = Page.GetByRole(AriaRole.Heading, new() { Name = "Afhandeling" }) })).ToBeVisibleAsync();

            await Step("Then the heading 'Gerelateerde zaak' appears under 'Vraag 1'");

            var vraagHeading = Page.GetByRole(AriaRole.Heading, new() { Name = "Vraag 1" });

            await Expect(vraagHeading.Locator("..").GetByRole(AriaRole.Heading, new() { Name = "Gerelateerde zaak" })).ToBeVisibleAsync();

            await Step("And the item 'ZAAK-2023-001' is listed");

            await Expect(Page.GetByText("ZAAK-2023-001")).ToBeVisibleAsync();

            await Step("And a checked checkbox is visible");

            await Expect(Page.GetByRole(AriaRole.Radio, new() { Name = "ZAAK-2023-001" })).ToBeCheckedAsync();
        }

        [TestMethod("3. Register Contactmoment bij Zaak - II")]
        public async Task RegisterContactmomentBijZaakII()
        {
            await Step("Precondition: Scenario: Register Contactmoment bij Zaak – I is executed");

            await RegisterContactmomentBijZaakI();

            await Step("Given the user is on the Afhandeling-form");

            // This locator does not use an ARIA role. Consider updating the locator to use an ARIA role for better accessibility.
            await Expect(Page.Locator("form", new() { Has = Page.GetByRole(AriaRole.Heading, new() { Name = "Afhandeling" }) })).ToBeVisibleAsync();

            await Step("When user enters value 'Contactmoment bij ZAAK-2023-001' in field Specifieke vraag");

            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Specifieke vraag" }).FillAsync("Contactmoment bij ZAAK-2023-001");

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetByLabel("Kanaal").SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("And value 'Zelfstandig afgehandeld' in field Afhandeling");

            await Page.GetByRole(AriaRole.Combobox, new() { Name = "Afhandeling" }).SelectOptionAsync(new[] { "Zelfstandig afgehandeld" });

            await Step("And selects value 'Parkeren' in field Afdeling");

            // This locator uses an ARIA role but does not have an associated label. Consider adding a label for better accessibility.
            await Page.Locator("input[type='search']").ClickAsync();

            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And clicks on Opslaan button");

            await Page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" }).ClickAsync();

            await Step("Then message as 'Het contactmoment is opgeslagen' is displayed on the Startpagina");

            await Expect(Page.GetByRole(AriaRole.Status)).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

        [TestMethod("4. View Contactmoment bij Zaak")]
        public async Task ViewContactmomentBijZaak()
        {
            // await Step("Precondition: Scenario: Register Contactmoment bij Zaak – II is executed");

            // await RegisterContactmomentBijZaakII();

            await Step("Given the user is on the Startpagina");

            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on 'Zaken' in the menu");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Zaken" }).ClickAsync();

            await Step("And enters 'ZAAK-2023-001' in the search field");

            await Page.GetByTitle("ZAAK-1").FillAsync("ZAAK-2023-002");

            await Step("And clicks zoeken");

            await Page.GetByTitle("Zoeken").ClickAsync();

            await Step("And navigates to the screen 'Zaak ZAAK-2023-001'");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Zaak ZAAK-2023-002" })).ToBeVisibleAsync();
            // await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("And clicks on tab Contactmomenten");

            await Page.GetByRole(AriaRole.Tab, new() { Name = "Contactmomenten" }).ClickAsync();

            await Step("Then the first Contactmoment should contain the details as entered in Scenario: Register Contactmoment bij Zaak – II");

            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var firstContactMomentSummary = Page.FirstContactMomentSummary();

            await Expect(firstContactMomentSummary.GespreksresultaatHeader()).ToHaveTextAsync("Zelfstandig afgehandeld");
            await Expect(firstContactMomentSummary.AfdelingHeader()).ToHaveTextAsync("Parkeren");

            await Page.FirstContactMomentDetails().Locator("summary").ClickAsync();

            var detailsList = Page.FirstContactMomentDetailsList();
            await Expect(detailsList.ZaakNumber()).ToHaveTextAsync("ZAAK-2023-001");
            await Expect(detailsList.ContactMomentDescription()).ToHaveTextAsync("Contactmoment bij ZAAK-2023-001");
        }
    }


}

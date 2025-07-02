using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.EndToEndTest.Common.Helpers;
using Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers;
using Kiss.Bff.EndToEndTest.Helpers;
using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactverzoek.Helpers;

namespace Kiss.Bff.EndToEndTest.ContactMomentSearch
{
    [TestClass]
    public class ContactMomentCreation : KissPlaywrightTest
    {

        [TestMethod("1. Contact moment Creation for person ")]
        public async Task ContactMomentCreation_Person()
        {
            await Step("Given the user is on the startpagina ");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"999993148\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("999993148");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();

            await Step("Then user is navigated to Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user enters 'Automation Test afhandeling form' in field specific vraag");

            await Page.GetSpecificVraagField().FillAsync("Automation Test afhandeling form");

            await Step("And user selects 'TESTtest' from dropdown list of Afhandeling");

            await Page.GetAfhandelingField().ClickAsync();

            await Page.GetAfhandelingField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "TESTtest" } }); ;

            await Step("And user selects 'parkeren' from the dropdown list");

            await Page.GetAfdelingVoorField().ClickAsync();
            await Page.GetByText("Parkeren").ClickAsync();

            await Expect(Page.GetKanaalField()).ToHaveJSPropertyAsync("validationMessage", "Please select an item in the list.");

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetKanaalField().ClickAsync();

            await Page.GetKanaalField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("And clicks on Opslaan button");

            await Page.GetOpslaanButton().ClickAsync();

            await Step("And Afhandeling form is successfully submitted");

            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"999993148\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("999993148");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();

            await Step("Then user is navigated to Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

            await Step("And user navigates to the contactmomenten tab to view the created contact request");

            await Page.GetByRole(AriaRole.Tab, new() { Name = "Contactmomenten" }).ClickAsync();

            await Step("And contactmoment details are displayed");

            await Page.Locator("summary").Filter(new() { HasText = "TESTtest" }).First.PressAsync("Enter");

            await Expect(Page.GetByRole(AriaRole.Definition).Filter(new() { HasText = "Automation Test afhandeling form" })).ToBeVisibleAsync();


        }

        [TestMethod("Contact moment Creation for person is cancelled ")]
        public async Task ContactMomentCreation_Person_cancel()
        {
            await Step("Given the user is on the startpagina ");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"999993148\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("999993148");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();

            await Step("Then user is navigated to Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user enters 'Automation Test afhandeling form' in field specific vraag");

            await Page.GetSpecificVraagField().FillAsync("Automation Test afhandeling form");

            await Step("And user selects 'TESTtest' from dropdown list of Afhandeling");

            await Page.GetAfhandelingField().ClickAsync();

            await Page.GetAfhandelingField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "TESTtest" } }); ;

            await Step("And user selects 'parkeren' from the dropdown list");

            await Page.GetAfdelingVoorField().ClickAsync();
            await Page.GetByText("Parkeren").ClickAsync();

            await Expect(Page.GetKanaalField()).ToHaveJSPropertyAsync("validationMessage", "Please select an item in the list.");

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetKanaalField().ClickAsync();

            await Page.GetKanaalField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("user click on Annuleren");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And confirms a pop-up window with the message ‘Weet je zeker dat je het contactmoment wilt annuleren? Alle gegevens worden verwijderd.’");
            using (var _ = Page.AcceptAllDialogs())
            {
                await Page.GetByRole(AriaRole.Button, new() { Name = "Ja" }).ClickAsync();

            }

            await Step("Then user navigates to KISS home page");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Startscherm" })).ToBeVisibleAsync();

        }

        [TestMethod("3. Contact moment Creation for company")]
        public async Task ContactMomentCreation_Company()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And user enters “000055679269” in Vestigingsnummer field");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();
            await Page.Company_KvknummerInput().FillAsync("000055679269");

            await Step("And clicks the search button");
            await Page.Company_KvknummerSearchButton().ClickAsync();
            await Page.WaitForURLAsync("**/bedrijven/*");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user enters 'Automation Test afhandeling form' in field specific vraag");

            await Page.GetSpecificVraagField().FillAsync("Automation Test afhandeling form");

            await Step("And user selects 'TESTtest' from dropdown list of Afhandeling");

            await Page.GetAfhandelingField().ClickAsync();

            await Page.GetAfhandelingField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "TESTtest" } }); ;

            await Step("And user selects 'parkeren' from the dropdown list");

            await Page.GetAfdelingVoorField().ClickAsync();
            await Page.GetByText("Parkeren").ClickAsync();

            await Expect(Page.GetKanaalField()).ToHaveJSPropertyAsync("validationMessage", "Please select an item in the list.");

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetKanaalField().ClickAsync();

            await Page.GetKanaalField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("And clicks on Opslaan button");

            await Page.GetOpslaanButton().ClickAsync();

            await Step("And Afhandeling form is successfully submitted");

            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"999993148\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("999993148");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();

            await Step("Then user is navigated to Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

            await Step("And user navigates to the contactmomenten tab to view the created contact request");

            await Page.GetByRole(AriaRole.Tab, new() { Name = "Contactmomenten" }).ClickAsync();

            await Step("And contactmoment details are displayed");

            await Page.Locator("summary").Filter(new() { HasText = "TESTtest" }).First.PressAsync("Enter");

            await Expect(Page.GetByRole(AriaRole.Definition).Filter(new() { HasText = "Automation Test afhandeling form" })).ToBeVisibleAsync();

        }

        [TestMethod("4. Contact moment Creation for company is cancelled")]
        public async Task ContactMomentCreation_Company_Cancel()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And user enters “000055679269” in Vestigingsnummer field");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();
            await Page.Company_KvknummerInput().FillAsync("000055679269");

            await Step("And clicks the search button");
            await Page.Company_KvknummerSearchButton().ClickAsync();
            await Page.WaitForURLAsync("**/bedrijven/*");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user enters 'Automation Test afhandeling form' in field specific vraag");

            await Page.GetSpecificVraagField().FillAsync("Automation Test afhandeling form");

            await Step("And user selects 'TESTtest' from dropdown list of Afhandeling");

            await Page.GetAfhandelingField().ClickAsync();

            await Page.GetAfhandelingField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "TESTtest" } }); ;

            await Step("And user selects 'parkeren' from the dropdown list");

            await Page.GetAfdelingVoorField().ClickAsync();
            await Page.GetByText("Parkeren").ClickAsync();

            await Expect(Page.GetKanaalField()).ToHaveJSPropertyAsync("validationMessage", "Please select an item in the list.");

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetKanaalField().ClickAsync();

            await Page.GetKanaalField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("user click on Annuleren");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And confirms a pop-up window with the message ‘Weet je zeker dat je het contactmoment wilt annuleren? Alle gegevens worden verwijderd.’");
            using (var _ = Page.AcceptAllDialogs())
            {
                await Page.GetByRole(AriaRole.Button, new() { Name = "Ja" }).ClickAsync();

            }

            await Step("Then user navigates to KISS home page");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Startscherm" })).ToBeVisibleAsync();


        }

        [TestMethod("Scenario 1: Multiple ContactMoments")]
        public async Task ContactMomentMultiple()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("Add a note");
            var note = "test multiple contactmoment";
            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note);

            await Step("Search for VAC’s, look at first two search results ");
            await Page.GetByRole(AriaRole.Combobox).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox).FillAsync("het");
            await Page.GetByText("Heb ik een ROVA-pas nodig voor de gft-container?").ClickAsync();
            await Page.GetByText("Heb ik een ROVA-pas nodig voor de gft-container?").ClickAsync();

            await Step("Start another contact moment");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Nieuw", Exact = true }).ClickAsync();

            await Step("Search for Kennisbank, look at first two search results ");
            await Page.GetByRole(AriaRole.Combobox).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox).FillAsync("heb ik een vergunning");
            await Page.GetByText("heb ik een vergunning nodig om mijn pand te splits").ClickAsync();
            await Page.GetByText("heb ik een vergunning nodig om mijn pand te splits").ClickAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And selects group radiobutton");
            await Page.GetMedewerkerRadioButton().ClickAsync();

            await Step("And user selects 'Sytske' from dropdown list of medewerker group");

            await Page.GetByRole(AriaRole.Combobox, new() { Name = "Medewerker" }).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox, new() { Name = "Medewerker" }).FillAsync("sytsk");
            await Page.GetByText("Sytske de eSuiteBeheerder").ClickAsync();

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("And the Contactverzoek section of the Afhandeling screen is loaded");
            await Expect(Page.GetByText("Contactverzoek maken voor")).ToBeVisibleAsync();

            await Step(" VACs are displayed in the Gerelaterede VACs section in afhnadeling form");
            var sectionLocator = Page.Locator("section.gerelateerde-resources");

            await Expect(sectionLocator.GetByRole(AriaRole.Heading, new() { Name = "Gerelateerde VAC" })).ToBeVisibleAsync();

            var vergunningLabel = sectionLocator.Locator("label").Filter(new() { HasText = "Heb ik een vergunning nodig om mijn pand te splitsen?" });
            await Expect(vergunningLabel).ToBeVisibleAsync();

            await Step("Verify that 'Contactverzoek gemaakt' is selected in Afhandeling dropdown");
            await Expect(Page.GetByLabel("Afhandeling")).ToHaveValueAsync("Contactverzoek gemaakt");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("user click on Annuleren");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And confirms a pop-up window with the message ‘Weet je zeker dat je het contactmoment wilt annuleren? Alle gegevens worden verwijderd.’");
            using (var _ = Page.AcceptAllDialogs())
            {
                await Page.GetByRole(AriaRole.Button, new() { Name = "Ja" }).ClickAsync();

            }

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step(" VACs are displayed in the Gerelaterede VACs section in afhnadeling form");

            await Expect(sectionLocator.GetByRole(AriaRole.Heading, new() { Name = "Gerelateerde VAC" })).ToBeVisibleAsync();

            var label = sectionLocator.Locator("label").Filter(new() { HasText = "Heb ik een ROVA-pas nodig voor de gft-container?" });
            await Expect(label).ToBeVisibleAsync();

        }

        [TestMethod("Scenario 2: Multiple ContactMoments")]
        public async Task ContactMomentMultipleScenario2()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("Add a note");
            var note = "test multiple contactmoment scenario 2";
            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note);

            await Step("And user enters \"999993653\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("999993653");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();

            await Step(" user click through first available Zaken in the Zaken tab");
            await Page.GetByRole(AriaRole.Tab, new() { Name = "Zaken" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Details 100-" }).ClickAsync();
            await Expect(Page.GetByText("Algemeen").First).ToBeVisibleAsync();

            await Step("Start another contact moment");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Nieuw", Exact = true }).ClickAsync();

            await Step("When user click Bedrijf from the menu item");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And user enters '69599068' in the field KVK-nummer of vestigingsnummer");

            await Page.Company_KvknummerInput().FillAsync("69599068");

            await Step("And clicks the search button");

            await Page.Company_KvknummerSearchButton().ClickAsync();

            await Step("go to tab Nieuws en werkinstructies, and link 1 item to this Contactmoment ");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Nieuws en werkinstructies" }).ClickAsync();
            await Page.GetByRole(AriaRole.Article).Filter(new() { HasText = "28-03-2025, 12:03Bewerkt op" }).GetByLabel("Opslaan bij contactmoment").CheckAsync();

            await Step("Add a note");
            var note2 = "test multiple contactmoment scenario relate niews werkinstructies";
            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note2);

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();
            await Expect(Page.GetByText("Afhandeling").First).ToBeVisibleAsync();


            await Step("check if the right Bedrijf is linked, check that the Nieuws-item is linked ");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Gerelateerd nieuwsbericht" })).ToBeVisibleAsync();
            await Expect(Page.Locator("span").Filter(new() { HasText = "Latest release on dev.kiss-" })).ToBeVisibleAsync();

            await Step("user click on Annuleren");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And confirms a pop-up window with the message ‘Weet je zeker dat je het contactmoment wilt annuleren? Alle gegevens worden verwijderd.’");
            using (var _ = Page.AcceptAllDialogs())
            {
                await Page.GetByRole(AriaRole.Button, new() { Name = "Ja" }).ClickAsync();

            }

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("verify that the data from first contactmoment is displayed");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Klant" })).ToBeVisibleAsync();
            await Expect(Page.Locator("span").Filter(new() { HasText = "Suzanne Moulin" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Gerelateerde zaak" })).ToBeVisibleAsync();
            await Expect(Page.Locator("span").Filter(new() { HasText = "100-2025" })).ToBeVisibleAsync();
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Notitie" })).ToHaveValueAsync(note);

        }


    }
}

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

    }
}

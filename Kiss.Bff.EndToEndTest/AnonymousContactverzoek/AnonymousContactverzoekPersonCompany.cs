
using Kiss.Bff.EndToEndTest.AnonymousContactmoment.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactverzoek.Helpers;
using Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers;
using Kiss.Bff.EndToEndTest.Helpers;
using Kiss.Bff.EndToEndTest.Infrastructure.ApiClients.Dtos;


namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentVerzoek
{

    [TestClass]
    public class AnonymousContactVerzoekPersonCompany : KissPlaywrightTest
    {
        [TestMethod("1.Contactverzoek form prefill for BSN 999993264")]
        public async Task AnonymousContactVerzoekformBSN()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"999993653\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("999993264");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then user is navigated to Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").First.ClickAsync();

            await Step("And enters 'test automation contactverzoek' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And field Voornaam hasvalue Fatima Zouilikha");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Voornaam" })).ToHaveValueAsync("Fatima Zouilikha");

            await Step("And field Achternaam has value Moulin ");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Achternaam" })).ToHaveValueAsync("Djibet");

            await Step("And field Telefoonnummer 1 has value Moulin ");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Telefoonnummer 1" })).ToHaveValueAsync("0612345678");

            await Step("And field E-mailadres has value SuzyM.OK26@syps.nl ");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "E-mailadres" })).ToHaveValueAsync("fatimaz@syps.nl");

        }

        [TestMethod("2.Contactverzoek form prefill for BSN form submission")]
        public async Task AnonymousContactVerzoekcontactverzoekBSN()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"999993264\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("999993264");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then user is navigated to Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").First.ClickAsync();

            await Step("And enters 'test automation contactverzoek' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation contactverzoek");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(["Balie"]);

            await Step("And user clicks on Opslaan button");

            var klantContactPostResponse = await Page.RunAndWaitForResponseAsync(async () =>
            {
                await Page.GetOpslaanButton().ClickAsync();
            },
                response => response.Url.Contains("/postklantcontacten")
            );

            var klantContactUuid = await klantContactPostResponse.JsonAsync<UuidDto>();

            // Register cleanup
            RegisterCleanup(async () =>
            {
                await CleanupPostKlantContactenCall(klantContactUuid.Value);
            });

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"999993264\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("999993264");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then user is navigated to Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

            await Step("And user navigates to the contactverzoeken tab to view the created contact request");

            await Page.GetByRole(AriaRole.Tab, new() { Name = "Contactverzoeken" }).ClickAsync();

            await Step("And contactverzoek details are displayed");

            await Page.Locator("summary").Filter(new() { HasText = "automation test" }).First.PressAsync("Enter");

            await Expect(Page.GetByRole(AriaRole.Definition).Filter(new() { HasText = "fatimaz@syps.nl" })).ToBeVisibleAsync();
        }

        [TestMethod("3.Contactverzoek form prefill for company, Vestigingsnr 000055679269")]
        public async Task AnonymousContactVerzoekformVestiging()
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
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").First.ClickAsync();

            await Step("And enters 'test automation contactverzoek' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And field organisatie has value Voorlopige Deponering B.V.");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Organisatie" })).ToHaveValueAsync("Voorlopige Deponering B.V."); // waits up to 10 seconds

            await Step("And field Telefoonnummer 1 has value Moulin ");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "Telefoonnummer 1" })).ToHaveValueAsync("0885213577");

            await Step("And field E-mailadres has value SuzyM.OK26@syps.nl ");
            await Expect(Page.GetByRole(AriaRole.Textbox, new() { Name = "E-mailadres" })).ToHaveValueAsync("voorlopige.deponering@syps.nl");
        }

        [TestMethod("4.Contactverzoek form prefill for company with Vestigingsnr")]
        public async Task AnonymousContactVerzoekVestigingContactVerzoek()
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
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").First.ClickAsync();

            await Step("And enters 'test automation contactverzoek' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(["Balie"]);

            await Step("And user clicks on Opslaan button");

            var klantContactPostResponse = await Page.RunAndWaitForResponseAsync(async () =>
            {
                await Page.GetOpslaanButton().ClickAsync();
            },
                response => response.Url.Contains("/postklantcontacten")
            );

            var klantContactUuid = await klantContactPostResponse.JsonAsync<UuidDto>();

            // Register cleanup
            RegisterCleanup(async () =>
            {
                await CleanupPostKlantContactenCall(klantContactUuid.Value);
            });

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And user enters “000055679269” in Vestigingsnummer field");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();
            await Page.Company_KvknummerInput().FillAsync("000055679269");

            await Step("And clicks the search button");
            await Page.Company_KvknummerSearchButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("user is navigated to Bedrijfinformatie page of Test BV Donald Nevenvestiging");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Bedrijfsinformatie" })).ToBeVisibleAsync();

            await Step("And user navigates to the contactverzoeken tab to view the created contact request");
            await Page.GetByRole(AriaRole.Tab, new() { Name = "Contactverzoeken" }).ClickAsync();

            await Step("And contactverzoek details are displayed");
            await Page.Locator("summary").Filter(new() { HasText = "automation test" }).First.PressAsync("Enter");
            await Expect(Page.GetByRole(AriaRole.Definition).Filter(new() { HasText = "voorlopige.deponering@syps.nl" })).ToBeVisibleAsync();
        }

        [TestMethod("5.Cancel a Contact verzoek Creation for BSN 999993264")]
        public async Task AnonymousContactVerzoekformCancel()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"999993264\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("999993264");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("Then user is navigated to Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").First.ClickAsync();

            await Step("And enters 'test automation contactverzoek' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation contactverzoek");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(["Balie"]);

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

        [TestMethod("6. Contact verzoek Creation for company cancelled")]
        public async Task AnonymousContactVerzoekVestigingCancel()
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
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").First.ClickAsync();

            await Step("And enters 'test automation contactverzoek' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(["Balie"]);

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
        private async Task CleanupPostKlantContactenCall(string klantContactUuid)
        {
            var actorKlantContact = await OpenKlantApiClient.GetActorKlantContact(klantContactUuid);
            await OpenKlantApiClient.DeleteActor(actorKlantContact.Actor.Uuid);
            await OpenKlantApiClient.DeleteKlantContact(actorKlantContact.KlantContact.Uuid);
        }

    }
}

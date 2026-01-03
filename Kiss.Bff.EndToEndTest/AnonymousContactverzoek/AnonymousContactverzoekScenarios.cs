using Kiss.Bff.EndToEndTest.AnonymousContactmoment.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactverzoek.Helpers;
using Kiss.Bff.EndToEndTest.Infrastructure.ApiClients.Dtos;
using Microsoft.Playwright;


namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentVerzoek
{

    [TestClass]
    public class AnonymousContactVerzoekScenarios : KissPlaywrightTest
    {
        [TestMethod("1. Contactverzoek creation and search via telefoonnummer for an afdeling")]
        public async Task AnonymousContactVerzoekTelefoonAfdeling()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And enter '0617138555' in field telefoonnummer");
            await Page.GetTelefoonnummerTextbox().FillAsync("0617138555");

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

            // Clean up later
            RegisterCleanup(async () =>
            {
                await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            });

            await Step("user starts a new Contactmoment and navigates to contactverzoek tab");
            await Page.CreateNewContactmomentAsync();
            await Page.GetContactverzoekenLink().ClickAsync();

            await Step("enter phone number in field Telefoonnummer of e-mailadres");
            await Page.GetContactverzoekSearchBar().FillAsync("0617138555");

            await Step("And clicks the Zoeken button");
            await Page.GetZoekenButton().ClickAsync();

            await Step("And contactverzoek details are displayed");
            var matchingRow = Page.Locator("table.overview tbody tr").Filter(new()
            {
                Has = Page.GetByText("automation test specific vraag")
            });

            await matchingRow.First.GetByRole(AriaRole.Button).PressAsync("Enter");

            var contactDetails = Page.GetByText("0617138555").First;
            await contactDetails.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");
        }

        [TestMethod("2. Contactverzoek creation and search via email for an afdeling")]
        public async Task AnonymousContactVerzoekEmailAfdeling()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And enter 'testautomation@example.com' in field e-mailadres");
            await Page.GetEmailTextbox().FillAsync("testautomation@info.nl");

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

            // Register cleanup
            RegisterCleanup(async () =>
            {
                await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            });

            await Step("user starts a new Contactmoment and navigates to contactverzoek tab");
            await Page.CreateNewContactmomentAsync();
            await Page.GetContactverzoekenLink().ClickAsync();

            await Step("enter email address in field Telefoonnummer of e-mailadres");
            await Page.GetEmailTextbox().FillAsync("testautomation@info.nl");

            await Step("And clicks the Zoeken button");

            await Page.GetZoekenButton().ClickAsync();

            await Step("And contactverzoek details are displayed");

            var matchingRow = Page.Locator("table.overview tbody tr").Filter(new()
            {
                Has = Page.GetByText("automation test specific vraag")
            });

            await matchingRow.First.GetByRole(AriaRole.Button).PressAsync("Enter");

            var contactDetails = Page.GetByText("testautomation@info.nl").First;
            await contactDetails.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");
        }

        [TestMethod("3. Contactverzoek creation and search via telefoonnummer for group")]

        public async Task AnonymousContactVerzoekTelefoonGroup()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And selects group radiobutton");
            await Page.GetGroupRadioButton().ClickAsync();

            await Step("And user selects 'Brandweer' from dropdown list of field group");
            await Page.GetGroupCombobox().FillAsync("Communicatieadviseur");
            await Page.GetByText("Communicatieadviseurs").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And enter '0617138556' in field telefoonnummer");
            await Page.GetTelefoonnummerTextbox().FillAsync("0617138556");

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

            // Register cleanup
            RegisterCleanup(async () =>
            {
                await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            });

            await Step("user starts a new Contactmoment and navigates to contactverzoek tab");
            await Page.CreateNewContactmomentAsync();
            await Page.GetContactverzoekenLink().ClickAsync();

            await Step("enter phone number in field Telefoonnummer of e-mailadres");
            await Page.GetContactverzoekSearchBar().FillAsync("0617138556");

            await Step("And clicks the Zoeken button");

            await Page.GetZoekenButton().ClickAsync();

            await Step("And contactverzoek details are displayed");

            var matchingRow = Page.Locator("table.overview tbody tr").Filter(new()
            {
                Has = Page.GetByText("automation test")
            });

            await matchingRow.First.GetByRole(AriaRole.Button).PressAsync("Enter");

            var contactDetails = Page.GetByText("0617138556").First;
            await contactDetails.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");
        }

        [TestMethod("4. Contactverzoek creation and search via email for group")]
        public async Task AnonymousContactVerzoekEmailGroup()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And selects group radiobutton");
            await Page.GetGroupRadioButton().ClickAsync();

            await Step("And user selects 'Brandweer' from dropdown list of field group");
            await Page.GetGroupCombobox().FillAsync("Communicatieadviseur");
            await Page.GetByText("Communicatieadviseurs").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And enter 'testautomation@example.com' in field e-mailadres");
            await Page.GetEmailTextbox().FillAsync("testautomation@example.com");

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

            // Register cleanup
            RegisterCleanup(async () =>
            {
                await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            });

            await Step("user starts a new Contactmoment and navigates to contactverzoek tab");
            await Page.CreateNewContactmomentAsync();
            await Page.GetContactverzoekenLink().ClickAsync();

            await Step("enter email address in field Telefoonnummer of e-mailadres");
            await Page.GetEmailTextbox().FillAsync("testautomation@example.com");

            await Step("And clicks the Zoeken button");

            await Page.GetZoekenButton().ClickAsync();

            await Step("And contactverzoek details are displayed");

            var matchingRow = Page.Locator("table.overview tbody tr").Filter(new()
            {
                Has = Page.GetByText("automation test")
            });

            await matchingRow.First.GetByRole(AriaRole.Button).PressAsync("Enter");

            var contactDetails = Page.GetByText("testautomation@example.com").First;
            await contactDetails.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");
        }

        [TestMethod("5. Validation of phone number field 1 in contactverzoek form")]
        public async Task AnonymousContactVerzoekTelefoonValidation()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And the Contactverzoek section of the Afhandeling screen is loaded");
            await Expect(Page.GetByText("Contactverzoek maken voor")).ToBeVisibleAsync();

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(["Balie"]);

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("message as vul minimal een telefoonnummer of een e-mailadres in is displayed");
            await Expect(Page.GetTelefoonnummer1field()).ToHaveJSPropertyAsync("validationMessage", "Vul minimaal een telefoonnummer of een e-mailadres in");

        }

        [TestMethod("6. Validation of telefoon nummer 2 field in contactverzoek form")]
        public async Task AnonymousContactVerzoekTelefoon2Validation()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And the Contactverzoek section of the Afhandeling screen is loaded");
            await Expect(Page.GetByText("Contactverzoek maken voor")).ToBeVisibleAsync();

            await Step("And user fills in 'automation test specific vraag' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(["Balie"]);

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("message as vul minimal een telefoonnummer of een e-mailadres in is displayed");
            await Expect(Page.GetTelefoonnummer1field()).ToHaveJSPropertyAsync("validationMessage", "Vul minimaal een telefoonnummer of een e-mailadres in");

            await Step("enter “0617” in field telefoonnummer 2 ");
            await Page.GetTelefoonnummer2field().FillAsync("0617");

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("message as vul een geldig telefoonnummer in. is displayed");
            await Expect(Page.GetTelefoonnummer2field()).ToHaveJSPropertyAsync("validationMessage", "Vul een geldig telefoonnummer in.");

            await Step("enter “0617138777” in field telefoonnummer 2 ");
            await Page.GetTelefoonnummer2field().FillAsync("0617138777");

            await Step("And user clicks on Opslaan button");

            var klantContactPostResponse = await Page.RunAndWaitForResponseAsync(async () =>
            {
                await Page.GetOpslaanButton().ClickAsync();
            },
                response => response.Url.Contains("/postklantcontacten")
            );

            // Register cleanup
            RegisterCleanup(async () =>
            {
                await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            });

            await Step("Then message as 'Het contactmoment is opgeslagen' is displayed");

            await Expect(Page.GetContactVerzoekSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

        [TestMethod("7. Validation of Email field in contactverzoek form")]
        public async Task AnonymousContactVerzoekEmailValidation()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And Afdeling radiobutton is pre-selected");
            var radio = Page.GetAfdelingRadioButton();
            await radio.CheckAsync();
            Assert.IsTrue(await radio.IsCheckedAsync());

            await Step("And user selects 'parkeren' from dropdown list of field afdeling");
            await Page.GetAfdelingCombobox().FillAsync("Parkeren");
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And enters 'test automation' in interne toelichting voor medewerker");
            await Page.GetInterneToelichtingTextbox().FillAsync("test automation");

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("And user fills in 'Hoe gaat het' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test specific vraag");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(["Balie"]);

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("message as vul minimal een telefoonnummer of een e-mailadres in is displayed");
            await Expect(Page.GetTelefoonnummer1field()).ToHaveJSPropertyAsync("validationMessage", "Vul minimaal een telefoonnummer of een e-mailadres in");

            await Step("enter automation in E-mailadres ");
            await Page.GetEmailfield().FillAsync("automation");

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("message as vul een geldig e-mail adres in is displayed");
            await Expect(Page.GetEmailfield()).ToHaveJSPropertyAsync("validationMessage", "Vul een geldig e-mailadres in.");

            await Step("enter automation@info.nl in field emailfield ");
            await Page.GetEmailfield().FillAsync("automation@info.nl");

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then message as 'Het contactmoment is opgeslagen' is displayed");

            await Expect(Page.GetContactVerzoekSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");

        }

        [TestMethod("8. Contactverzoek creation and search via telefoonnummer for medewerker")]

        public async Task AnonymousContactVerzoekTelefoonMedewerker()
        {
            await Step("Given the user is on the Startpagina");
            await Page.GotoAsync("/");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And clicks on Contactverzoek-pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("And selects group radiobutton");
            await Page.GetMedewerkerRadioButton().ClickAsync();

            await Step("And user selects 'Sytske' from dropdown list of medewerker group");

            await Page.GetByRole(AriaRole.Combobox, new() { Name = "Medewerker" }).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox, new() { Name = "Medewerker" }).FillAsync("sytsk");
            await Page.GetByText("Sytske de eSuiteBeheerder").ClickAsync();

            await Step("And click on afronden");
            await Page.GetAfrondenButton().ClickAsync();

            await Step("And the Contactverzoek section of the Afhandeling screen is loaded");
            await Expect(Page.GetByText("Contactverzoek maken voor")).ToBeVisibleAsync();

            await Step("And user fills in 'automation test' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("automation test");

            await Step("select channel from the list");
            await Page.GetByLabel("Kanaal").SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("error message as 'Please select an item in the list.' is displayed for the field Afdeling / groep.");
            await Expect(Page.GetAfdelingTextbox()).ToHaveJSPropertyAsync("validationMessage", "Please select an item in the list.");

            await Step("User selects 'Afdeling: Communicatie' from dropdown list of field Afdeling / groep");

            await Page.GetByLabel("Afdeling / groep Afdeling:")
            .SelectOptionAsync(new SelectOptionValue { Label = "Afdeling: Communicatie" });

            await Step(" user fills in interne toelichting voor medewerker");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Interne toelichting voor" }).FillAsync("test interne toelichting");

            await Step("And enter '0617138555' in field telefoonnummer");
            await Page.GetTelefoonnummerTextbox().FillAsync("0617178888");

            await Step("And user clicks on Opslaan button");

            var klantContactPostResponse = await Page.RunAndWaitForResponseAsync(async () =>
            {
                await Page.GetOpslaanButton().ClickAsync();
            },
                response => response.Url.Contains("/postklantcontacten")
            );

            // Register cleanup
            RegisterCleanup(async () =>
            {
                await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            });

            await Step("user starts a new Contactmoment and navigates to contactverzoek tab");
            await Page.CreateNewContactmomentAsync();
            await Page.GetContactverzoekenLink().ClickAsync();

            await Step("enter phone number in field Telefoonnummer of e-mailadres");
            await Page.GetContactverzoekSearchBar().FillAsync("0617178888");

            await Step("And clicks the Zoeken button");

            await Page.GetZoekenButton().ClickAsync();

            await Step("And contactverzoek details are displayed");
            var matchingRow = Page.Locator("table.overview tbody tr").Filter(new()
            {
                Has = Page.GetByText("automation test")
            });

            await matchingRow.First.GetByRole(AriaRole.Button).PressAsync("Enter");

            var contactDetails = Page.GetByText("0617178888").First;
            await contactDetails.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            Assert.IsTrue(await contactDetails.IsVisibleAsync(), "The contactverzoek details are not displayed.");
        }
    }


}



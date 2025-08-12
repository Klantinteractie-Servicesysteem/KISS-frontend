
using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;

namespace Kiss.Bff.EndToEndTest.AfhandelingForm
{

    [TestClass]
    public class AfhandelingFormScenarios : KissPlaywrightTest
    {

        [TestMethod("1. Validation of Text in Notitieblok")]
        public async Task TestValidationOfTextInNotitieblok()
        {
            await Step("Given the user is on KISS home page ");

            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");

            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("When user enters “test notitieblok” in Notitieblok");

            var note = "test notitieblok";

            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note);

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("Then Afhandeling form has value as “test notitieblok” in field Notitie");

            await Expect(Page.GetAfhandelingNotitieTextBox()).ToHaveValueAsync(note);
        }

        [TestMethod("2. Error validation of Afhandeling form")]
        public async Task TestErrorValidationOfAfhandelingForm()
        {
            await Step("Precondition: Field Notitieblok has value 'test notitieblok'");

            await Page.GotoAsync("/");

            await Page.GetNieuwContactmomentButton().ClickAsync();

            var note = "test notitieblok";
            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note);

            await Step("Given user clicks on Afronden button on the Notes-Contactverzoek-Pane");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("When Afhandeling form is displayed");

            await Expect(Page.GetAfhandelingForm()).ToBeVisibleAsync();

            await Step("And user clicks on Opslaan button");

            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then error message as 'Please fill in this field.' is displayed for the field specific vraag");

            await Expect(Page.GetSpecificVraagField()).ToHaveJSPropertyAsync("validationMessage", "Please fill in this field.");

            await Step("And user enters 'Test' in field specific vraag");

            await Page.GetSpecificVraagField().FillAsync("Test");

            await Step("And clicks on Opslaan button");

            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then error message as 'Please select an item in the list.' is displayed for the field Kanaal");

            await Expect(Page.GetKanaalField()).ToHaveJSPropertyAsync("validationMessage", "Please select an item in the list.");

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetKanaalField().ClickAsync();

            await Page.GetKanaalField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("And clicks on Opslaan button");

            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then error message as 'Please select an item in the list.' is displayed for the field Afhandeling");

            await Expect(Page.GetAfhandelingField()).ToHaveJSPropertyAsync("validationMessage", "Please select an item in the list.");

            await Step("And user selects 'TESTtest' from dropdown list of Afhandeling");

            await Page.GetAfhandelingField().ClickAsync();

            await Page.GetAfhandelingField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "TESTtest" } }); ;

            await Step("And clicks on Opslaan button");

            await Page.GetOpslaanButton().ClickAsync();

            await Step("Then error message as 'Please fill in this field.' is displayed for the field Afdeling");

            await Expect(Page.GetAfdelingVoorField()).ToHaveJSPropertyAsync("validationMessage", "Please fill in this field.");

            await Step("And user selects 'parkeren' from the dropdown list");

            await Page.GetAfdelingVoorField().ClickAsync();
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And clicks on Opslaan button");

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

            await Step("Then Afhandeling form is successfully submitted");

            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");

        }

        [TestMethod("3. Successful submission of Afhandeling form")]
        public async Task TestSuccessfulSubmissionOfAfhandelingForm()
        {
            await Step("Precondition: In Afdelingen-register, there is a Afdeling Parkeren present");

            await Page.GotoAsync("/");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Contactverzoekformulieren afdelingen" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Cell, new() { Name = "Parkeren" }).First).ToBeVisibleAsync();

            await Step("Precondition: Under Beheer, there is a Gespreksresultaat 'Zelfstandig afgehandeld'");

            await Page.GotoAsync("/");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" }).ClickAsync();
            await Page.GetByRole(AriaRole.Link, new() { Name = "Gespreksresultaten" }).ClickAsync();
            await Expect(Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = "Zelfstandig afgehandeld" })).ToBeVisibleAsync();

            await Step("Precondition: Field Notitieblok has value 'test notitieblok'");

            await Page.GotoAsync("/");

            await Page.GetNieuwContactmomentButton().ClickAsync();

            var note = "test notitieblok";
            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note);

            await Step("Given user clicks on Afronden button on the Notes-Contactverzoek-Pane");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("And Afhandeling form is displayed");

            await Expect(Page.GetAfhandelingForm()).ToBeVisibleAsync();

            await Step("When user enters value 'hoe gaat het' in field Specifieke vraag");

            await Page.GetSpecificVraagField().FillAsync("hoe gaat het");

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetKanaalField().ClickAsync();

            await Page.GetKanaalField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("And value 'Zelfstandig afgehandeld' in field Afhandeling");

            await Page.GetAfhandelingField().ClickAsync();
            await Page.GetAfhandelingField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Zelfstandig afgehandeld" } });

            await Step("And selects value 'Parkeren' in field Afdeling");

            await Page.GetAfdelingVoorField().ClickAsync();
            await Page.GetByText("Parkeren").ClickAsync();

            await Step("And clicks on Opslaan button");

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

            await Step("Then message as 'Het contactmoment is opgeslagen' is displayed on the Startpagina");

            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

    }
}

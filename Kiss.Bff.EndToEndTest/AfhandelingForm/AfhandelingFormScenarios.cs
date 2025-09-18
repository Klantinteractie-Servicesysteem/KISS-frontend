
using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;
using System.Text.Json;

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

            var elementHandle = await Page.GetSpecificVraagField().ElementHandleAsync();
            var validationMessage = await elementHandle.EvaluateAsync<string>("el => el.validationMessage");
            Assert.IsFalse(string.IsNullOrEmpty(validationMessage), "Expected a validation message, but none was found.");

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

            var elementHandle2 = await Page.GetAfdelingVoorField().ElementHandleAsync();
            var validationMessage2 = await elementHandle2.EvaluateAsync<string>("el => el.validationMessage");
            Assert.IsFalse(string.IsNullOrEmpty(validationMessage2), "Expected a validation message, but none was found.");

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

        // When selecting a search result, KISS will try to find that item in OverigeObjecten
        // If it finds it there and it has an afdeling, then this afdeling will be prefilled in the Afhandeling form of the contactmoment
        // However in OverigeObjecten the name of the afdelingen property can be 'afdelingNaam' or 'afdelingnaam'. It should work with both.

        [TestMethod("4. Prefilling Afdeling Field Based on Selected article")]
        // To make it easier to analyse the result, make the search so specific that it only returns one result
        [DataRow("VAC This VAC has a property afdelingnaam with lower case n", "VAC This VAC has a property afdelingnaam with lower case n", "Advies, support en kennis (ASK)", true)]
        [DataRow("This VAC has a property afdelingNaam with Upper Case N", "VAC This VAC has a property afdelingNaam with Upper Case N", "Advies, support en kennis (ASK)", false)]
        [DataRow("Wegenverkeerswet", "Kennisbank Wegenverkeerswet, ontheffing", "Publiekscontacten Vergunningen", true)]
        [DataRow("Wegenverkeerswet - afdelingNaam", "Kennisbank Wegenverkeerswet, ontheffing - afdelingNaam", "Publiekscontacten Vergunningen", false)]
        public async Task When_SearchResultWithAfdelingSelected_Expect_AfhandelingFormAfdelingPrefilled(string searchTerm, string resultName, string expectedAfdeling, bool expectLowerCaseAfdelingPropertyName)
        {
            await Step("Given the user is on KISS home page ");
            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("When user enters Note in Notitieblok");
            await Page.GetContactmomentNotitieblokTextbox().FillAsync("Note field for test");

            await Step($"And user search and fill '{searchTerm}'");

            var searchResponse = await Page.RunAndWaitForResponseAsync(async () =>
            {
                await Page.SearchAndSelectItem(searchTerm, resultName, exact: true);
            },
            response => response.Url.Contains("api/elasticsearch/.ent-search-engine-documents-engine-crawler,search-kennisbank,search-smoelenboek,search-vac/_search")
            );

            // We will inspect the data from elasticsearch. This tells us whether the source data uses an afdelingnaam property with uppercase or lowercase
            // If we don't do this we won't know if the test succeeds because KISS handles both property names correctly
            // or because the testdata just happens is not representative (anymore) and only contains records that all have afdelingnaam properties with the same casing
            var json = await searchResponse.JsonAsync();
            var hit = json.Value.GetProperty("hits").GetProperty("hits")[0];
            var source = hit.GetProperty("_source");

            // Determine if it's VAC or Kennisbank and get the first afdeling
            JsonElement firstAfdeling;
            if (source.TryGetProperty("VAC", out var vac))
            {
                firstAfdeling = vac.GetProperty("afdelingen")[0];
            }
            else
            {
                firstAfdeling = source.GetProperty("Kennisbank").GetProperty("afdelingen")[0];
            }

            // Check for both property naming conventions
            var hasLowerCase = firstAfdeling.TryGetProperty("afdelingnaam", out var afdelingnaamLower);
            var hasUpperCase = firstAfdeling.TryGetProperty("afdelingNaam", out var afdelingnaamUpper);

            // If we expect that this testcase used the all lowercase afdelingnaam property 
            // then afdelingnaam should have a value and afdelingNaam should not have a value
            if (expectLowerCaseAfdelingPropertyName)
            {
                Assert.IsTrue(hasLowerCase, "Expected to find 'afdelingnaam' (lowercase) property");
                Assert.IsFalse(hasUpperCase, "Expected NOT to find 'afdelingNaam' (uppercase) property");
                Assert.AreEqual(expectedAfdeling, afdelingnaamLower.GetString());
            }
            else
            {
                Assert.IsFalse(hasLowerCase, "Expected NOT to find 'afdelingnaam' (lowercase) property");
                Assert.IsTrue(hasUpperCase, "Expected to find 'afdelingNaam' (uppercase) property");
                Assert.AreEqual(expectedAfdeling, afdelingnaamUpper.GetString());
            }

            await Step("Click the Afronden button");
            await Page.GetAfrondenButton().ClickAsync();

            await Step($"Then Afhandeling form has value as '{expectedAfdeling}' in field Afdeling");
            await Expect(Page.GetAfdelingVoorField()).ToHaveValueAsync(expectedAfdeling);

            await Step("And user selects first option in field Kanaal");
            await Page.GetKanaalField().SelectOptionAsync(new SelectOptionValue { Index = 0 });

            await Step("And select an Afhandeling");
            var options = await Page.GetAfhandelingField().EvaluateAsync<string[]>("el => Array.from(el.options).map(o => o.text)");
            var selectedIndex = options.Length > 0 && options[0] == "Contactverzoek gemaakt" ? 1 : 0;
            await Page.GetAfhandelingField().SelectOptionAsync(new SelectOptionValue { Index = selectedIndex });

            await Step("And clicks on Opslaan button");
            var klantContactPostResponse = await Page.RunAndWaitForResponseAsync(
                async () => await Page.GetOpslaanButton().ClickAsync(),
                response => response.Url.Contains("/postklantcontacten")
            );

            // Clean up later
            RegisterCleanup(async () =>
            {
                await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            });

            await Step("Then message as 'Het contactmoment is opgeslagen' is displayed");
            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

    }
}
using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
using Kiss.Bff.EndToEndTest.AfhandelingForm.Models;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;
using System.Text.Json;
using Kiss.Bff.EndToEndTest.AnonymousContactverzoek.Helpers;

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

        // NOTE: These scenarios (4a, 4b, 4c, 4d) were separated from a single DataRow test because they randomly fail when run in DataRow sequence
        [TestMethod("4a. Prefilling Afdeling Field Based on Selected article - afdelingnaam lower case n")]
        public async Task When_SearchResultWithAfdelingSelected_Expect_AfhandelingFormAfdelingPrefilled_4a()
        {
            // Workaround: Ensure the search result is selected correctly for VAC with afdelingnaam (lowercase n)
            await RunAfdelingPrefillScenario_SelectVACWithLowercaseAfdelingnaam(
                "This VAC has a property afdelingnaam with lower case n",
                "VAC This VAC has a property afdelingnaam with lower case n",
                "Advies, support en kennis (ASK)",
                true
            );
        }

        [TestMethod("4b. Prefilling Afdeling Field Based on Selected article - afdelingNaam upper case N")]
        public async Task When_SearchResultWithAfdelingSelected_Expect_AfhandelingFormAfdelingPrefilled_4b()
        {
            await RunAfdelingPrefillScenario(
                "This VAC has a property afdelingNaam with Upper Case N",
                "VAC This VAC has a property afdelingNaam with Upper Case N",
                "Advies, support en kennis (ASK)",
                false
            );
        }

        [TestMethod("4c. Prefilling Afdeling Field Based on Selected article - Wegenverkeerswet")]
        public async Task When_SearchResultWithAfdelingSelected_Expect_AfhandelingFormAfdelingPrefilled_4c()
        {
            await RunAfdelingPrefillScenario(
                "Wegenverkeerswet",
                "Kennisbank Wegenverkeerswet, ontheffing",
                "Publiekscontacten Vergunningen",
                true
            );
        }

        [TestMethod("4d. Prefilling Afdeling Field Based on Selected article - Wegenverkeerswet afdelingNaam")]
        public async Task When_SearchResultWithAfdelingSelected_Expect_AfhandelingFormAfdelingPrefilled_4d()
        {
            await RunAfdelingPrefillScenario(
                "Wegenverkeerswet - afdelingNaam",
                "Kennisbank Wegenverkeerswet, ontheffing - afdelingNaam",
                "Publiekscontacten Vergunningen",
                false
            );
        }

        private async Task RunAfdelingPrefillScenario(string searchTerm, string resultName, string expectedAfdeling, bool expectLowerCaseAfdelingPropertyName)
        {
            await Step("Given the user is on KISS home page ");
            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("When user enters Note in Notitieblok");
            await Page.GetContactmomentNotitieblokTextbox().FillAsync("Note field for test");

            await Step($"And user search and fill '{searchTerm}'");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var searchResponse = await Page.RunAndWaitForResponseAsync(async () =>
            {
                await Page.SearchAndSelectItem(searchTerm, resultName, exact: true);
                await Page.WaitForTimeoutAsync(5000);
            },
            response => response.Url.Contains("api/elasticsearch") && response.Url.Contains("_search")
            );

            var deserializedSearchResponse = await searchResponse.JsonAsync<Rootobject>();

            // Find the hit with the matching title
            var normalizedResultName = resultName;
            // Remove "Kennisbank " or "VAC " prefix if present
            if (normalizedResultName.StartsWith("Kennisbank "))
            {
                normalizedResultName = normalizedResultName.Substring("Kennisbank ".Length);
            }
            else if (normalizedResultName.StartsWith("VAC "))
            {
                normalizedResultName = normalizedResultName.Substring("VAC ".Length);
            }

            var matchingHit = deserializedSearchResponse.hits.hits
                .FirstOrDefault(hit =>
                    // Match top-level title for both Kennisbank and VAC
                    (hit._source.title != null && hit._source.title == normalizedResultName) ||
                    // Match VAC title (if nested VAC object has title property)
                    (hit._source.VAC != null && hit._source.VAC.title != null && hit._source.VAC.title == normalizedResultName) ||
                    // Match Kennisbank title
                    (hit._source.Kennisbank != null && hit._source.Kennisbank.title != null &&
                        (hit._source.Kennisbank.title == normalizedResultName ||
                         hit._source.Kennisbank.title.Replace(",", "") == normalizedResultName.Replace(",", "")))
                );

            Assert.IsNotNull(matchingHit, $"No search result found with title '{resultName}'.");

            Afdelingen firstAfdeling;
            // Use the correct object for afdeling extraction
            if (matchingHit._source.VAC != null && matchingHit._source.VAC.afdelingen != null && matchingHit._source.VAC.afdelingen.Count() > 0)
            {
                firstAfdeling = matchingHit._source.VAC.afdelingen[0];
            }
            else if (matchingHit._source.Kennisbank != null && matchingHit._source.Kennisbank.afdelingen != null && matchingHit._source.Kennisbank.afdelingen.Count() > 0)
            {
                firstAfdeling = matchingHit._source.Kennisbank.afdelingen[0];
            }
            else
            {
                Assert.Fail("No afdelingen found in the search result.");
                return;
            }

            var afdelingnaamLower = firstAfdeling.afdelingnaam;
            var afdelingnaamUpper = firstAfdeling.afdelingNaam;

            if (expectLowerCaseAfdelingPropertyName)
            {
                Assert.IsNotNull(afdelingnaamLower, "Expected 'afdelingnaam' (lowercase) to have a value");
                Assert.AreEqual(expectedAfdeling, afdelingnaamLower, $"Expected afdeling value '{expectedAfdeling}' but found '{afdelingnaamLower}'");
            }
            else
            {
                Assert.IsNotNull(afdelingnaamUpper, "Expected 'afdelingNaam' (uppercase) to have a value");
                Assert.AreEqual(expectedAfdeling, afdelingnaamUpper, $"Expected afdeling value '{expectedAfdeling}' but found '{afdelingnaamUpper}'");
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

            RegisterCleanup(async () =>
            {
                await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            });

            await Step("Then message as 'Het contactmoment is opgeslagen' is displayed");
            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

        // Add a specialized scenario runner for 4a to handle the selector issue
        private async Task RunAfdelingPrefillScenario_SelectVACWithLowercaseAfdelingnaam(string searchTerm, string resultName, string expectedAfdeling, bool expectLowerCaseAfdelingPropertyName)
        {
            await Step("Given the user is on KISS home page ");
            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("When user enters Note in Notitieblok");
            await Page.GetContactmomentNotitieblokTextbox().FillAsync("Note field for test");

            await Step($"And user search and fill '{searchTerm}'");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Capture the response during the search action
            var searchResponse = await Page.RunAndWaitForResponseAsync(
                async () =>
                {
                    await Page.SearchAndSelectItem(searchTerm, resultName, exact: true);
                    await Page.WaitForTimeoutAsync(5000);
                },
                response => response.Url.Contains("api/elasticsearch") && response.Url.Contains("_search")
            );

            var deserializedSearchResponse = await searchResponse.JsonAsync<Rootobject>();

            // Find the hit with the matching title
            var normalizedResultName = resultName;
            // Remove "Kennisbank " or "VAC " prefix if present
            if (normalizedResultName.StartsWith("Kennisbank "))
            {
                normalizedResultName = normalizedResultName.Substring("Kennisbank ".Length);
            }
            else if (normalizedResultName.StartsWith("VAC "))
            {
                normalizedResultName = normalizedResultName.Substring("VAC ".Length);
            }

            var matchingHit = deserializedSearchResponse.hits.hits
                .FirstOrDefault(hit =>
                    // Match top-level title for both Kennisbank and VAC
                    (hit._source.title != null && hit._source.title == normalizedResultName) ||
                    // Match VAC title (if nested VAC object has title property)
                    (hit._source.VAC != null && hit._source.VAC.title != null && hit._source.VAC.title == normalizedResultName) ||
                    // Match Kennisbank title
                    (hit._source.Kennisbank != null && hit._source.Kennisbank.title != null &&
                        (hit._source.Kennisbank.title == normalizedResultName ||
                         hit._source.Kennisbank.title.Replace(",", "") == normalizedResultName.Replace(",", "")))
                );

            Assert.IsNotNull(matchingHit, $"No search result found with title '{resultName}'.");

            Afdelingen firstAfdeling;
            // Use the correct object for afdeling extraction
            if (matchingHit._source.VAC != null && matchingHit._source.VAC.afdelingen != null && matchingHit._source.VAC.afdelingen.Count() > 0)
            {
                firstAfdeling = matchingHit._source.VAC.afdelingen[0];
            }
            else if (matchingHit._source.Kennisbank != null && matchingHit._source.Kennisbank.afdelingen != null && matchingHit._source.Kennisbank.afdelingen.Count() > 0)
            {
                firstAfdeling = matchingHit._source.Kennisbank.afdelingen[0];
            }
            else
            {
                Assert.Fail("No afdelingen found in the search result.");
                return;
            }

            var afdelingnaamLower = firstAfdeling.afdelingnaam;
            var afdelingnaamUpper = firstAfdeling.afdelingNaam;

            if (expectLowerCaseAfdelingPropertyName)
            {
                Assert.IsNotNull(afdelingnaamLower, "Expected 'afdelingnaam' (lowercase) to have a value");
                Assert.AreEqual(expectedAfdeling, afdelingnaamLower, $"Expected afdeling value '{expectedAfdeling}' but found '{afdelingnaamLower}'");
            }
            else
            {
                Assert.IsNotNull(afdelingnaamUpper, "Expected 'afdelingNaam' (uppercase) to have a value");
                Assert.AreEqual(expectedAfdeling, afdelingnaamUpper, $"Expected afdeling value '{expectedAfdeling}' but found '{afdelingnaamUpper}'");
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

            RegisterCleanup(async () =>
            {
                await TestCleanupHelper.CleanupPostKlantContacten(klantContactPostResponse);
            });

            await Step("Then message as 'Het contactmoment is opgeslagen' is displayed");
            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

        [TestMethod("5. Cancel from Notitieblok - Confirm with Ja")]
        public async Task When_CancelFromNotitieblokAndConfirmJa_Expect_RedirectToHomePage()
        {
            await Step("Given user is on KISS DEV environment");
            await Page.GotoAsync("/");

            await Step("And click on Nieuw contactmoment");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("When user click on Annuleren available after notice block");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And click on Ja in confirmation pop-up");
            await Page.GetConfirmationJaButton().ClickAsync();

            await Step("Then user should be redirected back to KISS HOME page");
            await Expect(Page).ToHaveURLAsync("/");
            await Expect(Page.GetNieuwContactmomentButton()).ToBeVisibleAsync();
        }

        [TestMethod("6. Cancel from Notitieblok - Confirm with Nee")]
        public async Task When_CancelFromNotitieblokAndConfirmNee_Expect_RemainOnScreen()
        {
            await Step("Given user is on KISS Home page");
            await Page.GotoAsync("/");

            await Step("And click on Nieuw contactmoment");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("When user click on Annuleren available after notice block");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And click on Nee in confirmation pop-up");
            await Page.GetConfirmationNeeButton().ClickAsync();

            await Step("Then user should remain in Nieuw contactmoment screen");
            await Expect(Page.GetContactmomentNotitieblokTextbox()).ToBeVisibleAsync();
            await Expect(Page.GetAfrondenButton()).ToBeVisibleAsync();
        }

        [TestMethod("7. Cancel from Contactverzoeken Pane")]
        public async Task When_CancelFromContactverzoekPaneAndConfirmJa_Expect_RedirectToHomePage()
        {
            await Step("Given user is on KISS home page");
            await Page.GotoAsync("/");

            await Step("And click on Nieuw contactmoment");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("And click on contactverzoeken pane");
            await Page.CreateNewcontactVerzoekAsync();

            await Step("When user click on Annuleren");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And click on Ja in confirmation pop-up");
            await Page.GetConfirmationJaButton().ClickAsync();

            await Step("Then user should be redirected back to KISS HOME page");
            await Expect(Page).ToHaveURLAsync("/");
            await Expect(Page.GetNieuwContactmomentButton()).ToBeVisibleAsync();
        }

        [TestMethod("8. Cancel from Persoonsinformatie Page")]
        public async Task When_CancelFromPersoonsinformatiePageAndConfirmJa_Expect_RedirectToHomePage()
        {
            await Step("Given user is on KISS home page");
            await Page.GotoAsync("/");

            await Step("And click on Nieuw contactmoment");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("And navigates to persoonsinformatie page of \"Suzanne Moulin\"");
            await Page.SearchAndSelectPerson("999993264");

            await Step("And click on Annuleren");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And click on Ja in confirmation pop-up");
            await Page.GetConfirmationJaButton().ClickAsync();

            await Step("Then user should be redirected back to KISS HOME page");
            await Expect(Page).ToHaveURLAsync("/");
            await Expect(Page.GetNieuwContactmomentButton()).ToBeVisibleAsync();
        }

        [TestMethod("9. Cancel from Bedrijfinformatie Page")]
        public async Task When_CancelFromBedrijfinformatiePageAndConfirmJa_Expect_RedirectToHomePage()
        {
            await Step("Given user is on KISS home page");
            await Page.GotoAsync("/");

            await Step("And click on Nieuw contactmoment");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("And navigates to Bedrijfinformatie page of company \"000037178598\"");
            await Page.SearchAndSelectCompany("000037178598");

            await Step("And click on Annuleren");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And click on Ja in confirmation pop-up");
            await Page.GetConfirmationJaButton().ClickAsync();

            await Step("Then user should be redirected back to KISS HOME page");
            await Expect(Page).ToHaveURLAsync("/");
            await Expect(Page.GetNieuwContactmomentButton()).ToBeVisibleAsync();
        }

        [TestMethod("10. Cancel from Zaak Page")]
        public async Task When_CancelFromZaakPageAndConfirmJa_Expect_RedirectToHomePage()
        {
            await Step("Given user is on KISS home page");
            await Page.GotoAsync("/");

            await Step("And click on Nieuw contactmoment");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("And navigates to Zaak page of \"ZAAK-2023-002\"");
            await Page.SearchAndSelectZaak("ZAAK-2023-002");

            await Step("And click on Annuleren");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And click on Ja in confirmation pop-up");
            await Page.GetConfirmationJaButton().ClickAsync();

            await Step("Then user should be redirected back to KISS HOME page");
            await Expect(Page).ToHaveURLAsync("/");

        }
        [TestMethod("11. Cancel from Multiple Sessions - Confirm with Ja")]
        public async Task When_CancelFromMultipleSessionsAndConfirmJa_Expect_SessionClosed()
        {
            await Step("Given user is on KISS Home page");
            await Page.GotoAsync("/");

            await Step("And click on Nieuw contactmoment");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("And click on Nieuw button TWICE to create multiple sessions");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Nieuw", Exact = true }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Page.GetByRole(AriaRole.Button, new() { Name = "Nieuw", Exact = true }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("And click on actief");
            await Page.GetActiefTab().ClickAsync();

            await Step("And three sessions should be there (1 current + 2 switchable)");
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Onbekende klant" }).And(Page.Locator(":not([disabled])"))).ToHaveCountAsync(2);

            var initialSessionCount = await Page.GetByRole(AriaRole.Button, new() { Name = "Onbekende klant" }).And(Page.Locator(":not([disabled])")).CountAsync();
            Assert.AreEqual(2, initialSessionCount, "Expected 2 switchable sessions initially");

            await Step("And user navigates to first switchable session");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Onbekende klant" }).And(Page.Locator(":not([disabled])")).First.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("When click on Annuleren");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And click on Ja in confirmation pop-up");
            await Page.GetConfirmationJaButton().ClickAsync();

            await Step("Then the session should be closed and one switchable session should still be there");

            // Click on Actief tab again to refresh the session list
            await Page.GetActiefTab().ClickAsync();

            // Wait for the session count to decrease to 1 switchable session
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Onbekende klant" }).And(Page.Locator(":not([disabled])"))).ToHaveCountAsync(1);

            var remainingSessionCount = await Page.GetByRole(AriaRole.Button, new() { Name = "Onbekende klant" }).And(Page.Locator(":not([disabled])")).CountAsync();
            Assert.AreEqual(1, remainingSessionCount, "Expected 1 remaining switchable session");
        }

        [TestMethod("12. Cancel from Multiple Sessions - Confirm with Nee")]
        public async Task When_CancelFromMultipleSessionsAndConfirmNee_Expect_AllSessionsRemain()
        {
            await Step("Given user is on KISS Home page");
            await Page.GotoAsync("/");

            await Step("And click on Nieuw contactmoment");
            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("And click on Nieuw button TWICE to create multiple sessions");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Nieuw", Exact = true }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Page.GetByRole(AriaRole.Button, new() { Name = "Nieuw", Exact = true }).ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("And click on actief");
            await Page.GetActiefTab().ClickAsync();

            await Step("And three sessions should be there (1 current + 2 switchable)");
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Onbekende klant" }).And(Page.Locator(":not([disabled])"))).ToHaveCountAsync(2);

            var initialSessionCount = await Page.GetByRole(AriaRole.Button, new() { Name = "Onbekende klant" }).And(Page.Locator(":not([disabled])")).CountAsync();
            Assert.AreEqual(2, initialSessionCount, "Expected 2 switchable sessions initially");

            await Step("And user navigates to first switchable session");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Onbekende klant" }).And(Page.Locator(":not([disabled])")).First.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("When click on Annuleren");
            await Page.GetAnnulerenButton().ClickAsync();

            await Step("And click on Nee in confirmation pop-up");
            await Page.GetConfirmationNeeButton().ClickAsync();

            await Step("Then the session should NOT be closed and all sessions should still be there");
            await Page.GetActiefTab().ClickAsync();

            // Verify that all sessions are still there (count should remain 2)
            await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Onbekende klant" }).And(Page.Locator(":not([disabled])"))).ToHaveCountAsync(2);

            var remainingSessionCount = await Page.GetByRole(AriaRole.Button, new() { Name = "Onbekende klant" }).And(Page.Locator(":not([disabled])")).CountAsync();
            Assert.AreEqual(2, remainingSessionCount, "Expected all 2 switchable sessions to remain after clicking Nee");

            await Step("And user should remain on the current session screen");

            await Expect(Page.GetAnnulerenButton()).ToBeVisibleAsync();
            await Expect(Page.GetContactmomentNotitieblokTextbox()).ToBeVisibleAsync();
        }
    }
}

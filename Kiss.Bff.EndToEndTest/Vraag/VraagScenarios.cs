
using Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers;
using Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers;
using Kiss.Bff.EndToEndTest.Common.Helpers;
using Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers;

namespace Kiss.Bff.EndToEndTest.VraagScenarios
{

    [TestClass]
    public class VraagScenarios : KissPlaywrightTest
    {

        [TestMethod("1. 2 vragen within 1 anonymous contactmoment")]
        public async Task vragenAnonymousContactMoment()
        {
            await Step("Given the user is on KISS home page ");

            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");

            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("When user enters “test vraag 1 in Notitieblok");

            var note1 = "test vraag 1";

            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note1);

            await Step("clicks on “+” icon");

            await Page.GetPlusIcon().ClickAsync();

            await Step("When user enters “test vraag 2 in Notitieblok");

            var note2 = "test vraag 2";

            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note2);

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("Then Afhandeling form has value as “test vraag 1 in field Notitie for vraag 1");
            await Expect(Page.GetAfhandelingNotitieTextBox().First).ToHaveValueAsync(note1);

            await Step("Then Afhandeling form has value as “test vraag 2 in field Notitie for vraag 2");
            await Expect(Page.GetAfhandelingNotitieTextBox().Nth(1)).ToHaveValueAsync(note2);

            await Step("Then both 'Vraag 1' and 'Vraag 2' sections should be visible");

            await Expect(
                Page.GetByRole(AriaRole.Article)
                    .Filter(new() { HasText = "Vraag 1" })
            ).ToBeVisibleAsync();

            await Expect(
                Page.GetByRole(AriaRole.Article)
                    .Filter(new() { HasText = "Vraag 2" })
            ).ToBeVisibleAsync();

            await Step("And user fills in 'automation test specific vraag 1' in the specific vraag field in vraag 1");
            await Page.GetSpecifiekeVraagTextbox().First.FillAsync("automation test specific vraag");

            await Step("And user fills in 'automation test specific vraag 2' in the specific vraag field in vraag 2");
            await Page.GetSpecifiekeVraagTextbox().Nth(1).FillAsync("automation test specific vraag 2");

            await Step("select channel from the list for vraag 1");
            await Page.GetByLabel("Kanaal").First.SelectOptionAsync(["Balie"]);

            await Step("select channel from the list for vraag 2");
            await Page.GetByLabel("Kanaal").Nth(1).SelectOptionAsync(["Balie"]);

            await Step("And value 'test Gespreksresultaat TEST' in field Afhandeling for vraag 1");

            await Page.GetAfhandelingField().First.ClickAsync();
            await Page.GetAfhandelingField().First.SelectOptionAsync(new[] { new SelectOptionValue { Label = "test Gespreksresultaat TEST" } });

            await Step("And value 'test Gespreksresultaat TEST' in field Afhandeling for vraag 2");

            await Page.GetAfhandelingField().Nth(1).ClickAsync();
            await Page.GetAfhandelingField().Nth(1).SelectOptionAsync(new[] { new SelectOptionValue { Label = "test Gespreksresultaat TEST" } });

            await Step("And selects value 'Parkeren' in field Afdeling for vraag 1");

            await Page.Locator("article").Filter(new() { HasText = "Vraag 1" })
            .Locator("input[type='search']").ClickAsync();
            await Page.GetByText("Parkeren").First.ClickAsync();

            await Step("And selects value 'Parkeren' in field Afdeling for vraag 2");

            await Page.Locator("article").Filter(new() { HasText = "Vraag 2" })
            .Locator("input[type='search']").ClickAsync();
            await Page.GetByText("Parkeren").Nth(0).ClickAsync();

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("And Afhandeling form is successfully submitted");

            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

        [TestMethod("2. Two vragen within 1 contactmoment for Vestiging")]
        public async Task VragenVestiging()
        {
            await Step("Given the user is on KISS home page ");

            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");

            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("And user enters “990000996048” in Vestigingsnummer field");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();
            await Page.Company_KvknummerInput().FillAsync("990000996048");

            await Step("And clicks the search button");
            await Page.Company_KvknummerSearchButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("When user enters “test vraag 1 in Notitieblok");

            var note1 = "test vraag 1";

            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note1);

            await Step("clicks on “+” icon");

            await Page.GetPlusIcon().ClickAsync();

            await Step("When user enters “test vraag 2 in Notitieblok");

            var note2 = "test vraag 2";

            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note2);

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("Then Afhandeling form has value as “test vraag 1 in field Notitie for vraag 1");
            await Expect(Page.GetAfhandelingNotitieTextBox().First).ToHaveValueAsync(note1);

            await Step("Then Afhandeling form has value as “test vraag 1 in field Notitie for vraag 1");
            await Expect(Page.GetAfhandelingNotitieTextBox().Nth(1)).ToHaveValueAsync(note2);

            await Step("Then both 'Vraag 1' and 'Vraag 2' sections should be visible");

            await Expect(
                Page.GetByRole(AriaRole.Article)
                    .Filter(new() { HasText = "Vraag 1" })
            ).ToBeVisibleAsync();

            await Expect(
                Page.GetByRole(AriaRole.Article)
                    .Filter(new() { HasText = "Vraag 2" })
            ).ToBeVisibleAsync();

            await Step("And user fills in 'automation test specific vraag 1' in the specific vraag field in vraag 1");
            await Page.GetSpecifiekeVraagTextbox().First.FillAsync("automation test specific vraag 1");

            await Step("And user fills in 'automation test specific vraag 2' in the specific vraag field in vraag 2");
            await Page.GetSpecifiekeVraagTextbox().Nth(1).FillAsync("automation test specific vraag 2");

            await Step("select channel from the list for vraag 1");
            await Page.GetByLabel("Kanaal").First.SelectOptionAsync(["Balie"]);

            await Step("select channel from the list for vraag 2");
            await Page.GetByLabel("Kanaal").Nth(1).SelectOptionAsync(["Balie"]);

            await Step("And value 'test Gespreksresultaat TEST' in field Afhandeling for vraag 1");

            await Page.GetAfhandelingField().First.ClickAsync();
            await Page.GetAfhandelingField().First.SelectOptionAsync(new[] { new SelectOptionValue { Label = "test Gespreksresultaat TEST" } });

            await Step("And value 'test Gespreksresultaat TEST' in field Afhandeling for vraag 2");

            await Page.GetAfhandelingField().Nth(1).ClickAsync();
            await Page.GetAfhandelingField().Nth(1).SelectOptionAsync(new[] { new SelectOptionValue { Label = "test Gespreksresultaat TEST" } });

            await Step("And selects value 'Parkeren' in field Afdeling for vraag 1");

            await Page.Locator("article").Filter(new() { HasText = "Vraag 1" })
            .Locator("input[type='search']").ClickAsync();
            await Page.GetByText("Parkeren").First.ClickAsync();

            await Step("And selects value 'Parkeren' in field Afdeling for vraag 2");

            await Page.Locator("article").Filter(new() { HasText = "Vraag 2" })
            .Locator("input[type='search']").ClickAsync();
            await Page.GetByText("Parkeren").Nth(0).ClickAsync();

            await Step("And user clicks on Opslaan button");
            await Page.GetOpslaanButton().ClickAsync();

            await Step("And Afhandeling form is successfully submitted");

            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");

            await Step("When the user starts a new Contactmoment");
            await Page.CreateNewContactmomentAsync();

            await Step("And user enters “990000996048” in Vestigingsnummer field");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();
            await Page.Company_KvknummerInput().FillAsync("990000996048");

            await Step("And clicks the search button");
            await Page.Company_KvknummerSearchButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("user is navigated to Bedrijfinformatie page of Prijsknaller B.V.");
            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Bedrijfsinformatie" })).ToBeVisibleAsync();

            await Step("And user navigates to the contactmoment tab to view the created contact moment");
            await Page.GetByRole(AriaRole.Tab, new() { Name = "Contactmomenten" }).ClickAsync();

            await Step("And contactmoment details are displayed");

            var matchingRow = Page.Locator("table.overview tbody tr").Filter(new()
            {
                Has = Page.GetByText("icatt")
            });

            await matchingRow.First.GetByRole(AriaRole.Button).PressAsync("Enter");

            await Expect(Page.GetByRole(AriaRole.Definition).Filter(new() { HasText = "test vraag 2" })).ToBeVisibleAsync();


        }

        [TestMethod("3. Two vragen with different bronnen within 1 contactmoment")]
        public async Task vragenwithbronnen()
        {
            await Step("Given the user is on KISS home page ");

            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");

            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("And checks the box VAC and the box Kennisbank in the search pane");

            await Page.GetByRole(AriaRole.Checkbox, new() { Name = "VAC" }).CheckAsync();
            await Page.GetByRole(AriaRole.Checkbox, new() { Name = "Kennisbank" }).CheckAsync();

            await Step("And enters “het” in the search field in the Search pane ");

            await Page.GetByRole(AriaRole.Combobox).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox).FillAsync("het");

            await Step("And clicks on the first result in the list, with the title {{title 1}}, with {{label1}} ");

            await Page.GetByText("heb ik een rova-pas nodig voor de gft-container?").ClickAsync();
            await Page.GetByText("heb ik een rova-pas nodig voor de gft-container?").ClickAsync();

            await Step("return to search and click on second result");

            await Page.GetByRole(AriaRole.Combobox).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox).FillAsync("het");
            await Page.GetByText("heb ik een vergunning nodig om mijn pand te splits").ClickAsync();
            await Page.GetByText("heb ik een vergunning nodig om mijn pand te splits").ClickAsync();

            await Step("When user enters “test vraag 1 in Notitieblok");

            var note1 = "test vraag 1";

            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note1);

            await Step("clicks on “+” icon");

            await Page.GetPlusIcon().ClickAsync();

            await Step("And enters De boom in the search field in the Search pane ");

            await Page.GetByRole(AriaRole.Combobox).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox).FillAsync("De boom");

            await Step("And clicks on the first result in the list, with the title {{title 1}}, with {{label1}} ");

            await Page.GetByText("De boom van de buren is veel").Nth(0).ClickAsync();
            await Page.GetByText("De boom van de buren is veel").ClickAsync();

            await Step("When user enters “test vraag 2 in Notitieblok");

            var note2 = "test vraag 2";

            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note2);

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("Then both 'Vraag 1' and 'Vraag 2' sections should be visible");

            await Expect(
                Page.GetByRole(AriaRole.Article)
                    .Filter(new() { HasText = "Vraag 1" })
            ).ToBeVisibleAsync();

            await Expect(
                Page.GetByRole(AriaRole.Article)
                    .Filter(new() { HasText = "Vraag 2" })
            ).ToBeVisibleAsync();

            await Step(" VACs are displayed in the Gerelaterede VACs section in afhnadeling form");
            var sectionLocator = Page.Locator("section.gerelateerde-resources");

            await Expect(sectionLocator.GetByRole(AriaRole.Heading, new() { Name = "Gerelateerde VACs" })).ToBeVisibleAsync();

            var vergunningLabel = sectionLocator.Locator("label").Filter(new() { HasText = "Heb ik een vergunning nodig om mijn pand te splitsen?" });
            await Expect(vergunningLabel).ToBeVisibleAsync();

            var rovaLabel = sectionLocator.Locator("label").Filter(new() { HasText = "Heb ik een ROVA-pas nodig voor de gft-container?" });
            await Expect(rovaLabel).ToBeVisibleAsync();

            var boomLabel = sectionLocator.Locator("label").Filter(new() { HasText = "De boom van de buren is veel" });
            await Expect(boomLabel).ToBeVisibleAsync();

        }

        [TestMethod("4. Vraag field displays last displayed section of Kennisartkel")]
        [Obsolete]
        public async Task vragenValueValidation()
        {
            await Step("Given the user is on KISS home page ");

            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");

            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("And enters “Kind erkennen” in the search field in the Search pane");

            var searchInput = Page.Locator("#global-search-input");

            await searchInput.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await searchInput.FillAsync("Kind erkennen");

            await Step("And clicks on the first result in the list");

            await Page.GetByText("Kind erkennen").First.ClickAsync();

            await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);

            await Page.GetByRole(AriaRole.Link, new() { Name = "Kennisbank Kind erkennen" }).ClickAsync();

            await Step("And navigate to BijZonderheden section ");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Bijzonderheden" }).ClickAsync();

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("Then vraag field value is prefilled with last visited section");

            var select = Page.GetByLabel("Vraag", new() { Exact = true });

            var selectedText = await select.EvaluateAsync<string>("el => el.options[el.selectedIndex].text");

            Assert.AreEqual("Kind erkennen - Bijzonderheden", selectedText);

        }

        [TestMethod("5. Long Vraag, no Specifieke vraag ")]
        public async Task longVraag()
        {
            await Step("Given the user is on KISS home page ");

            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");

            await Page.GetNieuwContactmomentButton().ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await Step("And enters “testing” in the search field in the Search pane ");

            await Page.GetByRole(AriaRole.Combobox).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox).FillAsync("testing");
            await Page.GetByRole(AriaRole.Combobox).PressAsync("Enter");

            await Step("And clicks on the VAC wth more characters ");
            await Page.GetSearchVAC().ClickAsync();

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetKanaalField().ClickAsync();

            await Page.GetKanaalField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("And value 'Zelfstandig afgehandeld' in field Afhandeling");

            await Page.GetAfhandelingField().ClickAsync();
            await Page.GetAfhandelingField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Zelfstandig afgehandeld" } });

            await Step("And selects value 'Publiekscontacten Vergunningen' in field Afdeling");

            await Page.GetAfdelingVoorField().ClickAsync();
            await Page.GetByText("Publiekscontacten Vergunningen").First.ClickAsync();

            await Step("And clicks on Opslaan button");

            var klantContactPostResponse = await Page.RunAndWaitForResponseAsync(async () =>
            {
                await Page.GetOpslaanButton().ClickAsync();
            },
                response => response.Url.Contains("/postklantcontacten")
            );

            await Step("And Afhandeling form is successfully submitted");

            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

        [TestMethod("6. Long vraag ánd Long specifieke vraag ")]
        public async Task longVraaglongSpecifiekevraag()
        {
            await Step("Given the user is on KISS home page ");

            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");

            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("When user enters “PC-1478” in Notitieblok");

            var note = "PC-1478”";

            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note);

            await Step("And enters “testing” in the search field in the Search pane ");

            await Page.GetByRole(AriaRole.Combobox).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox).FillAsync("testing");
            await Page.GetByRole(AriaRole.Combobox).PressAsync("Enter");

            await Step("And clicks on the VAC wth more characters ");
            await Page.GetSearchVAC().ClickAsync();

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in '180 char long string' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("This vraag is 180 characters long_efghi 4bcdefghi 5bcdefghi 6bcdefghi 7bcdefghi 8bcdefghi 9bcdefghi 10cdefghi 11cdefghi 12cdefghi 13cdefghi 14cdefghi 15cdefghi 16cdefghi");

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetKanaalField().ClickAsync();

            await Page.GetKanaalField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("And value 'Zelfstandig afgehandeld' in field Afhandeling");

            await Page.GetAfhandelingField().ClickAsync();
            await Page.GetAfhandelingField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Zelfstandig afgehandeld" } });

            await Step("And selects value 'Publiekscontacten Vergunningen' in field Afdeling");

            await Page.GetAfdelingVoorField().ClickAsync();
            await Page.GetByText("Publiekscontacten Vergunningen").First.ClickAsync();

            await Step("And clicks on Opslaan button");

            var klantContactPostResponse = await Page.RunAndWaitForResponseAsync(async () =>
            {
                await Page.GetOpslaanButton().ClickAsync();
            },
                response => response.Url.Contains("/postklantcontacten")
            );

            await Step("And Afhandeling form is successfully submitted");

            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

        [TestMethod("7. slightly shorter vraag and slightly shorter specifieke vraag ")]
        public async Task VraagandSpecifiekevraag()
        {
            await Step("Given the user is on KISS home page ");

            await Page.GotoAsync("/");

            await Step("And user clicks on Nieuw contactmoment button");

            await Page.GetNieuwContactmomentButton().ClickAsync();

            await Step("When user enters “PC-1478” in Notitieblok");

            var note = "PC-1478”";

            await Page.GetContactmomentNotitieblokTextbox().FillAsync(note);

            await Step("And enters “testing” in the search field in the Search pane ");

            await Page.GetByRole(AriaRole.Combobox).ClickAsync();
            await Page.GetByRole(AriaRole.Combobox).FillAsync("testing");
            await Page.GetByRole(AriaRole.Combobox).PressAsync("Enter");

            await Step("And clicks on the VAC wth more characters ");
            await Page.GetSearchVAC().ClickAsync();

            await Step("Click the Afronden button");

            await Page.GetAfrondenButton().ClickAsync();

            await Step("And user fills in '140 char long string' in the specific vraag field");
            await Page.GetSpecifiekeVraagTextbox().FillAsync("This vraag is 140 characters long_efghi 4bcdefghi 5bcdefghi 6bcdefghi 7bcdefghi 8bcdefghi 9bcdefghi 10cdefghi 11cdefghi 12cdefghi 13cdefghiJ");

            await Step("And user enters 'Live chat' in field Kanaal");

            await Page.GetKanaalField().ClickAsync();

            await Page.GetKanaalField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Live Chat" } });

            await Step("And value 'Zelfstandig afgehandeld' in field Afhandeling");

            await Page.GetAfhandelingField().ClickAsync();
            await Page.GetAfhandelingField().SelectOptionAsync(new[] { new SelectOptionValue { Label = "Zelfstandig afgehandeld" } });

            await Step("And selects value 'Publiekscontacten Vergunningen' in field Afdeling");

            await Page.GetAfdelingVoorField().ClickAsync();
            await Page.GetByText("Publiekscontacten Vergunningen").First.ClickAsync();

            await Step("And clicks on Opslaan button");

            var klantContactPostResponse = await Page.RunAndWaitForResponseAsync(async () =>
            {
                await Page.GetOpslaanButton().ClickAsync();
            },
                response => response.Url.Contains("/postklantcontacten")
            );

            await Step("And Afhandeling form is successfully submitted");
            await Expect(Page.GetAfhandelingSuccessToast()).ToHaveTextAsync("Het contactmoment is opgeslagen");
        }

    }
}

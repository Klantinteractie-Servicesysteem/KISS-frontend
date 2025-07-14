using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.EndToEndTest.Common.Helpers;
using Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers;

namespace Kiss.Bff.EndToEndTest.ContactMomentSearch
{
    [TestClass]
    public class SearchPersonScenarios : KissPlaywrightTest
    {

        [TestMethod("1. Searching by Last Name and Date of Birth (Valid)")]
        public async Task SearchByLastNameAndDOB_ValidAsync()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"Burck\" in the field achternaam and enters \"17-11-1952\" in the field geboortedatum ");

            await Page.Personen_LastNameInput().FillAsync("Burck");
            await Page.Personen_BirthDateInput().FillAsync("17-11-1952");

            await Step("And clicks the search button");

            await Page.PersonenFirst_SearchButton().ClickAsync();

            await Step("Then user is navigated to Persoonsinformatie page ");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

        }


        [TestMethod("2. Searching by Last Name and Date of Birth (Not Found)")]
        public async Task SearchByLastNameAndDOB_NotFoundAsync()
        {

            await Step("Given the user is on the startpagina ");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"TestDB” in the field achternaam and \"11-12-1990” in the field geboortedatum ");

            await Page.Personen_LastNameInput().FillAsync("TestDB");
            await Page.Personen_BirthDateInput().FillAsync("11-12-1990");

            await Step("And clicks the search button");

            await Page.PersonenFirst_SearchButton().ClickAsync();

            await Step("Then the message is displayed as “Geen resultaten gevonden voor ’TestDB, 11-12-1990’.");

            await Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor 'TestDB, 11-12-1990'.");

        }

        [TestMethod("3. Searching by BSN (Valid)")]
        public async Task SearchByBSN_Valid()
        {
            await Step("Given the user is on the startpagina ");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"999992223\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("999992223");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();

            await Step("Then user is navigated to Persoonsinformatie page");

            await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Persoonsinformatie" })).ToHaveTextAsync("Persoonsinformatie");

        }


        [TestMethod("4. Searching by BSN (Invalid)")]
        public async Task SearchByBSN_Invalid()
        {
            await Step("Given the user is on the startpagina ");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"123456789\" in the field bsn ");

            await Page.PersonenBsnInput().FillAsync("123456789");

            await Step("And clicks the search button");

            await Page.PersonenThird_SearchButton().ClickAsync();

            await Step("Then the message is displayed as “Dit is geen valide BSN.”");

            Assert.AreEqual(await Page.PersonenBsnInput().EvaluateAsync<string>("(el) => el.validationMessage"), "Dit is geen valide BSN.");

        }


        [TestMethod("5. Searching by Postcode and Huisnummer (Valid)")]
        public async Task SearchByPostcodeAndHuisnummer_Valid()
        {
            await Step("Given the user is on the startpagina ");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"3544NG\" in the field Postcode and “10” in field Huisnummer");

            var postCode = "3544 NG";
            var huisNummer = "10";

            await Page.Personen_PostCodeInput().FillAsync(postCode);
            await Page.Personen_HuisnummerInput().FillAsync(huisNummer);

            await Step("And clicks the search button");

            await Page.PersonenSecond_SearchButton().ClickAsync();


            await Step("Then a list of multiple records associated with same huisnummer and postcode is displayed ");

            await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();

            var resultCount = await Page.SearchAddressByPostalAndHuisNummer(postCode, huisNummer).CountAsync();

            Assert.IsTrue(resultCount > 2, $"Expected there to be multiple records associated with postCode {postCode} and huisNummer {huisNummer}, but found {resultCount}.");
        }

        [TestMethod("6. Searching by Postcode and Huisnummer (Not Found)")]
        public async Task SearchByPostcodeAndHuisnummer_NotFound()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"3544NG\" in the field postcode and “11” in field");

            await Page.Personen_PostCodeInput().FillAsync("3544 NG");
            await Page.Personen_HuisnummerInput().FillAsync("11");

            await Step("And clicks the search button");

            await Page.PersonenSecond_SearchButton().ClickAsync();

            await Step("Then the message as “Geen resultaten gevonden voor '3544NG, 11'.” is displayed ");

            await Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor '3544NG, 11'.");


        }


        [TestMethod("7. Searching by Partial Last Name and Date of Birth (Multiple Results)")]
        public async Task SearchByPartialLastNameAndDOB_MultipleResults()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters \"Mel\" in the field Achternaam and enters \"12091946\" in the field geboortedatum ");

            await Page.Personen_LastNameInput().FillAsync("Mel");
            await Page.Personen_BirthDateInput().FillAsync("12091946");

            await Step("And clicks the search button");

            await Page.PersonenFirst_SearchButton().ClickAsync();

            await Step("Then the message as “2 resultaten gevonden voor ' Mel, 12-09-1946 '.” is displayed ");

            await Page.GetByRole(AriaRole.Table).WaitForAsync();

            await Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("2 resultaten gevonden voor 'Mel, 12-09-1946'.");

            await Step("And a list of two records is displayed");

            await Expect(Page.GetByRole(AriaRole.Table).Locator("tr.row-link")).ToHaveCountAsync(2);

            await Step("One with the value \" Julia Christine Maria Melap\" in column Naam ");

            await Expect(Page.GetByRole(AriaRole.Table).Locator("tr.row-link")
                .Nth(0).Locator("th[scope='row']")).ToHaveTextAsync("Julia Christine Maria Melap");

            await Step("And one with value \"Julia Christina Melapatti\" in column Naam ");

            await Expect(Page.GetByRole(AriaRole.Table).Locator("tr.row-link")
                .Nth(1).Locator("th[scope='row']")).ToHaveTextAsync("Julia Christina Melapatti");




        }

    }
}

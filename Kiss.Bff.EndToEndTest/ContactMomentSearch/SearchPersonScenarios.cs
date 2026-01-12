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
        public async Task SearchingByLastNameAndDOB_ExpectNavigationToPersoonsinformatiePage()
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

            await Expect(Page.GetByText("Persoonsgegevens")).ToBeVisibleAsync();

        }


        [TestMethod("2. Searching by Last Name and Date of Birth (Not Found)")]
        public async Task SearchingByUnknownLastNameAndDOB_ExpectNoResultsFound()
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
        public async Task SearchingByBSN_ExpectNavigationToPersoonsinformatiePage()
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
            await Expect(Page.GetByText("Persoonsgegevens")).ToBeVisibleAsync();

        }


        [TestMethod("4. Searching by BSN (Invalid)")]
        public async Task SearchByUnknownBSN_ExpectNoResultsFound()
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
        public async Task SearchingByPostcodeAndHuisnummer_ExpectNavigationToPersoonsinformatiePag()
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
        public async Task SearchingByPostcodeAndHuisnummer_ExpectNoResultsFound()
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
        public async Task SearchingByPartialLastNameAndDOB_MultipleResults()
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

        [TestMethod("8. Searching by Postcode and Huisnummer with optional Achternaam")]
        public async Task SearchingByPostcodeHuisnummer_WithOptionalAchternaam_ExpectListofresult()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters postcode and huisnummer and fills an achternaam filter");

            await Page.Personen_PostCodeInput().FillAsync("2511CA");
            await Page.Personen_HuisnummerInput().FillAsync("21");
            await Page.Personen_PostcodeForm_AchternaamInput().FillAsync("Krabben");
            await Page.WaitForTimeoutAsync(1000);

            await Step("And clicks the search button");

            await Page.PersonenSecond_SearchButton().ClickAsync();

            await Step("Then the results should be filtered using achternaam");

            await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();

            var allNames = await Page.Locator("table tr.row-link th[scope='row']").AllTextContentsAsync();
            Assert.IsTrue(allNames.Any(), "Geen resultaten gevonden voor '2511CA, 21, Krabben'.");
            Assert.IsTrue(allNames.All(name => name.Contains("Krabben", StringComparison.OrdinalIgnoreCase)),
                "Niet alle resultaten bevatten 'krabben' in de naam.");
        }

        [TestMethod("9. Searching for a customer using the postcode and Huisnummer, and then Achternaam")]
        public async Task When_SearchingWithPostcodeHuisnummerAndThenAchternaam_ExpectFilteredResults()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters postcode and huisnummer and fills an achternaam filter");

            await Page.Personen_PostCodeInput().FillAsync("2511CA");
            await Page.Personen_HuisnummerInput().FillAsync("21");

            await Step("And clicks the search button");

            await Page.PersonenSecond_SearchButton().ClickAsync();

            await Step("Then a list of 11 records associated with same huisnummer and postcode is displayed ");
            await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
            await Expect(Page.GetByText("11 resultaten gevonden voor '2511CA, 21'.")).ToBeVisibleAsync();

            await Step("When user enters “kor” in the field Achternaam, ");
            await Page.Personen_PostcodeForm_AchternaamInput().FillAsync("Kor");

            await Step("And clicks the search button");

            await Page.PersonenSecond_SearchButton().ClickAsync();

            await Step("Then a list of 6 records associated with same huisnummer and postcode is displayed ");
            await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
            await Expect(Page.GetByText("6 resultaten gevonden voor '2511CA, 21, kor'.")).ToBeVisibleAsync();

        }

        [TestMethod("10. Searching for a customer using the postcode and Huisnummer, and only two letters of Achternaam")]
        public async Task When_AchternaamSearchInputLessThenThreeCharacters_Expect_ValidationMessage()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters postcode and huisnummer and fills an achternaam filter");

            await Page.Personen_PostCodeInput().FillAsync("2511CA");
            await Page.Personen_HuisnummerInput().FillAsync("21");
            await Page.Personen_PostcodeForm_AchternaamInput().FillAsync("Kr");

            await Step("And clicks the search button");

            await Page.PersonenSecond_SearchButton().ClickAsync();

            await Step("Then an error message should appear on achternaam field");

            await Expect(Page.Personen_PostcodeForm_AchternaamInput()).ToHaveJSPropertyAsync("validationMessage", "Vul een geldig (begin van een) achternaam in, van minimaal 3 tekens");


        }

        [TestMethod("11. Searching for a customer using the postcode and Huisnummer, and then huisletter")]
        public async Task When_SearchingWithPostcodeHuisnummerAndThenHuisletter_ExpectFilteredResults()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("And user enters postcode and huisnummer and fills an achternaam filter");

            await Page.Personen_PostCodeInput().FillAsync("1074HK");
            await Page.Personen_HuisnummerInput().FillAsync("1");

            await Step("And clicks the search button");

            await Page.PersonenSecond_SearchButton().ClickAsync();

            await Step("Then a list of 11 records associated with same huisnummer and postcode is displayed ");
            await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
            await Expect(Page.GetByText("4 resultaten gevonden voor '1074HK, 1'.")).ToBeVisibleAsync();

            await Step("When user enters “B” in the field Achternaam, ");
            await Page.Personen_HuisletterInput().FillAsync("b");

            await Step("And clicks the search button");

            await Page.PersonenSecond_SearchButton().ClickAsync();

            await Step("Then a list of 2 records associated with same huisletter and postcode is displayed ");
            await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
            await Expect(Page.GetByText("2 resultaten gevonden voor '1074HK, 1, b'.")).ToBeVisibleAsync();

        }


    }
}

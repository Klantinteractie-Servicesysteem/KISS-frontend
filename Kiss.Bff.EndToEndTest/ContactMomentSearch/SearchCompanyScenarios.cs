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
    public class SearchCompanyScenarios : KissPlaywrightTest
    {

        [TestMethod("1. Searching by Partial Last Name and Date of Birth (Single Result)")]
        public async Task SearchByBedrijfsnaam_MultipleRecordsAsync()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user clicks Bedrijf from the menu item ");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And user enters “Donald” in the field Bedrijfsnaam");

            var lastName = "Donald";

            await Page.Company_BedrijfsnaamInput().FillAsync(lastName);

            await Step("And clicks the search button");

            await Page.Company_FirstSearchButton().ClickAsync();

            await Step("Then the list displays multiple records associated with the name “Donald”.");

            await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();

            var resultCount = await Page.GetByRole(AriaRole.Table).GetByRole(AriaRole.Row).CountAsync();

            Assert.IsTrue(resultCount > 2, $"Expected multiple records associated with the last name '{lastName}', but found {resultCount}.");
        }

        [TestMethod("2. Search By Bedrijfsnaam Unique Result ")]
        public async Task SearchByBedrijfsnaam_UniqueResultAsync()
        {
            await Step("Given the user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("When user starts a new contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user clicks Bedrijf from the menu item ");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And user enters “Test Stichting Bolderbast” in the field Bedrijfsnaam");

            await Page.Company_BedrijfsnaamInput().FillAsync("Test Stichting Bolderbast");

            await Step("And clicks the search button");

            await Page.Company_FirstSearchButton().ClickAsync();

            await Step("Then user is navigated to bedrijfsinformatie page");

            await Expect(Page.GetByText("Bedrijfsgegevens")).ToBeVisibleAsync();
            await Expect(Page.Locator("h1").Filter(new() { HasText = "Test Stichting Bolderbast" })).ToBeVisibleAsync();

        }



        [TestMethod("3. Searching for a company by Bedrijfsnaam not available in the database")]
        public async Task SearchByNonExistentBedrijfsnaamAsync()
        {
            await Step("Given user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("And starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user click Bedrijf from the menu item");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And the user enters 'test automation' in the field Bedrijfsnaam");

            await Page.Company_BedrijfsnaamInput().FillAsync("test automation");

            await Step("And clicks the search button");

            await Page.Company_FirstSearchButton().ClickAsync();

            await Step("Then message is displayed as Geen resultaten gevonden voor 'test automation'.");

            await Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor 'test automation'.");
        }

        [TestMethod("4. Searching a company by KVK-nummer")]
        public async Task SearchByKvknummerAsync()
        {
            await Step("Given user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("And starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user click Bedrijf from the menu item");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And user enters '69599068' in the field KVK-nummer of vestigingsnummer");

            await Page.Company_KvknummerInput().FillAsync("69599068");

            await Step("And clicks the search button");

            await Page.Company_KvknummerSearchButton().ClickAsync();

            await Step("Then user is navigated to bedrijfinformatie page of Test Stichting Bolderbast");

            await Expect(Page.GetByText("Bedrijfsgegevens")).ToBeVisibleAsync();
            await Expect(Page.Locator("h1").Filter(new() { HasText = "Test Stichting Bolderbast" })).ToBeVisibleAsync();

        }

        [TestMethod("5. Searching a company using KVK-nummer with multiple records")]
        public async Task SearchByKvknummer_MultipleRecordsAsync()
        {
            await Step("Given user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("And starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user clicks Bedrijf from the menu item");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And the user enters '68750110' in the field KVK-nummer of vestigingsnummer");

            await Page.Company_KvknummerInput().FillAsync("68750110");

            await Step("And clicks the search button");

            await Page.Company_KvknummerSearchButton().ClickAsync();

            await Step("Then the list displays multiple records associated with the kvknummer '68750110'");

            await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();

            var resultCount = await Page.GetByRole(AriaRole.Table).GetByRole(AriaRole.Row).CountAsync();

            Assert.IsTrue(resultCount > 2, $"Expected there to be multiple records associated with kvknummer '68750110', but found {resultCount}.");
        }


        [TestMethod("6. Searching a company using vestigingsnummer which is unique")]
        public async Task SearchByVestigingsnummer_UniqueResultAsync()
        {
            await Step("Given user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("And user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user clicks Bedrijf from the menu item");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And user enters '000037178601' in the field KVK-nummer of vestigingsnummer");

            await Page.Company_KvknummerInput().FillAsync("000037178601");

            await Step("And clicks the search button");

            await Page.Company_KvknummerSearchButton().ClickAsync();

            await Step("Then user is navigated to bedrijfinformatie page of Test BV Donald Nevenvestiging");

            await Expect(Page.GetByText("Bedrijfsgegevens")).ToBeVisibleAsync();
            await Expect(Page.Locator("h1").Filter(new() { HasText = "Test BV Donald Nevenvestiging" })).ToBeVisibleAsync();

        }


        [TestMethod("7. Searching a company by a kvknummer which is not available in DB")]
        public async Task SearchByNonExistentKvknummerAsync()
        {
            await Step("Given user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("And user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user clicks Bedrijf from the menu item");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And user enters '12345678' in the field KVK-nummer of vestigingsnummer");

            await Page.Company_KvknummerInput().FillAsync("12345678");

            await Step("And clicks the search button");

            await Page.Company_KvknummerSearchButton().ClickAsync();

            await Step("Then message is displayed as 'Geen resultaten gevonden voor 12345678'.");

            await Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor '12345678'.");
        }

        [TestMethod("8. Searching a company by postcode and huisnummer")]
        public async Task SearchByPostcodeAndHuisnummerAsync()
        {
            await Step("Given user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("And user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user clicks Bedrijf from the menu item");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And the user enters '7431BX' in the field postcode and '3' in Huisnummer field");

            await Page.Company_PostcodeInput().FillAsync("7431BX");
            await Page.Company_HuisnummerInput().FillAsync("3");

            await Step("And clicks the search button");

            await Page.Company_PostcodeHuisnummerSearchButton().ClickAsync();

            await Step("Then user is navigated to Bedrijfsgegevens page of Test BV Donald Nevenvestiging");

            await Expect(Page.GetByText("Bedrijfsgegevens")).ToBeVisibleAsync();
            await Expect(Page.Locator("h1").Filter(new() { HasText = "Test BV Donald Nevenvestiging" })).ToBeVisibleAsync();

        }

        [TestMethod("9. Searching a company using postcode and huisnummer associated with multiple records")]
        public async Task SearchByPostcodeAndHuisnummer_MultipleRecordsAsync()
        {
            await Step("Given user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("And user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user clicks Bedrijf from the menu item");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And the user enters '2352SZ' in the field postcode and '37' in field Huisnummer");

            var postCode = "2352SZ";
            var huisNummer = "37";
            await Page.Company_PostcodeInput().FillAsync(postCode);
            await Page.Company_HuisnummerInput().FillAsync(huisNummer);

            await Step("And clicks the search button");

            await Page.Company_PostcodeHuisnummerSearchButton().ClickAsync();

            await Step("Then list of multiple records associated with postcode '2352SZ' and huisnummer '37' is displayed");

            await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();

            var resultCount = await Page.SearchCompanyByPostalAndHuisNummer(postCode, huisNummer).CountAsync();

            Assert.IsTrue(resultCount > 1, $"Expected multiple records associated with postcode '2352SZ' and huisnummer '37', but found {resultCount}.");
        }


        [TestMethod("10. Searching a company using postcode and huisnummer which is not present in DB")]
        public async Task SearchByNonExistentPostcodeAndHuisnummerAsync()
        {
            await Step("Given user is on the startpagina");

            await Page.GotoAsync("/");

            await Step("And user starts a new Contactmoment");

            await Page.CreateNewContactmomentAsync();

            await Step("When user clicks Bedrijf from the menu item");

            await Page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();

            await Step("And the user enters '1234AB' in the field postcode and '10' in field Huisnummer");

            await Page.Company_PostcodeInput().FillAsync("1234AB");
            await Page.Company_HuisnummerInput().FillAsync("10");

            await Step("And clicks the search button");

            await Page.Company_PostcodeHuisnummerSearchButton().ClickAsync();

            await Step("Then message is displayed as 'Geen resultaten gevonden voor '1234AB, 10''.");

            await Expect(Page.GetByRole(AriaRole.Caption)).ToHaveTextAsync("Geen resultaten gevonden voor '1234AB, 10'.");
        }

    }
}

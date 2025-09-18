using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers
{
    internal static class Locators
    {

        public static ILocator GetNieuwContactmomentButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" });
        }
        public static ILocator GetContactmomentNotitieblokTextbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Tabpanel, new() { Name = "Notitieblok" }).GetByRole(AriaRole.Textbox);
        }
        public static ILocator GetAfhandelingNotitieTextBox(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Notitie (maximaal 1000 tekens)" });
        }

        public static ILocator GetSpecificVraagField(this IPage page)
        {
            return page.GetByRole(AriaRole.Textbox, new() { Name = "Specifieke vraag (maximaal 180 tekens)" });
        }

        public static ILocator GetKanaalField(this IPage page)
        {
            return page.GetByLabel("Kanaal");
        }

        public static ILocator GetAfhandelingField(this IPage page)
        {
            return page.GetByRole(AriaRole.Combobox, new() { Name = "Afhandeling" });
        }

        // public static ILocator GetAfdelingField(this IPage page)
        // {
        //     return page.GetByRole(AriaRole.Combobox, new() { Name = "Afdeling" });
        // }

        // This input is not associated with a label. This needs to be handled in development to ensure proper accessibility and identification.
        public static ILocator GetAfdelingVoorField(this IPage page)
        {
            return page.Locator("input[type='search']");
        }

        public static ILocator GetAfhandelingSuccessToast(this IPage page)
        {
            return page.Locator("output[role='status']");
        }

        // public static ILocator GetAnnulerenButton(this IPage page) => page.GetByRole(AriaRole.Button, new() { Name = "Annuleren" });
        public static ILocator GetConfirmationJaButton(this IPage page) => page.GetByRole(AriaRole.Button, new() { Name = "Ja" });
        public static ILocator GetConfirmationNeeButton(this IPage page) => page.GetByRole(AriaRole.Button, new() { Name = "Nee" });
        public static ILocator GetNieuwButton(this IPage page) => page.GetByRole(AriaRole.Button, new() { Name = "Nieuw" });
        public static ILocator GetActiefTabButton(this IPage page) => page.GetByRole(AriaRole.Tab, new() { Name = "Actief" });
        public static ILocator GetActiveSessions(this IPage page) => page.Locator("[data-testid='active-session']"); // Adjust selector as needed
        public static ILocator GetFirstActiveSession(this IPage page) => page.GetActiveSessions().First;
        public static ILocator GetContactverzoekPaneButton(this IPage page) => page.GetByRole(AriaRole.Button, new() { Name = "Contactverzoeken" });
        public static ILocator GetTelefoonnummerField(this IPage page) => page.Locator("[data-testid='telefoonnummer-field']"); // Adjust as needed

        public static async Task SearchAndSelectPerson(this IPage page, string personName)
        {
            // Implement person search and selection logic
            await page.GetByTestId("person-search").FillAsync(personName);
            await page.GetByText(personName).ClickAsync();
        }

        public static async Task SearchAndSelectCompany(this IPage page, string companyName)
        {
            // Implement company search and selection logic
            await page.GetByTestId("company-search").FillAsync(companyName);
            await page.GetByText(companyName).ClickAsync();
        }

        public static async Task SearchAndSelectZaak(this IPage page, string zaakNumber)
        {
            // Try different role types
            await page.GetByRole(AriaRole.Tab, new() { Name = "Zaken" }).ClickAsync();
            // OR
            await page.GetByRole(AriaRole.Button, new() { Name = "Zaken" }).ClickAsync();

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var searchField = page.GetByRole(AriaRole.Textbox).First; // Get first textbox
            await searchField.FillAsync(zaakNumber);
            await page.GetByText(zaakNumber).ClickAsync();
        }
    }
}

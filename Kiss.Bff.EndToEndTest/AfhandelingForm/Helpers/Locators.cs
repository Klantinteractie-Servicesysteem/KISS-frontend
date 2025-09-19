using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers;

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

        // This input is not associated with a label. This needs to be handled in development to ensure proper accessibility and identification.
        public static ILocator GetAfdelingVoorField(this IPage page)
        {
            return page.Locator("input[type='search']");
        }

        public static ILocator GetAfhandelingSuccessToast(this IPage page)
        {
            return page.Locator(".confirm output[role='status']");
        }

        public static ILocator GetConfirmationJaButton(this IPage page) => page.GetByRole(AriaRole.Button, new() { Name = "Ja" });
        public static ILocator GetConfirmationNeeButton(this IPage page) => page.GetByRole(AriaRole.Button, new() { Name = "Nee" });
        public static async Task SearchAndSelectPerson(this IPage page, string bsn)
        {
            await page.PersonenBsnInput().FillAsync(bsn);
            await page.PersonenThird_SearchButton().ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        }

        public static async Task SearchAndSelectCompany(this IPage page, string Vestigingsnr)
        {
            await page.GetByRole(AriaRole.Link, new() { Name = "Bedrijven" }).ClickAsync();
            await page.Company_KvknummerInput().FillAsync(Vestigingsnr);
            await page.Company_KvknummerSearchButton().ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        }

        public static async Task SearchAndSelectZaak(this IPage page, string zaakNumber)
        {
            await page.GetByRole(AriaRole.Link, new() { Name = "Zaken" }).ClickAsync();
            await page.GetByPlaceholder("Zoek op zaaknummer").FillAsync(zaakNumber);
            await page.GetByTitle("Zoeken").ClickAsync();
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        }
    }
}

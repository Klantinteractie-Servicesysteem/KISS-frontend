using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.AfhandelingForm.Helpers
{
    internal static class Locatorsdd
    {

        public static ILocator GetPlusIcon(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Nieuwe vraag" });
        }

        public static async Task SearchAndSelectItem(this IPage page, string searchTerm, string resultName, bool exact = false)
        {
            await page.GetByRole(AriaRole.Combobox, new() { Name = "Zoekterm" }).ClickAsync();
            await page.GetByRole(AriaRole.Combobox, new() { Name = "Zoekterm" }).FillAsync(searchTerm);

            await page.GetByRole(AriaRole.Combobox).PressAsync("Enter");

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            await page.GetByRole(AriaRole.Link, new() { Name = resultName, Exact = exact }).ClickAsync();
        }

    }
}

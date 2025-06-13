using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.AnonymousContactverzoek.Helpers
{
    internal static class ManageAnonymousContactverzoek
    {
        public static async Task CreateNewcontactVerzoekAsync(this IPage page)
        {
            await page.GetByRole(AriaRole.Tab, new() { Name = "Contactverzoek", Exact = true }).ClickAsync();

        }
    }
}

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

        public static ILocator GetSearchVAC(this IPage page)
        {
            return page.GetByRole(AriaRole.Link, new() { Name = "VAC This title is 210" });
        }

    }
}

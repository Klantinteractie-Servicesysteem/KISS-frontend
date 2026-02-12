using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentBronnen.Helpers
{
    public static class Locators
    {
        // During development, it is essential to include the ARIA role to ensure the element is located accurately, rather than relying on the class name.
        public static ILocator GetGlobalSearch(this IPage page)
        {
            return page.Locator(".contactmomentLoopt").GetByRole(AriaRole.Combobox);
        }

        public static ILocator GetGlobalSearchResults(this IPage page)
        {
            return page.Locator(".search-results.isExpanded").GetByRole(AriaRole.Navigation).First.GetByRole(AriaRole.Link);
        }

        public static ILocator GetSmoelenboekCheckbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Checkbox, new() { Name = "Smoelenboek" });
        }

        public static ILocator GetVACCheckbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Checkbox, new() { Name = "VAC" });
        }

        public static ILocator GetKennisbankCheckbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Checkbox, new() { Name = "Kennisbank" });
        }

        public static ILocator GetInfoNlCheckbox(this IPage page)
        {
            return page.GetByRole(AriaRole.Checkbox, new() { Name = "info.nl" });
        }
        public static ILocator GetBijzonderhedenTab(this IPage page)
        {
            return page.GetByRole(AriaRole.Link, new() { Name = "Bijzonderheden" });
        }
               

        public static ILocator GetPersonenAfrondenButton(this IPage page)
        {
            return page.GetByRole(AriaRole.Button, new() { Name = "Afronden" });
        }

        public static ILocator GetAfhandelingForm(this IPage page)
        {
            return page.Locator("form.afhandeling");
        }

        public static ILocator GetVraagField(this IPage page)
        {
            return page.GetByLabel("Vraag", new() { Exact = true });
        }
        public static ILocator GetAfdelingField(this IPage page)
        {
            return page.Locator("input[type=\"search\"]");
        } 
       
    }
   }

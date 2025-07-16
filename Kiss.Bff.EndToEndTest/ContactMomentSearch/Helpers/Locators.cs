using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.ContactMomentSearch.Helpers
{
    internal static class Locators
    {

        public static ILocator Personen_LastNameInput(this IPage page) =>
           page.GetByRole(AriaRole.Textbox, new() { Name = "Achternaam" });

        public static ILocator Personen_BirthDateInput(this IPage page) =>
           page.GetByRole(AriaRole.Textbox, new() { Name = "Geboortedatum" });

        public static ILocator PersonenFirst_SearchButton(this IPage page) =>
           page.Locator("form").Filter(new() { HasText = "Achternaam Geboortedatum" }).GetByRole(AriaRole.Button);
        public static ILocator PersonenSecond_SearchButton(this IPage page) =>
          page.Locator("form").Filter(new() { HasText = "Postcode Huisnummer" }).GetByRole(AriaRole.Button);

        public static ILocator PersonenThird_SearchButton(this IPage page) =>
            page.Locator("form").Filter(new() { HasText = "Bsn Zoeken" }).GetByRole(AriaRole.Button);
        public static ILocator Personen_PostCodeInput(this IPage page) =>
           page.GetByRole(AriaRole.Textbox, new() { Name = "postcode" });
        public static ILocator Personen_HuisnummerInput(this IPage page) =>
          page.GetByRole(AriaRole.Textbox, new() { Name = "huisnummer" });

        public static ILocator SearchAddressByPostalAndHuisNummer(this IPage page, string postcode, string huisNummer)
        {
            return page.GetByRole(AriaRole.Table).Locator($"tbody tr:has(td:nth-child(4):has-text(\"{postcode.Trim()}\"))" +
                                         $":has(td:nth-child(3):has-text(\"{huisNummer.Trim()}\"))");
        }
        public static ILocator Company_BedrijfsnaamInput(this IPage page) =>
        page.GetByRole(AriaRole.Textbox, new() { Name = "Bedrijfsnaam" });

        public static ILocator Company_KVKNumberInput(this IPage page) =>
            page.GetByRole(AriaRole.Textbox, new() { Name = "KVK-nummer" });

        public static ILocator Company_PostcodeInput(this IPage page) =>
            page.GetByRole(AriaRole.Textbox, new() { Name = "Postcode" });

        public static ILocator Company_HuisnummerInput(this IPage page) =>
            page.GetByRole(AriaRole.Textbox, new() { Name = "Huisnummer" });

        public static ILocator Company_FirstSearchButton(this IPage page) =>
            page.Locator("form").Filter(new() { HasText = "Bedrijfsnaam Zoeken" }).GetByRole(AriaRole.Button);


        public static ILocator GetSearchResultsByPostcodeAndHuisnummer(this IPage page, string postcode, string huisnummer) =>
            page.GetByRole(AriaRole.Table).Locator($"tbody tr:has(td:nth-child(4):has-text(\"{postcode.Trim()}\"))" +
                                                 $":has(td:nth-child(3):has-text(\"{huisnummer.Trim()}\"))");

        public static ILocator PersonenBsnInput(this IPage page) =>
            page.GetByRole(AriaRole.Textbox, new() { Name = "bsn" });


        public static ILocator Company_KvknummerInput(this IPage page) =>
          page.GetByRole(AriaRole.Textbox, new() { Name = "KVK-nummer of vestigingsnummer" });

        public static ILocator Company_KvknummerSearchButton(this IPage page) =>
           page.Locator("form").Filter(new() { HasText = "KVK-nummer of" }).GetByRole(AriaRole.Button);

        public static ILocator Company_PostcodeHuisnummerSearchButton(this IPage page) =>
         page.Locator("form").Filter(new() { HasText = "Postcode Huisnummer Zoeken" }).GetByRole(AriaRole.Button);

        public static ILocator SearchCompanyByPostalAndHuisNummer(this IPage page, string postcode, string huisNummer)
        {
            return page.GetByRole(AriaRole.Table).Locator($"tbody tr:has(td:nth-child(4):has-text(\"{postcode.Trim()} {huisNummer.Trim()}\"))");
        }

    }


}


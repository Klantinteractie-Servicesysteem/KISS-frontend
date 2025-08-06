namespace Kiss.Bff.EndToEndTest.AnonymousContactmomentZaak.Helpers
{

    public static class Locators
    {
        // This locator does not use an ARIA role. Consider updating the locator to use an ARIA role for better accessibility.
        public static ILocator FirstContactMomentSummary(this IPage page) => page.Locator("ul.overview > li:nth-child(2) summary");
        // This locator does not use an ARIA role. Consider updating the locator to use an ARIA role for better accessibility.
        public static ILocator GespreksresultaatHeader(this ILocator parentLocator) => parentLocator.Locator("span[aria-describedby='gespreksresultaat-header']");
        // This locator does not use an ARIA role. Consider updating the locator to use an ARIA role for better accessibility.
        public static ILocator AfdelingHeader(this ILocator parentLocator) => parentLocator.Locator("span[aria-describedby='afdeling-header']");
        // This locator does not use an ARIA role. Consider updating the locator to use an ARIA role for better accessibility.
        public static ILocator FirstContactMomentDetails(this IPage page) => page.Locator("ul.overview > li:nth-child(2) details");
        // This locator does not use an ARIA role. Consider updating the locator to use an ARIA role for better accessibility.
        public static ILocator FirstContactMomentDetailsList(this IPage page) => page.Locator("ul.overview > li:nth-child(2) dl");
        // This locator does not use an ARIA role. Consider updating the locator to use an ARIA role for better accessibility.
        public static ILocator ZaakNumber(this ILocator parentLocator) => parentLocator.Locator("dt:text('Zaaknummer') + dd");
        // This locator does not use an ARIA role. Consider updating the locator to use an ARIA role for better accessibility.
        public static ILocator ContactMomentDescription(this ILocator parentLocator) => parentLocator.Locator("dd:nth-of-type(3)");
    }
}



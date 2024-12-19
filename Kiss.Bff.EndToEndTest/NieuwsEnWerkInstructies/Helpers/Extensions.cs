namespace Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies.Helpers
{
    internal static class Extensions
    {
        public static ILocator GetNieuwsSection(this IPage page) => page.Locator("section").Filter(new() { HasText = "Nieuws" });
        public static ILocator GetWerkinstructiesSection(this IPage page) => page.Locator("section").Filter(new() { HasText = "Werkinstructies" });
        public static ILocator GetBerichtOnHomePage(this IPage page, Bericht bericht) => page.GetByRole(AriaRole.Article).Filter(new() { Has = page.GetByRole(AriaRole.Heading, new() { Name = bericht.Titel }) });

        public static async Task NavigateToNieuwsWerkinstructiesBeheer(this IPage page)
        {
            var beheerlink = page.GetByRole(AriaRole.Link, new() { Name = "Beheer" });
            var berichtenlink = page.GetByRole(AriaRole.Link, new() { Name = "Nieuws en werkinstructies" });

            await beheerlink.Or(berichtenlink).First.WaitForAsync();

            if (await beheerlink.IsVisibleAsync())
            {
                await beheerlink.ClickAsync();
            }

            await beheerlink.WaitForAsync(new() { State = WaitForSelectorState.Hidden });

            if (await berichtenlink.GetAttributeAsync("aria-current") != "page")
            {
                await berichtenlink.ClickAsync();
            }
        }
    }
}

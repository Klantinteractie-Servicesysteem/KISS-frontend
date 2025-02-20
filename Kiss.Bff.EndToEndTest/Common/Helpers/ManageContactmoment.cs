
namespace Kiss.Bff.EndToEndTest.Common.Helpers
{
    internal static class ManageContactmoment
    {
        public static async Task CreateNewContactmomentAsync(this IPage page)
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Nieuw contactmoment" }).ClickAsync();
        }
    }
}

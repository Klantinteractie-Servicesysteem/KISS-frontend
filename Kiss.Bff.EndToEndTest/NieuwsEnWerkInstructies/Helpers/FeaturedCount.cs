using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies.Helpers
{
    internal static class FeaturedCount
    {
        public static async Task<int> GetFeaturedCount(this IPage page)
        {
            // Declare featuredIndicator outside the try block so it's accessible throughout the method
            var featuredIndicator = page.Locator(".featured-indicator");
            try
            {
                await featuredIndicator.WaitForAsync(new() { Timeout = 10000 });
            }
            catch (TimeoutException)
            {
                // No featured indicator rendered within the timeout; treat as zero featured items.
                return 0;
            }

            if (await featuredIndicator.IsVisibleAsync())
            {
                var featureText = await featuredIndicator.TextContentAsync();
                return int.Parse(featureText!);
            }
            return 0;
        }
    }
}

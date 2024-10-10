using System.Xml.Linq;

namespace Kiss.Bff.EndToEndTest;

[TestClass]
public class NieuwsEnWerkInstructies : BaseTestInitializer
{
    //[TestMethod]
    //public async Task Als_ik_op_de_paginering_links_klik_navigeer_ik_naar_een_nieuwe_pagina()
    //{
    //    // Locate the 'Nieuws' section
    //    await Expect(NieuwsSection).ToBeVisibleAsync();

    //    // Locate the 'Next' page button using the pagination structure
    //    var nextPageButton = NieuwsSection.Locator("[rel='next']").First;

    //    await Expect(nextPageButton).ToBeVisibleAsync();

    //    // Click the 'Next' page button
    //    await nextPageButton.ClickAsync();

    //    // Wait for the button to ensure the page navigation has started
    //    await nextPageButton.WaitForAsync();

    //    // Verify that the first page button is still visible after navigation
    //    var firstPageButton = NieuwsSection.GetByLabel("Pagina 1");
    //    // TODO fix the pagination component. numbers should always have an aria label with the number in it
    //    //await Expect(firstPageButton).ToBeVisibleAsync();

    //    // Verify that the current page button reflects the correct page number
    //    var currentPageButton = NieuwsSection.Locator("[aria-current=page]");
    //    var page2Button = NieuwsSection.GetByLabel("Pagina 2");
    //    var page2ButtonWithAriaCurrentPage = currentPageButton.And(page2Button);

    //    // Ensure the current page button's aria-label attribute is 'Pagina 2'
    //    await Expect(page2ButtonWithAriaCurrentPage).ToBeVisibleAsync();
    //}


    //[TestMethod]
    //public async Task Als_ik_skill_filters_selecteer_worden_de_nieuwberichten_hierop_gefilterd()
    //{
    //    // Example: Test filtering by skill
    //    var categorieFilterSection = Page.Locator("details").Filter(new() { HasText = "Filter op categorie" });
    //    await Expect(categorieFilterSection).ToBeVisibleAsync();
    //    await categorieFilterSection.Locator("summary").ClickAsync();
    //    var algemeenCheckbox = categorieFilterSection.GetByRole(AriaRole.Checkbox, new() { Name = "Algemeen" });
    //    var belastingenCheckbox = categorieFilterSection.GetByRole(AriaRole.Checkbox, new() { Name = "Belastingen" });

    //    await algemeenCheckbox.CheckAsync();
    //    await belastingenCheckbox.CheckAsync();

    //    // Verify results are filtered
    //    var articles = Page.GetByRole(AriaRole.Article);
    //    await Expect(articles.First).ToBeVisibleAsync();

    //    var resultCount = await articles.CountAsync();

    //    Assert.IsTrue(resultCount > 0, "Expected to find articles after filtering by skills.");

    //    // Loop through each article and verify it contains at least one of the selected skills
    //    for (var i = 0; i < resultCount; i++)
    //    {
    //        var article = articles.Nth(i);
    //        var algemeenSkill = article.Locator("small.category-Algemeen");
    //        var belastingenSkill = article.Locator("small.category-Belastingen");
    //        await Expect(algemeenSkill.Or(belastingenSkill).First).ToBeVisibleAsync();
    //    }

    //    // Reset filters
    //    await algemeenCheckbox.UncheckAsync();
    //    await belastingenCheckbox.UncheckAsync();
    //}

    [TestMethod]
    public async Task Als_ik_een_oud_bericht_update_komt_deze_bovenaan()
    {
        try
        {
            // Check if old test messages exist
            var oldTestMessageLocator = Page.Locator("article:has-text('8e600d44-81fb-4302-9675-31b687619026')");
            if (await oldTestMessageLocator.IsVisibleAsync())
            {
                await DeleteBericht("8e600d44-81fb-4302-9675-31b687619026");
                await DeleteBericht("724e44a3-6ba1-4e92-85c3-d44e35238f4a");
                await DeleteBericht("5b8277a7-fb1a-4358-8099-24b9487b29bc");
            }


            // Step 2: Create Message A with the publish date one minute in the past
            await CreateBericht("Message A: 8e600d44-81fb-4302-9675-31b687619026", false, "", TimeSpan.FromMinutes(-1));

            // Create Message B and C with the current publish date
            await CreateBericht("Message B: 724e44a3-6ba1-4e92-85c3-d44e35238f4a", false, "");
            await CreateBericht("Important Message C: 5b8277a7-fb1a-4358-8099-24b9487b29bc", true, "");

            // Go to the page and retrieve the order of articles
            await Page.GotoAsync("/");
            var allArticles = NieuwsSection.GetByRole(AriaRole.Article);

            // Dictionary to hold article positions
            var orderOnPage = new Dictionary<string, int>();
            for (var index = 0; index < await allArticles.CountAsync(); index++)
            {
                var element = allArticles.Nth(index);
                var innerHtml = await element.InnerTextAsync();

                if (innerHtml.Contains("Message A: 8e600d44-81fb-4302-9675-31b687619026"))
                {
                    orderOnPage.Add("Message A", index);
                }
                if (innerHtml.Contains("Message B: 724e44a3-6ba1-4e92-85c3-d44e35238f4a"))
                {
                    orderOnPage.Add("Message B", index);
                }
                if (innerHtml.Contains("Message C: 5b8277a7-fb1a-4358-8099-24b9487b29bc"))
                {
                    orderOnPage.Add("Message C", index);
                }
            }

            // Assert the initial order: A (lowest), B, C (highest)
            var indexVanA = orderOnPage["Message A"];
            var indexVanB = orderOnPage["Message B"];
            var indexVanC = orderOnPage["Message C"];

            Assert.IsTrue(indexVanC < indexVanB && indexVanB < indexVanA, "Initial order should be C, B, A.");

            // Act: Update message A
            await UpdateBericht("Message A: 8e600d44-81fb-4302-9675-31b687619026", "Updated Message A: 8e600d44-81fb-4302-9675-31b687619026");

            // Refresh page and retrieve articles again
            await Page.GotoAsync("/");
            allArticles = NieuwsSection.GetByRole(AriaRole.Article);

            // Rebuild the dictionary for updated positions
            orderOnPage.Clear();
            for (var index = 0; index < await allArticles.CountAsync(); index++)
            {
                var element = allArticles.Nth(index);
                var innerHtml = await element.InnerTextAsync();

                if (innerHtml.Contains("Updated Message A: 8e600d44-81fb-4302-9675-31b687619026"))
                {
                    orderOnPage.Add("Message A", index);
                }
                if (innerHtml.Contains("Message B: 724e44a3-6ba1-4e92-85c3-d44e35238f4a"))
                {
                    orderOnPage.Add("Message B", index);
                }
                if (innerHtml.Contains("Message C: 5b8277a7-fb1a-4358-8099-24b9487b29bc"))
                {
                    orderOnPage.Add("Message C", index);
                }
            }

            // Assert the updated order: C (highest), B, A (lowest)
            indexVanA = orderOnPage["Message A"];
            indexVanB = orderOnPage["Message B"];
            indexVanC = orderOnPage["Message C"];

            Assert.IsTrue(indexVanC < indexVanB && indexVanB > indexVanA, "Updated order should be C, A, B.");
        }
        finally
        {
            // Clean up test messages
            await DeleteBericht("8e600d44-81fb-4302-9675-31b687619026");
            await DeleteBericht("724e44a3-6ba1-4e92-85c3-d44e35238f4a");
            await DeleteBericht("5b8277a7-fb1a-4358-8099-24b9487b29bc");
        }
    }


    //[TestMethod]
    //public async Task Als_ik_een_belangrijk_bericht_publiceer_komt_deze_bovenaan()
    //{
    //    var titel = $"End to end test {Guid.NewGuid()}";
    //    var featuredIndicator = Page.Locator(".featured-indicator");
    //    var intitialFeatureCount = await featuredIndicator.IsVisibleAsync()
    //        && int.TryParse(await featuredIndicator.TextContentAsync(), out var c)
    //            ? c
    //            : 0;

    //    await CreateBericht(titel, true, "");

    //    try
    //    {
    //        await Page.GotoAsync("/");

    //        await Expect(NieuwsSection).ToBeVisibleAsync();
    //        var firstArticle = NieuwsSection.GetByRole(AriaRole.Article).First;
    //        await Expect(firstArticle).ToContainTextAsync(titel);
    //        await Expect(firstArticle).ToContainTextAsync("Belangrijk");
    //        await firstArticle.GetByRole(AriaRole.Button, new() { Name = "Markeer als gelezen" }).ClickAsync();
    //        await Page.WaitForResponseAsync(x => x.Url.Contains("featuredcount"));
    //        await Expect(featuredIndicator).ToHaveTextAsync(intitialFeatureCount.ToString());
    //    }
    //    finally
    //    {
    //        await DeleteBericht(titel);
    //    }
    //}

    //[TestMethod]
    //public async Task Als_ik_een_skill_toevoeg_wordt_deze_vermeld_in_de_filter()
    //{
    //    // Define the new skill name to be added and tested
    //    var newSkill = "Test Skill";

    //    try
    //    {
    //        // Step 1: Navigate to the Skills management page
    //        await NavigateToSkillsBeheer();

    //        // Step 2: Add the new skill
    //        await CreateSkill(newSkill);
    //        await Page.GotoAsync("/");
    //        // Step 3: Open the filter dropdown to verify the skill
    //        await Page.ClickAsync("summary:has-text('Filter op categorie')");

    //        // Step 4: Verify the newly added skill appears in the filter list as a checkbox option
    //        var addedSkillCheckbox = Page.GetByRole(AriaRole.Checkbox, new() { Name = newSkill }).First;
    //        await Expect(addedSkillCheckbox).ToBeVisibleAsync();

    //    }
    //    finally
    //    {
    //        // Optional clean-up: Remove the skill after test completion if necessary
    //        await DeleteSkill(newSkill);
    //    }
    //}

    //// Made private because the test isn't done yet, this is just a stepping stone made with the playwright editor
    //[TestMethod]
    //public async Task Als_ik_een_skill_en_nieuws_item_toevoeg_zou_ik_deze_moeten_zien_bij_filteren()
    //{
    //    var newSkill = "Test Skill";
    //    var newsTitle = "Test Nieuws Item";
    //    bool isImportant = false;

    //    try
    //    {
    //        // Step 1: Create a new skill
    //        await CreateSkill(newSkill);

    //        // Step 2: Create a news item with the new skill
    //        await CreateBericht(newsTitle, isImportant, newSkill);

    //        // Step 3: Verify that the news item appears when filtering by the new skill
    //        await Page.GotoAsync("/");

    //        await Page.ClickAsync("summary:has-text('Filter op categorie')"); // Open the filter dropdown
    //        var skillCheckbox = Page.GetByRole(AriaRole.Checkbox, new() { Name = newSkill }).First;
    //        await skillCheckbox.CheckAsync(); // Check the skill in the filter

    //        // Step 4: Verify the news item appears
    //        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = newsTitle })).ToBeVisibleAsync();
    //    }
    //    finally
    //    {
    //        await DeleteBericht(newsTitle);
    //        await DeleteSkill(newSkill);
    //    }
    //}

    private ILocator NieuwsSection => Page.Locator("section").Filter(new() { HasText = "Nieuws" });

    private async Task MarkAllNewsItems(bool read)
    {
        // Locate the 'Nieuws' section
        await Expect(NieuwsSection).ToBeVisibleAsync();

        var firstGelezenButton = NieuwsSection.GetByTitle("Markeer als gelezen").First;
        var firstOnGelezenButton = NieuwsSection.GetByTitle("Markeer als ongelezen").First;

        var buttonToClick = read
            ? firstGelezenButton
            : firstOnGelezenButton;

        var firstPage = NieuwsSection.GetByRole(AriaRole.Link).Filter(new() { HasTextRegex = new("^1$") });

        if (!await IsDisabledPage(firstPage))
        {
            await firstPage.ClickAsync();
        }

        while (true)
        {
            await Expect(firstOnGelezenButton.Or(firstGelezenButton).First).ToBeVisibleAsync();

            // Mark all news items as read on the current page
            while (await buttonToClick.IsVisibleAsync())
            {
                await buttonToClick.ClickAsync();
            }

            var nextPage = NieuwsSection.Locator("[rel='next']").First;

            if (await IsDisabledPage(nextPage))
            {
                break;
            }

            await nextPage.ClickAsync();
        }

        if (!await IsDisabledPage(firstPage))
        {
            await firstPage.ClickAsync();
        }
    }

    private async Task NavigateToNieuwsWerkinstructiesBeheer()
    {
        var beheerlink = Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" });
        var berichtenlink = Page.GetByRole(AriaRole.Link, new() { Name = "Nieuws en werkinstructies" });

        await Expect(beheerlink.Or(berichtenlink).First).ToBeVisibleAsync();

        if (await beheerlink.IsVisibleAsync())
        {
            await beheerlink.ClickAsync();
        }

        await Expect(beheerlink).ToBeVisibleAsync(new() { Visible = false });

        if (await berichtenlink.GetAttributeAsync("aria-current") != "page")
        {
            await berichtenlink.ClickAsync();
        }
    }

    private async Task NavigateToSkillsBeheer()
    {
        var beheerlink = Page.GetByRole(AriaRole.Link, new() { Name = "Beheer" });
        var skillslink = Page.GetByRole(AriaRole.Link, new() { Name = "Skills" });

        await Expect(beheerlink.Or(skillslink).First).ToBeVisibleAsync();

        if (await beheerlink.IsVisibleAsync())
        {
            await beheerlink.ClickAsync();
        }

        await Expect(beheerlink).ToBeVisibleAsync(new() { Visible = false });

        if (await skillslink.GetAttributeAsync("aria-current") != "page")
        {
            await skillslink.ClickAsync();
        }
    }

    private async Task CreateBericht(string titel, bool isBelangrijk, string skill, TimeSpan? publishDateOffset = null)
    {
        await NavigateToNieuwsWerkinstructiesBeheer();
        var toevoegenLink = Page.GetByRole(AriaRole.Link, new() { Name = "Toevoegen" });
        await toevoegenLink.ClickAsync();
        await Page.GetByRole(AriaRole.Radio, new() { Name = "Nieuws" }).CheckAsync();

        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(titel);

        // Fill in the content area
        await Page.Locator(".ck-content").WaitForAsync();
        await Page.Locator("textarea").FillAsync(titel);

        if (isBelangrijk)
        {
            await Page.GetByRole(AriaRole.Checkbox, new() { Name = "Belangrijk" }).CheckAsync();
        }

        if (!string.IsNullOrEmpty(skill))
        {
            var skillCheckbox = Page.GetByRole(AriaRole.Checkbox, new() { Name = skill });
            await skillCheckbox.CheckAsync(); // Ensure the skill checkbox is checked
        }

        // Use the current time as the base publish date
        DateTime publishDate = DateTime.Now;

        // Apply the provided offset to the publish date
        if (publishDateOffset.HasValue)
        {
            publishDate = publishDate.Add(publishDateOffset.Value);
        }

        // Set the publish date in the input field
        var publishDateInput = Page.Locator("#publicatieDatum");
        await publishDateInput.FillAsync(publishDate.ToString("yyyy-MM-ddTHH:mm"));

        var opslaanKnop = Page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
        while (await opslaanKnop.IsVisibleAsync() && await opslaanKnop.IsEnabledAsync())
        {
            await opslaanKnop.ClickAsync();
        }

        await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
    }

    private async Task UpdateBericht(string oldTitle, string newTitle)
    {
        // Navigate to the news management page
        await NavigateToNieuwsWerkinstructiesBeheer();

        // Find the news item by its old title
        var nieuwsRows = Page.GetByRole(AriaRole.Row)
            .Filter(new()
            {
                Has = Page.GetByRole(AriaRole.Cell, new() { Name = oldTitle, Exact = true })
            });

        // Click the "Details" link for the news item
        await nieuwsRows.GetByRole(AriaRole.Link, new() { Name = "Details" }).ClickAsync();

        // Update the title to the new one
        await Page.GetByLabel("Titel").FillAsync(newTitle);

        // Save the changes
        await Page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
    }


    private async Task DeleteBericht(string titel)
    {
        await NavigateToNieuwsWerkinstructiesBeheer();
        var nieuwsRows = Page.GetByRole(AriaRole.Row)
            .Filter(new()
            {
                Has = Page.GetByRole(AriaRole.Cell, new() { Name = "Nieuws" })
            })
            .Filter(new()
            {
                Has = Page.GetByRole(AriaRole.Cell, new() { Name = titel, Exact = false })
            });

        var deleteButton = nieuwsRows.GetByTitle("Verwijder");
        
        Page.Dialog += Accept;
        await deleteButton.ClickAsync();
        Page.Dialog -= Accept;
        await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
    }

    private async Task CreateSkill(string skillName)
    {
        // Step 1: Navigate to the "Skills" beheer page
        await NavigateToSkillsBeheer();

        // Step 2: Click on the "Toevoegen" button to add a new skill
        var toevoegenLink = Page.GetByRole(AriaRole.Link, new() { Name = "toevoegen" });
        await toevoegenLink.ClickAsync();

        // Step 3: Fill in the skill name in the input field
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Naam" }).FillAsync(skillName);

        // Step 4: Locate and click the "Opslaan" button to save the new skill
        var opslaanKnop = Page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });

        // Ensure that the save button is visible and enabled before clicking
        while (await opslaanKnop.IsVisibleAsync() && await opslaanKnop.IsEnabledAsync())
        {
            await opslaanKnop.ClickAsync();
        }

        // Step 5: Optionally verify that the new skill has been added
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Skills" })).ToBeVisibleAsync();
    }

    private async Task DeleteSkill(string skillName)
    {
        // Step 1: Navigate to the Skills management page
        await NavigateToSkillsBeheer();

        // Step 2: Locate the skill item by its name
        var skillLocator = Page.Locator($"li.listItem:has-text('{skillName}')").First;

        // Step 3: Check if the skill exists
        if (await skillLocator.CountAsync() > 0)
        {
            // Step 4: Click the delete button for the specified skill
            var deleteButton = skillLocator.Locator("button[title='Verwijderen']").First;
            await Expect(deleteButton).ToBeVisibleAsync();
            await Expect(deleteButton).ToBeEnabledAsync();
            if (await deleteButton.IsVisibleAsync())
            {
                // Handle the confirmation dialog
                Page.Dialog += async (sender, dialog) =>
                {
                    await dialog.AcceptAsync(); // Accept the confirmation dialog
                };

                await deleteButton.ClickAsync(); // Click the delete button
            }
            else
            {
                throw new Exception("Delete button is not visible.");
            }

            // Step 7: Verify the skill is no longer present in the list
            await Expect(skillLocator).ToBeHiddenAsync();
        }
        else
        {
            // Handle the case where the skill does not exist
            throw new Exception($"Skill '{skillName}' not found for deletion.");
        }
    }

    static async void Accept(object? _, IDialog dialog) => await dialog.AcceptAsync();

    private async Task<bool> IsDisabledPage(ILocator locator)
    {
        await Expect(locator).ToBeVisibleAsync();

        var classes = await locator.GetAttributeAsync("class");
        if (classes == null) return false;
        // we always have a next page link, but sometimes it is disabled. TO DO: use disabled attribute so we don't have to rely on classes
        return classes.Contains("denhaag-pagination__link--disabled")
            || classes.Contains("denhaag-pagination__link--current");
    }
}

using Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies.Helpers;

namespace Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies;

[TestClass]
public class Scenarios : BaseTestInitializer
{
    [TestMethod]
    public async Task Scenario1()
    {
        await Step("When navigates through the HOME Page");
        await Page.GotoAsync("/");

        await Step("Then nieuws items are displayed");
        await Expect(Page.GetNieuwsSection().GetByRole(AriaRole.Article).First).ToBeVisibleAsync();

        await Step("And werkinstructies are displayed");
        await Expect(Page.GetWerkinstructiesSection().GetByRole(AriaRole.Article).First).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Scenario2()
    {
        await Step("Given there is an important message");
        await using var testbericht = await Page.CreateBericht(new() { Titel = "Playwright test bericht belangrijk", IsBelangrijk = true });

        await Step("When navigates through the HOME Page");
        await Page.GotoAsync("/");

        await Step("Then count of the important messages displayed in the News and Instructions tabs.");
        var count = await GetFeaturedCount();
        Assert.AreNotEqual(0, count);
    }

    [TestMethod]
    public async Task Scenario3()
    {
        await Step("Given there is a nieuws item");

        await using var testbericht = await Page.CreateBericht(new() { Titel = "Playwright test bericht", Inhoud = "Inhoud die we gaan verbergen" });
        var article = Page.GetBerichtOnHomePage(testbericht);
        var markeerGelezenButton = article.GetByRole(AriaRole.Button).And(article.GetByTitle("Markeer als gelezen"));
        var markeerOngelezenButton = article.GetByRole(AriaRole.Button).And(article.GetByTitle("Markeer als ongelezen"));
        var body = article.GetByText(testbericht.Inhoud!);

        await Step("When navigates through the HOME Page");
        await Page.GotoAsync("/");

        await Step("And clicks on the book icon next to the news icon");
        await markeerGelezenButton.ClickAsync();

        await Step("Then button changes to 'markeer ongelezen'");
        await Expect(markeerGelezenButton).ToBeHiddenAsync();
        await Expect(markeerOngelezenButton).ToBeVisibleAsync();

        await Step("And only title is displayed ");
        await Expect(body).ToBeHiddenAsync();
    }

    [TestMethod]
    public async Task Scenario4()
    {
        await Step("Given there are at least 20 news articles");
        var berichtRequests = Enumerable.Range(1, 20)
            .Select(x => new CreateBerichtRequest
            {
                Titel = "Playwright test bericht " + x
            });

        await using var berichten = await Page.CreateBerichten(berichtRequests);

        await Step("When navigates through the HOME Page");
        await Page.GotoAsync("/");

        // Locate the 'Nieuws' section
        await Expect(Page.GetNieuwsSection()).ToBeVisibleAsync();
        // Locate the 'Next' page button using the pagination structure
        var nextPageButton = Page.GetNieuwsSection().Locator("[rel='next']").First;

        await Step("And clicks on the \"Next\" button to go to the next page");
        await nextPageButton.ClickAsync();

        await Step("Then should see 10 new articles on the next page");
        await Expect(Page.GetNieuwsSection().GetByRole(AriaRole.Article)).ToHaveCountAsync(10);

        await Step("And the current page number should be 2");
        var currentPageButton = Page.GetNieuwsSection().Locator("[aria-current=page]");
        var page2Button = Page.GetNieuwsSection().GetByLabel("Pagina 2");
        await Expect(currentPageButton.And(page2Button)).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Scenario5()
    {
        await Step("Given there are at least 20 news articles");
        var berichtRequests = Enumerable.Range(1, 20)
            .Select(x => new CreateBerichtRequest
            {
                Titel = "Playwright test bericht " + x
            });

        await using var berichten = await Page.CreateBerichten(berichtRequests);

        await Step("And is on the HOME Page");
        await Page.GotoAsync("/");

        await Expect(Page.GetNieuwsSection()).ToBeVisibleAsync();

        // Locate the 'Next' page button using the pagination structure
        var nextPageButton = Page.GetNieuwsSection().Locator("[rel='next']").First;
        await Step("And is on page 2 with 10 articles displayed");
        await nextPageButton.ClickAsync();
        await Expect(Page.GetNieuwsSection().GetByRole(AriaRole.Article)).ToHaveCountAsync(10);

        await Step("When clicks on the \"Previous\" button");
        var previousPageButton = Page.GetNieuwsSection().Locator("[rel='prev']").First;
        await previousPageButton.ClickAsync();

        await Step("Then should see 10 new articles on the next page");
        await Expect(Page.GetNieuwsSection().GetByRole(AriaRole.Article)).ToHaveCountAsync(10);

        await Step("And the current page number should be 1");
        var currentPageButton = Page.GetNieuwsSection().Locator("[aria-current=page]");
        var page1Button = Page.GetNieuwsSection().GetByLabel("Pagina 1");
        var currentPageButtonWithPage1Text = currentPageButton.And(page1Button);

        await Expect(currentPageButtonWithPage1Text).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Scenario6()
    {
        await Step("Given there are at least 20 werkinstructies");
        var berichtRequests = Enumerable.Range(1, 20)
            .Select(x => new CreateBerichtRequest
            {
                Titel = "Playwright test bericht " + x,
                BerichtType = BerichtType.Werkinstructie
            });

        await using var berichten = await Page.CreateBerichten(berichtRequests);

        await Step("And is on the HOME Page");
        await Page.GotoAsync("/");
        await Expect(Page.GetWerkinstructiesSection()).ToBeVisibleAsync();

        // Locate the 'Next' page button using the pagination structure
        var nextPageButton = Page.GetWerkinstructiesSection().Locator("[rel='next']").First;
        await Step("And is on page 2 with 10 articles displayed");
        await nextPageButton.ClickAsync();
        await Expect(Page.GetWerkinstructiesSection().GetByRole(AriaRole.Article)).ToHaveCountAsync(10);

        await Step("When clicks on the \"Previous\" button");
        var previousPageButton = Page.GetWerkinstructiesSection().Locator("[rel='prev']").First;
        await previousPageButton.ClickAsync();

        await Step("Then should see 10 new articles on the next page");
        await Expect(Page.GetWerkinstructiesSection().GetByRole(AriaRole.Article)).ToHaveCountAsync(10);

        await Step("And the current page number should be 1");
        var currentPageButton = Page.GetWerkinstructiesSection().Locator("[aria-current=page]");
        var page1Button = Page.GetWerkinstructiesSection().GetByLabel("Pagina 1");
        var currentPageButtonWithPage1Text = currentPageButton.And(page1Button);

        await Expect(currentPageButtonWithPage1Text).ToBeVisibleAsync();
    }

    [TestMethod]
    public async Task Scenario7()
    {
        await Step("Given there are at least 20 werkinstructies");
        var berichtRequests = Enumerable.Range(1, 20)
            .Select(x => new CreateBerichtRequest
            {
                Titel = "Playwright test bericht " + x,
                BerichtType = BerichtType.Werkinstructie
            });

        await using var berichten = await Page.CreateBerichten(berichtRequests);

        await Step("And is on the HOME Page");
        await Page.GotoAsync("/");
        await Expect(Page.GetWerkinstructiesSection()).ToBeVisibleAsync();

        // Locate the 'Next' page button using the pagination structure
        var nextPageButton = Page.GetWerkinstructiesSection().Locator("[rel='next']").First;
        var werkinstructies = Page.GetWerkinstructiesSection().GetByRole(AriaRole.Article);
        
        await Step("And is on the last page of articles");

        // keep clicking on the next page button until it's disabled
        while (!await IsDisabledPage(nextPageButton))
        {
            await nextPageButton.ClickAsync();
            await werkinstructies.First.WaitForAsync();
        }

        await Step("When clicks on the \"Next\" button");

        await Assert.ThrowsExceptionAsync<TimeoutException>(
            () => nextPageButton.ClickAsync(new() { Timeout = 1000 }),
            "Expected the button to not be clickable, but it was");

        await Step("Then should remain on the last page");
    }


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

    // Dit test Stap 2. 8. 9. 10. 15. 
    //[TestMethod]
    //public async Task Als_ik_een_oud_bericht_update_komt_deze_bovenaan()
    //{
    //    try
    //    {
    //        // Check if old test messages exist
    //        var oldTestMessageLocator = Page.Locator("article:has-text('8e600d44-81fb-4302-9675-31b687619026')");
    //        if (await oldTestMessageLocator.IsVisibleAsync())
    //        {
    //            await DeleteBericht("8e600d44-81fb-4302-9675-31b687619026");
    //            await DeleteBericht("724e44a3-6ba1-4e92-85c3-d44e35238f4a");
    //            await DeleteBericht("5b8277a7-fb1a-4358-8099-24b9487b29bc");
    //        }


    //        // Step 2: Create Message A with the publish date one minute in the past
    //        await CreateBericht("Message A: 8e600d44-81fb-4302-9675-31b687619026", false, "", TimeSpan.FromMinutes(-1));

    //        // Create Message B and C with the current publish date
    //        await CreateBericht("Message B: 724e44a3-6ba1-4e92-85c3-d44e35238f4a", false, "");
    //        await CreateBericht("Important Message C: 5b8277a7-fb1a-4358-8099-24b9487b29bc", true, "");

    //        // Go to the page and retrieve the order of articles
    //        await Page.GotoAsync("/");

    //        await Page.WaitForSelectorAsync("article:has-text('Message A')");
    //        await Page.WaitForSelectorAsync("article:has-text('Message B')");
    //        await Page.WaitForSelectorAsync("article:has-text('Message C')");

    //        var allArticles = NieuwsSection.GetByRole(AriaRole.Article);

    //        // Dictionary to hold article positions
    //        var initialOrderOnPage = new Dictionary<string, int>();

    //        for (var index = 0; index < await allArticles.CountAsync(); index++)
    //        {
    //            var element = allArticles.Nth(index);
    //            var innerHtml = await element.InnerTextAsync();

    //            if (innerHtml.Contains("Message A: 8e600d44-81fb-4302-9675-31b687619026"))
    //            {
    //                initialOrderOnPage.Add("Message A", index);
    //            }
    //            if (innerHtml.Contains("Message B: 724e44a3-6ba1-4e92-85c3-d44e35238f4a"))
    //            {
    //                initialOrderOnPage.Add("Message B", index);
    //            }
    //            if (innerHtml.Contains("Message C: 5b8277a7-fb1a-4358-8099-24b9487b29bc"))
    //            {
    //                initialOrderOnPage.Add("Message C", index);
    //            }
    //        }

    //        var indexVanA = initialOrderOnPage["Message A"];
    //        var indexVanB = initialOrderOnPage["Message B"];
    //        var indexVanC = initialOrderOnPage["Message C"];

    //        Assert.IsTrue(indexVanC < indexVanB && indexVanB < indexVanA, "Initial order should be C, B, A.");

    //        // Act: Update message A
    //        await UpdateBericht("Message A: 8e600d44-81fb-4302-9675-31b687619026", "Updated Message A: 8e600d44-81fb-4302-9675-31b687619026");

    //        // Refresh page and retrieve articles again
    //        await Page.GotoAsync("/");

    //        await Page.WaitForSelectorAsync("article:has-text('Message A')");
    //        await Page.WaitForSelectorAsync("article:has-text('Message B')");
    //        await Page.WaitForSelectorAsync("article:has-text('Message C')");

    //        allArticles = NieuwsSection.GetByRole(AriaRole.Article);

    //        // Rebuild the dictionary for updated positions
    //        var orderOnPageAfterMessageUpdate = new Dictionary<string, int>();
    //        for (var index = 0; index < await allArticles.CountAsync(); index++)
    //        {
    //            var element = allArticles.Nth(index);
    //            var innerHtml = await element.InnerTextAsync();

    //            if (innerHtml.Contains("Updated Message A: 8e600d44-81fb-4302-9675-31b687619026"))
    //            {
    //                orderOnPageAfterMessageUpdate.Add("Message A", index);
    //            }
    //            if (innerHtml.Contains("Message B: 724e44a3-6ba1-4e92-85c3-d44e35238f4a"))
    //            {
    //                orderOnPageAfterMessageUpdate.Add("Message B", index);
    //            }
    //            if (innerHtml.Contains("Message C: 5b8277a7-fb1a-4358-8099-24b9487b29bc"))
    //            {
    //                orderOnPageAfterMessageUpdate.Add("Message C", index);
    //            }
    //        }

    //        // Assert the updated order: C (highest), B, A (lowest)
    //        indexVanA = orderOnPageAfterMessageUpdate["Message A"];
    //        indexVanB = orderOnPageAfterMessageUpdate["Message B"];
    //        indexVanC = orderOnPageAfterMessageUpdate["Message C"];

    //        Assert.IsTrue(indexVanC < indexVanB && indexVanB > indexVanA, "Updated order should be C, A, B.");
    //    }
    //    finally
    //    {
    //        // Clean up test messages
    //        await DeleteBericht("8e600d44-81fb-4302-9675-31b687619026");
    //        await DeleteBericht("724e44a3-6ba1-4e92-85c3-d44e35238f4a");
    //        await DeleteBericht("5b8277a7-fb1a-4358-8099-24b9487b29bc");
    //    }
    //}

    //// 9. Publiceer een bericht met markering Belangrijk 
    //[TestMethod]
    //public async Task Als_ik_een_belangrijk_bericht_publiceer_komt_deze_bovenaan()
    //{
    //    var titel = $"End to end test {Guid.NewGuid()}";
    //    // Step 1: Get the initial featured indicator count
    //    var initialFeatureCount = await GetFeaturedCount();

    //    // Step 2: Create a new important message
    //    await CreateBericht(titel, true, "");

    //    try
    //    {
    //        // Step 3: Go to the page and ensure the news section is visible
    //        await Page.GotoAsync("/");

    //        await Expect(NieuwsSection).ToBeVisibleAsync();

    //        // Step 4: Check if the newly created important message appears at the top
    //        var firstArticle = NieuwsSection.GetByRole(AriaRole.Article).First;
    //        await Expect(firstArticle).ToContainTextAsync(titel);
    //        var isBelangrijk = await firstArticle.Locator(".featured").IsVisibleAsync();

    //        // Ensure the first article contains "Belangrijk" only if it's supposed to
    //        if (isBelangrijk)
    //        {
    //            await Expect(firstArticle.Locator(".featured")).ToContainTextAsync("Belangrijk");
    //        }
    //        else
    //        {
    //            Console.WriteLine("This article does not contain the 'Belangrijk' tag.");
    //        }

    //        // Step 5: Get the new featured count
    //        var updatedCount = await GetFeaturedCount();
    //        Assert.IsTrue(updatedCount >= initialFeatureCount + 1, $"Expected featured count to be at least {initialFeatureCount + 1}, but got {updatedCount}");

    //        // Step 6: Mark the article as read
    //        await firstArticle.GetByRole(AriaRole.Button, new() { Name = "Markeer als gelezen" }).ClickAsync();

    //        // Step 7: Validate that the featured count is now back to the initial count
    //        var reUpdatedCount = await GetFeaturedCount();
    //        Assert.IsTrue(reUpdatedCount == initialFeatureCount, $"Expected featured count to be equal to the initial count {initialFeatureCount} again, but instead got {reUpdatedCount}");
    //    }
    //    finally
    //    {
    //        // Step 8: Clean up by deleting the created message
    //        await DeleteBericht(titel);
    //    }
    //}

    private async Task<int> GetFeaturedCount()
    {
        // Declare featuredIndicator outside the try block so it's accessible throughout the method
        var featuredIndicator = Page.Locator(".featured-indicator");
        await Page.WaitForResponseAsync(x => x.Url.Contains("featuredcount"));
        if (await featuredIndicator.IsVisibleAsync())
        {
            var featureText = await featuredIndicator.TextContentAsync();
            return int.Parse(featureText!);
        }
        return 0;
    }


    //// This test covers Step 12. 13. 14.
    //[TestMethod]
    //public async Task Als_ik_een_skill_toevoeg_wordt_deze_vermeld_in_de_filter()
    //{
    //    // Define the new skill name to be added and tested
    //    var newSkill = "Playwright Test Skill";

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
    //        // clean-up: Remove the skill after test completion
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
        var publishDate = DateTime.Now;

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
                Has = Page.GetByRole(AriaRole.Cell, new() { Name = "Nieuws" }).First
            })
            .Filter(new()
            {
                Has = Page.GetByRole(AriaRole.Cell, new() { Name = titel, Exact = false }).First
            });

        var deleteButton = nieuwsRows.GetByTitle("Verwijder").First;

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

        // Step 2: Locate the skill listitem by its name
        var skillLocator = Page.GetByRole(AriaRole.Listitem).Filter(new() { HasText = skillName });

        // Step 3: Locate the delete button within the listitem
        var deleteButton = skillLocator.GetByRole(AriaRole.Button).And(Page.GetByTitle("Verwijderen"));

        // Step 4: Click the delete button and accept the dialog
        Page.Dialog += Accept;
        await deleteButton.ClickAsync();

        // Step 5: Verify the skill is no longer present in the list
        await Expect(skillLocator).ToBeHiddenAsync();
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

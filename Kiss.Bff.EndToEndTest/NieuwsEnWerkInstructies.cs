﻿namespace Kiss.Bff.EndToEndTest;

[TestClass]
public class NieuwsEnWerkInstructies : BaseTestInitializer
{
    [TestMethod]
    public async Task Als_ik_op_de_paginering_links_klik_navigeer_ik_naar_een_nieuwe_pagina()
    {
        // Locate the 'Nieuws' section
        await Expect(NieuwsSection).ToBeVisibleAsync();

        // Locate the 'Next' page button using the pagination structure
        var nextPageButton = NieuwsSection.Locator("[rel='next']").First;

        await Expect(nextPageButton).ToBeVisibleAsync();

        // Click the 'Next' page button
        await nextPageButton.ClickAsync();

        // Wait for the button to ensure the page navigation has started
        await nextPageButton.WaitForAsync();

        // Verify that the first page button is still visible after navigation
        var firstPageButton = NieuwsSection.GetByLabel("Pagina 1");
        // TODO fix the pagination component. numbers should always have an aria label with the number in it
        //await Expect(firstPageButton).ToBeVisibleAsync();

        // Verify that the current page button reflects the correct page number
        var currentPageButton = NieuwsSection.Locator("[aria-current=page]");
        var page2Button = NieuwsSection.GetByLabel("Pagina 2");
        var page2ButtonWithAriaCurrentPage = currentPageButton.And(page2Button);

        // Ensure the current page button's aria-label attribute is 'Pagina 2'
        await Expect(page2ButtonWithAriaCurrentPage).ToBeVisibleAsync();
    }


    [TestMethod]
    public async Task Als_ik_skill_filters_selecteer_worden_de_nieuwberichten_hierop_gefilterd()
    {
        // Example: Test filtering by skill
        var categorieFilterSection = Page.Locator("details").Filter(new() { HasText = "Filter op categorie" });
        await Expect(categorieFilterSection).ToBeVisibleAsync();
        await categorieFilterSection.Locator("summary").ClickAsync();
        var algemeenCheckbox = categorieFilterSection.GetByRole(AriaRole.Checkbox, new() { Name = "Algemeen" });
        var belastingenCheckbox = categorieFilterSection.GetByRole(AriaRole.Checkbox, new() { Name = "Belastingen" });

        await algemeenCheckbox.CheckAsync();
        await belastingenCheckbox.CheckAsync();

        // Verify results are filtered
        var articles = Page.GetByRole(AriaRole.Article);
        await Expect(articles.First).ToBeVisibleAsync();

        var resultCount = await articles.CountAsync();

        Assert.IsTrue(resultCount > 0, "Expected to find articles after filtering by skills.");

        // Loop through each article and verify it contains at least one of the selected skills
        for (var i = 0; i < resultCount; i++)
        {
            var article = articles.Nth(i);
            var algemeenSkill = article.Locator("small.category-Algemeen");
            var belastingenSkill = article.Locator("small.category-Belastingen");
            await Expect(algemeenSkill.Or(belastingenSkill).First).ToBeVisibleAsync();
        }

        // Reset filters
        await algemeenCheckbox.UncheckAsync();
        await belastingenCheckbox.UncheckAsync();
    }

    [TestMethod]
    public async Task Als_ik_een_oud_bericht_update_komt_deze_bovenaan()
    {
        await MarkAllNewsItems(true);
        var (title, isBelangrijk) = await UpdateLastNotImportantNieuwsberichtInBeheer();

        // navigate back to home
        await Page.GotoAsync("/");

        var articles = NieuwsSection.GetByRole(AriaRole.Article);

        var firstArticle = isBelangrijk
            ? articles.First
            : articles.Filter(new LocatorFilterOptions() { HasNot = Page.Locator(".featured") }).First;

        var heading = firstArticle.GetByRole(AriaRole.Heading).First;

        await Expect(heading).ToHaveTextAsync(title);
        await MarkAllNewsItems(false);
    }

    [TestMethod]
    public async Task Als_ik_een_belangrijk_bericht_publiceer_komt_deze_bovenaan()
    {
        var titel = $"End to end test {Guid.NewGuid()}";
        var featuredIndicator = Page.Locator(".featured-indicator");
        var intitialFeatureCount = await featuredIndicator.IsVisibleAsync()
            && int.TryParse(await featuredIndicator.TextContentAsync(), out var c)
                ? c
                : 0;

        await CreateBericht(titel, true);
        
        try
        {
            await Page.GotoAsync("/");

            await Expect(NieuwsSection).ToBeVisibleAsync();
            var firstArticle = NieuwsSection.GetByRole(AriaRole.Article).First;
            await Expect(firstArticle).ToContainTextAsync(titel);
            await Expect(firstArticle).ToContainTextAsync("Belangrijk");
            await firstArticle.GetByRole(AriaRole.Button, new() { Name = "Markeer als gelezen" }).ClickAsync();
            await Page.WaitForResponseAsync(x => x.Url.Contains("featuredcount"));
            await Expect(featuredIndicator).ToHaveTextAsync(intitialFeatureCount.ToString());
        }
        finally
        {
            await DeleteBericht(titel);
        }
    }

    // Made private because the test isn't done yet, this is just a stepping stone made with the playwright editor
    [TestMethod]
    private async Task MarkAsImportant()
    {
        // Example: Mark a news item as important
        await Page.ClickAsync("a:has-text('Details')");
        await Page.CheckAsync("input[name='Belangrijk']");
        await Page.ClickAsync("button:has-text('Opslaan')");
        await Page.WaitForURLAsync("https://dev.kiss-demo.nl/Beheer/NieuwsEnWerkinstructies");

        // Verify the item is marked as important
        Assert.IsTrue(await Page.IsVisibleAsync("a:has-text('Belangrijk')"));
    }

    // Made private because the test isn't done yet, this is just a stepping stone made with the playwright editor
    [TestMethod]
    private async Task TestAddAndVerifySkillAsync()
    {
        // Add a skill
        await Page.ClickAsync("a:has-text('Skills')");
        await Page.FillAsync("input[name='new-skill']", "Test Skill");
        await Page.ClickAsync("button:has-text('Voeg toe')");

        // Verify the skill appears in the dropdown
        await Page.ClickAsync("summary:has-text('Filter op categorie')");
        Assert.IsTrue(await Page.IsVisibleAsync("input[name='Test Skill']"));
    }

    // Made private because the test isn't done yet, this is just a stepping stone made with the playwright editor
    [TestMethod]
    private async Task TestDeleteSkillAsync()
    {
        // Delete the skill
        await Page.ClickAsync("a:has-text('Skills')");
        await Page.ClickAsync("button:has-text('Verwijder')");

        // Verify the skill is removed
        await Page.ClickAsync("summary:has-text('Filter op categorie')");
        Assert.IsFalse(await Page.IsVisibleAsync("input[name='Test Skill']"));
    }

    // Made private because the test isn't done yet, this is just a stepping stone made with the playwright editor
    [TestMethod]
    private async Task TestModifyAndVerifyNewsItemAsync()
    {
        // Modify an existing news item
        await Page.ClickAsync("a:has-text('Details')");
        await Page.FillAsync("textarea[name='editor']", "Updated Content");
        await Page.ClickAsync("button:has-text('Opslaan')");
        await Page.WaitForURLAsync("https://dev.kiss-demo.nl/Beheer/NieuwsEnWerkinstructies");

        // Verify the modification
        Assert.IsTrue(await Page.IsVisibleAsync("article:has-text('Updated Content')"));
    }

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

    private async Task<(string Titel, bool IsBelangrijk)> UpdateLastNotImportantNieuwsberichtInBeheer()
    {
        await NavigateToNieuwsWerkinstructiesBeheer();

        var nieuwsRows = Page.GetByRole(AriaRole.Row)
            .Filter(new()
            {
                Has = Page.GetByRole(AriaRole.Cell, new() { Name = "Nieuws" })
            });

        await nieuwsRows.GetByRole(AriaRole.Link, new() { Name = "Details" }).Last.ClickAsync();

        var titel = await Page.GetByLabel("Titel").InputValueAsync();
        var isBelangrijk = await Page.GetByLabel("Belangrijk").IsCheckedAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();

        return (titel, isBelangrijk);
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

    private async Task CreateBericht(string titel, bool isBelangrijk)
    {
        await NavigateToNieuwsWerkinstructiesBeheer();
        var toevoegenLink = Page.GetByRole(AriaRole.Link, new() { Name = "Toevoegen" });
        await toevoegenLink.ClickAsync();
        await Page.GetByRole(AriaRole.Radio, new() { Name = "Nieuws" }).CheckAsync();
        
        await Page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(titel);
        // TODO label van inhoud wordt niet geassocieerd met de input
        // await Page.GetByRole(AriaRole.Textbox, new() { Name = "Inhoud" }).FillAsync(titel);
        await Page.Locator(".ck-content").WaitForAsync();
        await Page.Locator("textarea").FillAsync(titel);

        if (isBelangrijk)
        {
            await Page.GetByRole(AriaRole.Checkbox, new() { Name = "Belangrijk" }).CheckAsync();
        }

        var opslaanKnop = Page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });

        while (await opslaanKnop.IsVisibleAsync() && await opslaanKnop.IsEnabledAsync())
        {
            await opslaanKnop.ClickAsync();
        }
        
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
                Has = Page.GetByRole(AriaRole.Cell, new() { Name = titel, Exact = true })
            });

        var deleteButton = nieuwsRows.GetByTitle("Verwijder");
        
        Page.Dialog += Accept;
        await deleteButton.ClickAsync();
        Page.Dialog -= Accept;
        await Expect(Page.GetByRole(AriaRole.Table)).ToBeVisibleAsync();
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

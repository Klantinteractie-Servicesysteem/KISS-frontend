﻿using System;
using Kiss.Bff.EndToEndTest.Helpers;

namespace Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies.Helpers
{
    internal static class ManageBerichtExtension
    {
        public static async Task<IAsyncDisposable> CreateBerichten(this IPage page, IEnumerable<CreateBerichtRequest> requests)
        {
            var berichten = new List<Bericht>();
            async ValueTask Dispose()
            {
                if (berichten.Count == 0) return;
                await page.Context.Tracing.GroupEndAsync();
                await page.Context.Tracing.GroupAsync("Cleanup berichten");
                await page.Context.Tracing.GroupAsync("Start cleanup");
                foreach (var item in berichten)
                {
                    try
                    {
                        await item.DisposeAsync();
                    }
                    catch (Exception)
                    {
                    }
                }
                await page.Context.Tracing.GroupEndAsync();
            }
            try
            {
                foreach (var item in requests)
                {
                    var bericht = await page.CreateBerichtAsync(item);
                    berichten.Add(bericht);
                }
                return new DoOnDisposeAsync(Dispose);
            }
            catch (Exception)
            {
                await Dispose();
                throw;
            }
        }

        private class DoOnDisposeAsync(Func<ValueTask> thingToDo) : IAsyncDisposable
        {
            public ValueTask DisposeAsync() => thingToDo();
        }

        public static async Task<Bericht> CreateBerichtAsync(this IPage page, CreateBerichtRequest request)
        {
            request = request with { Body = !string.IsNullOrWhiteSpace(request.Body) ? request.Body : request.Title };
            await page.NavigateToNieuwsWerkinstructiesBeheer();
            var toevoegenLink = page.GetByRole(AriaRole.Link, new() { Name = "Toevoegen" });
            await toevoegenLink.ClickAsync();
            await page.GetByRole(AriaRole.Radio, new() { Name = request.BerichtType.ToString() }).CheckAsync();

            await page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(request.Title);

            await page.GetByRole(AriaRole.Textbox, new() { Name = "Rich Text Editor" }).FillAsync(request.Body);

            if (request.IsImportant)
            {
                await page.GetByRole(AriaRole.Checkbox, new() { Name = "Belangrijk" }).CheckAsync();
            }

            if (!string.IsNullOrEmpty(request.Skill))
            {
                List<string> skills = new();

                if (request.Skill.Contains(","))
                    skills.AddRange(request.Skill.Split(","));
                else
                    skills.Add(request.Skill);

                foreach (var skill in skills)
                {
                    var skillCheckbox = page.GetByRole(AriaRole.Checkbox, new() { Name = skill });
                    await skillCheckbox.CheckAsync(); // Ensure the skill checkbox is checked
                }
            }

            // Use the current time as the base publish date
            var publishDate = DateTime.Now;

            // Apply the provided offset to the publish date
            if (request.PublishDateOffset.HasValue)
            {
                publishDate = publishDate.Add(request.PublishDateOffset.Value);
            }

            // Set the publish date in the input field
            var publishDateInput = page.GetByLabel("Publicatiedatum");
            await publishDateInput.FillAsync(publishDate.ToString("yyyy-MM-ddTHH:mm"));

            var PublicatieEinddatumInput = page.GetByLabel("Publicatie-einddatum");
            await PublicatieEinddatumInput.FillAsync(request.PublicatieEinddatum.ToString("yyyy-MM-ddTHH:mm"));

            var opslaanKnop = page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
            while (await opslaanKnop.IsVisibleAsync() && await opslaanKnop.IsEnabledAsync())
            {
                await opslaanKnop.ClickAsync();
            }

            await page.GetByRole(AriaRole.Table).WaitForAsync();
            return new(page)
            {
                Key = request.Title,
                IsImportant = request.IsImportant,
                Title = request.Title,
                PublishDateOffset = request.PublishDateOffset,
                PublicatieDatum = publishDate,
                PublicatieEinddatum = request.PublicatieEinddatum,
                Skill = request.Skill,
                Body = request.Body,
                BerichtType = request.BerichtType,
            };
        }
        public static async Task UpdateBerichtAsync(this IPage page, Bericht bericht)
        {
            await page.NavigateToNieuwsWerkinstructiesBeheer();
            var row = page.GetBeheerRowByValue(bericht.Key);
            await row.GetByRole(AriaRole.Link).ClickAsync();

            // Update fields
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(bericht.Title);
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Rich Text Editor" }).FillAsync(bericht.Body);

            var belangrijk = page.GetByRole(AriaRole.Checkbox, new() { Name = "Belangrijk" });
            if (bericht.IsImportant)
                await belangrijk.CheckAsync();
            else
                await belangrijk.UncheckAsync();

            if (!string.IsNullOrEmpty(bericht.Skill))
            {
                var skillCheckbox = page.GetByRole(AriaRole.Checkbox, new() { Name = bericht.Skill });
                await skillCheckbox.CheckAsync();
            }

            await page.GetByLabel("Publicatiedatum").FillAsync(bericht.PublicatieDatum.ToString("yyyy-MM-ddTHH:mm"));
            await page.GetByLabel("Publicatie-einddatum").FillAsync(bericht.PublicatieEinddatum.ToString("yyyy-MM-ddTHH:mm"));

            var opslaanKnop = page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
            while (await opslaanKnop.IsVisibleAsync() && await opslaanKnop.IsEnabledAsync())
            {
                await opslaanKnop.ClickAsync();
            }

            await page.GetByRole(AriaRole.Table).WaitForAsync().ContinueWith(x =>
            {
                if (x.IsCompleted)
                {
                    bericht.Key = bericht.Title;
                    return;
                }
                throw new Exception("Failed to update the bericht.");
            });


        }

        public static async Task<bool> FindBerichtOnPagesAsync(this IPage page, string searchString, IEnumerable<string>? whitelistUrls = null)
        {
            var urls = await page.GetAllLinksFromNavAsync();
            var tasks = urls.Select(async url =>
            {
                var href = await url.GetAttributeAsync("href");
                if (href == null || href.StartsWith("/api") || href == "/") return false;

                return await page.GotoAsync(href).ContinueWith(async x =>
                {
                    var locator = page.GetBeheerRowByValue(searchString);
                    return await locator.IsVisibleAsync();
                }).Unwrap();
            });

            var results = await Task.WhenAll(tasks);

            return results.Any(x => x == true);
        }

       
    }


    internal class Bericht :  IAsyncDisposable
    {
        private readonly IPage _page;
        
        public Bericht(IPage page)
        {
            _page = page;
        }
        public required string Key { get; set; }
        public required string Title { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime PublicatieDatum { get; set; }
        public DateTime PublicatieEinddatum { get; set; }
        public new bool IsImportant { get; set; }
        public new string? Skill { get; set; }
        public new BerichtType BerichtType { get; set; }
        public new TimeSpan? PublishDateOffset { get; set; }

    
        public async ValueTask DisposeAsync()
        {
            await _page.Context.Tracing.GroupEndAsync();
            await _page.Context.Tracing.GroupAsync("Cleanup artikel");
            await _page.NavigateToNieuwsWerkinstructiesBeheer();
            var nieuwsRows = _page.GetByRole(AriaRole.Row)
                .Filter(new()
                { 
                    Has = _page.GetByRole(AriaRole.Cell, new() { Name = BerichtType.ToString(), Exact = true }).First
                })
                 .Filter(new()
                 {
                     Has = _page.GetByRole(AriaRole.Cell, new() { Name = Title, Exact = true }).First,
                   });

            var deleteButton = nieuwsRows.GetByTitle("Verwijder").First;
            using (var _ = _page.AcceptAllDialogs())
            {
                await deleteButton.ClickAsync();
            }
            await _page.GetByRole(AriaRole.Table).WaitForAsync();
        }
    }

    internal enum BerichtType
    {
        Nieuws,
        Werkinstructie
    }

    internal record class CreateBerichtRequest()
    {
        public required string Title { get; init; }
        public virtual string? Body { get; init; }
        public bool IsImportant { get; init; }
        public string? Skill { get; init; }
        public BerichtType BerichtType { get; init; } = BerichtType.Nieuws;

        public TimeSpan? PublishDateOffset { get; init; }

        public DateTime PublicatieDatum { get; init; }
        public DateTime PublicatieEinddatum { get; init; } = DateTime.Now.AddDays(1);
    }
}

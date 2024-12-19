using Kiss.Bff.EndToEndTest.Helpers;

namespace Kiss.Bff.EndToEndTest.NieuwsEnWerkInstructies.Helpers
{
    internal static class CreateBerichtExtension
    {
        public static async Task<IAsyncDisposable> CreateBerichten(this IPage page, IEnumerable<CreateBerichtRequest> requests)
        {
            var berichten = new List<Bericht>();
            async ValueTask Dispose()
            {
                if (berichten.Count == 0) return;
                await page.Context.Tracing.GroupEndAsync();
                await page.Context.Tracing.GroupAsync("Cleanup");
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
                    var bericht = await page.CreateBericht(item);
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

        public static async Task<Bericht> CreateBericht(this IPage page, CreateBerichtRequest request)
        {
            await page.NavigateToNieuwsWerkinstructiesBeheer();
            var toevoegenLink = page.GetByRole(AriaRole.Link, new() { Name = "Toevoegen" });
            await toevoegenLink.ClickAsync();
            await page.GetByRole(AriaRole.Radio, new() { Name = request.BerichtType.ToString() }).CheckAsync();

            await page.GetByRole(AriaRole.Textbox, new() { Name = "Titel" }).FillAsync(request.Titel);

            await page.GetByRole(AriaRole.Textbox, new() { Name = "Rich Text Editor" }).FillAsync(!string.IsNullOrWhiteSpace(request.Inhoud) ? request.Inhoud : request.Titel);

            if (request.IsBelangrijk)
            {
                await page.GetByRole(AriaRole.Checkbox, new() { Name = "Belangrijk" }).CheckAsync();
            }

            if (!string.IsNullOrEmpty(request.Skill))
            {
                var skillCheckbox = page.GetByRole(AriaRole.Checkbox, new() { Name = request.Skill });
                await skillCheckbox.CheckAsync(); // Ensure the skill checkbox is checked
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

            var opslaanKnop = page.GetByRole(AriaRole.Button, new() { Name = "Opslaan" });
            while (await opslaanKnop.IsVisibleAsync() && await opslaanKnop.IsEnabledAsync())
            {
                await opslaanKnop.ClickAsync();
            }

            await page.GetByRole(AriaRole.Table).WaitForAsync();
            return new(page)
            {
                IsBelangrijk = request.IsBelangrijk,
                Titel = request.Titel,
                PublishDateOffset = request.PublishDateOffset,
                Skill = request.Skill,
                Inhoud = request.Inhoud,
                BerichtType = request.BerichtType,
            };
        }
    }

    internal record class Bericht(IPage Page) : CreateBerichtRequest, IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await Page.NavigateToNieuwsWerkinstructiesBeheer();
            var nieuwsRows = Page.GetByRole(AriaRole.Row)
                .Filter(new()
                {
                    Has = Page.GetByRole(AriaRole.Cell, new() { Name = BerichtType.ToString() }).First
                })
                .Filter(new()
                {
                    Has = Page.GetByRole(AriaRole.Cell, new() { Name = Titel, Exact = true }).First
                });

            var deleteButton = nieuwsRows.GetByTitle("Verwijder").First;
            using (var _ = Page.AcceptAllDialogs())
            {
                await deleteButton.ClickAsync();
            }
            await Page.GetByRole(AriaRole.Table).WaitForAsync();
        }
    }

    internal enum BerichtType
    {
        Nieuws,
        Werkinstructie
    }

    internal record class CreateBerichtRequest()
    {
        public required string Titel { get; init; }
        public string? Inhoud { get; init; }
        public bool IsBelangrijk { get; init; }
        public string? Skill { get; init; }
        public BerichtType BerichtType { get; init; } = BerichtType.Nieuws;

        public TimeSpan? PublishDateOffset { get; init; }
    }
}

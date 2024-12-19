using System.Collections.Concurrent;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;


namespace Kiss.Bff.EndToEndTest
{
    [TestClass]
    public class BaseTestInitializer : PageTest
    {
        private const string StoragePath = "./auth.json";

        private static readonly IConfiguration s_configuration = new ConfigurationBuilder()
            .AddUserSecrets<BaseTestInitializer>()
            .AddEnvironmentVariables()
            .Build();

        private static readonly UniqueOtpHelper s_uniqueOtpHelper = new(GetRequiredConfig("TestSettings:TEST_TOTP_SECRET"));
        private static readonly ConcurrentDictionary<string, string> s_testsHtml = [];


        private readonly List<string> _steps = [];

        [TestInitialize]
        public virtual async Task TestInitialize()
        {
            var username = GetRequiredConfig("TestSettings:TEST_USERNAME");
            var password = GetRequiredConfig("TestSettings:TEST_PASSWORD");

            var loginHelper = new AzureAdLoginHelper(Page, username, password, s_uniqueOtpHelper);
            await loginHelper.LoginAsync();
            await Context.StorageStateAsync(new() { Path = StoragePath });

            await Context.Tracing.StartAsync(new()
            {
                Title = $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}",
                Screenshots = true,
                Snapshots = true,
                Sources = true,
            });

            Console.WriteLine(TestContext.TestName);
        }

        protected async Task Step(string description)
        {
            await Context.Tracing.GroupEndAsync();
            await Context.Tracing.GroupAsync(description);
            Console.WriteLine(description);
            _steps.Add(description);
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await Context.Tracing.GroupEndAsync();
            var fileName = $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}.zip";
            var fullPath = Path.Combine(Environment.CurrentDirectory, "playwright-traces", fileName);

            await Context.Tracing.StopAsync(new()
            {
                Path = fullPath
            });

            var html = $"""
            <div data-outcome="{TestContext.CurrentTestOutcome}">
                <h2>{TestContext.TestName}</h2>
                <a target="_blank" href="https://trace.playwright.dev/?trace=https://klantinteractie-servicesysteem.github.io/KISS-frontend/{fileName}">Playwright tracing</a>
                <p>Steps:</p>
                <ol>{string.Join("", _steps.Select(step => $"""
                    <li>{step}</li>
                """))}
                </ol>
            </div>
            """;

            s_testsHtml.TryAdd(TestContext.TestName!, html);
        }

        [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
        public static async Task ClassCleanup()
        {
            var html = $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src https://unpkg.com/simpledotcss@2.3.3/simple.min.css 'sha256-kZS4Ytg58npMuRkQ/J+zM+WLW6n1befhcf5YgPVHdEE=';">
                <title>Playwright traces</title>
                <link rel="stylesheet" href="https://unpkg.com/simpledotcss@2.3.3/simple.min.css" integrity="sha384-Cxvt41nwdtHMOjpCqr+FaCybNL58LeIc0vPSLR4KlFSCBrHTb095iJbbw+hDTklQ" crossorigin="anonymous">
                <style>[data-outcome=Failed]{color: var(--code)}</style>
            </head>
            <body>
                <main>
                    {{string.Join("", s_testsHtml.OrderBy(x=> x.Key).Select(x=> x.Value))}}
                </main>
            </body>
            """;
            
            using var writer = File.CreateText(Path.Combine(Environment.CurrentDirectory, "playwright-traces", "index.html"));
            await writer.WriteLineAsync(html);
        }

        private static string GetRequiredConfig(string key)
        {
            var value = s_configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"'{key}' is missing from the configuration");
            }
            return value;
        }

        public override BrowserNewContextOptions ContextOptions()
        {
            return new(base.ContextOptions())
            {
                BaseURL = GetRequiredConfig("TestSettings:TEST_BASE_URL"),
                // save auth state so we don't need to log in in every single test
                StorageStatePath = File.Exists(StoragePath) ? StoragePath : null,
            };
        }
    }
}

using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using Kiss.Bff.EndToEndTest.Infrastructure;
using Kiss.Bff.EndToEndTest.Infrastructure.ApiClients;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Kiss.Bff.EndToEndTest
{
    /// <summary>
    /// Inherit this class in each test class. This does the following:<br/>
    /// 1. Makes sure the user is logged in before each test starts<br/>
    /// 2. Makes sure Playwright records traces for each test<br/>
    /// 3. Exposes a <see cref="Step(string)"/> method to define test steps. These show up in the Playwright traces and in the test report.<br/>
    /// 4. Builds a html test report after all tests in a test class are done.
    /// We upload these to <a href="https://klantinteractie-servicesysteem.github.io/KISS-frontend/">github pages</a>
    /// </summary>
    [TestClass]
    public class KissPlaywrightTest : PageTest
    {
        private const string StoragePath = "./auth.json";

        public readonly OpenKlantApiClient OpenKlantApiClient = new(
            GetRequiredConfig("TestSettings:TEST_OPEN_KLANT_BASE_URL"),
            GetRequiredConfig("TestSettings:TEST_OPEN_KLANT_SECRET")
            );

        public TestCleanupHelper TestCleanupHelper { get; }

        private static readonly IConfiguration s_configuration = new ConfigurationBuilder()
            .AddUserSecrets<KissPlaywrightTest>()
            .AddEnvironmentVariables()
            .Build();

        private static readonly UniqueOtpHelper s_uniqueOtpHelper = new(GetRequiredConfig("TestSettings:TEST_TOTP_SECRET"));

        // this is used to build a test report for each test
        private static readonly ConcurrentDictionary<string, string> s_testReports = [];

        // Counter for DataRow variations of the same test method
        private static readonly ConcurrentDictionary<string, int> s_dataRowCounters = [];

        private readonly List<string> _steps = [];

        // clean up actions that are registered by the tests
        private readonly List<Func<Task>> _cleanupActions = [];

        public KissPlaywrightTest()
        {
            TestCleanupHelper = new TestCleanupHelper(OpenKlantApiClient);
        }

        /// <summary>
        /// This is run before each test
        /// </summary>
        /// <returns></returns>
        [TestInitialize]
        public virtual async Task TestInitialize()
        {
            // Set global timeouts to 50 seconds
            Page.SetDefaultTimeout(50000); // 50 seconds for all actions
            Page.SetDefaultNavigationTimeout(50000); // 50 seconds for navigation

            // log in with azure ad
            var username = GetRequiredConfig("TestSettings:TEST_USERNAME");
            var password = GetRequiredConfig("TestSettings:TEST_PASSWORD");

            var loginHelper = new AzureAdLoginHelper(Page, username, password, s_uniqueOtpHelper);
            await loginHelper.LoginAsync();
            // store the cookie so we stay logged in in each test
            await Context.StorageStateAsync(new() { Path = StoragePath });

            // start tracing. we do this AFTER logging in so the password doesn't end up in the tracing
            await Context.Tracing.StartAsync(new()
            {
                Title = $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}",
                Screenshots = true,
                Snapshots = true,
                Sources = true,
            });
        }

        /// <summary>
        /// Start a test step. This ends up in the test report and as group in the playwright tracing
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        protected async Task Step(string description)
        {
            await Context.Tracing.GroupEndAsync();
            await Context.Tracing.GroupAsync(description);
            _steps.Add(description);
        }

        /// <summary>
        /// This is run after each test
        /// </summary>
        /// <returns></returns>
        [TestCleanup]
        public async Task TestCleanup()
        {
            try
            {
                // if we are in a group, end it
                await Context.Tracing.GroupEndAsync();
            }
            catch (Exception)
            {
                // Silently handle tracing group end errors
            }

            var fileName = $"{TestContext.FullyQualifiedTestClassName}.{TestContext.TestName}.zip";
            var fullPath = Path.Combine(Environment.CurrentDirectory, "playwright-traces", fileName);

            Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "playwright-traces"));

            try
            {
                // stop tracing and save a zip file in the output directory
                await Context.Tracing.StopAsync(new()
                {
                    Path = fullPath
                });
            }
            catch (PlaywrightException ex) when (ex.Message.Contains("Must start tracing"))
            {
                fileName = ""; // No trace file available
            }
            catch (Exception)
            {
                fileName = ""; // No trace file available
            }

            // Get the descriptive test name from the TestMethod attribute
            var descriptiveTestName = GetDescriptiveTestName();

            // Extract the actual class name from the fully qualified class name
            var actualClassName = TestContext.FullyQualifiedTestClassName?.Split('.').LastOrDefault() ?? "Unknown";

            // Enhanced HTML report with descriptive test name and error handling
            var traceSection = !string.IsNullOrEmpty(fileName)
                ? $"""<p><strong>Trace:</strong> <a target="_blank" href="https://trace.playwright.dev/?trace=https://klantinteractie-servicesysteem.github.io/KISS-frontend/{fileName}">View Playwright Trace</a></p>"""
                : $"""<p><strong>Trace:</strong> <em>Not available (test failed during initialization)</em></p>""";

            // For failed tests, we'll show basic failure info
            var errorSection = TestContext.CurrentTestOutcome == UnitTestOutcome.Failed
                ? $"""
        <div class="error-details">
            <p><strong>Status:</strong> Test Failed</p>
            <p><strong>Note:</strong> Check console output for detailed error information</p>
        </div>
        """ : "";

            var html = $"""
    <div data-outcome="{TestContext.CurrentTestOutcome}" data-test-name="{HtmlEncoder.Default.Encode(descriptiveTestName)}" data-class-name="{HtmlEncoder.Default.Encode(actualClassName)}">
        <div class="test-header">
            <h4>{HtmlEncoder.Default.Encode(descriptiveTestName)}</h4>
        </div>
        <div class="test-details">
            {traceSection}
        </div>
        {errorSection}
        <details class="test-steps">
            <summary>Test Steps ({_steps.Count})</summary>
            <ol>{string.Join("", _steps.Select(step => $"""
                <li>{HtmlEncoder.Default.Encode(step)}</li>
            """))}
            </ol>
        </details>
    </div>
    """;

            // Always add the test report, even for failed tests
            // For DataRow tests, MSTest appends parameters to TestName automatically
            // Use a counter as fallback if the same key already exists
            var baseKey = TestContext.TestName!;
            var uniqueKey = baseKey;
            var counter = 1;

            while (!s_testReports.TryAdd(uniqueKey, html))
            {
                uniqueKey = $"{baseKey}_{counter}";
                counter++;
            }

            // Run cleanup actions in reverse order
            foreach (var cleanup in ((IEnumerable<Func<Task>>)_cleanupActions).Reverse())
            {
                try
                {
                    await cleanup();
                }
                catch (Exception)
                {
                    // Silently handle cleanup failures
                }
            }
        }

        private static string ExtractClassName(string html)
        {
            // Try to get the class name from the data-class-name attribute we store
            var classNameMatch = System.Text.RegularExpressions.Regex.Match(html, @"data-class-name=""([^""]+)""");
            if (classNameMatch.Success && !string.IsNullOrEmpty(classNameMatch.Groups[1].Value))
            {
                return classNameMatch.Groups[1].Value;
            }

            // If no class name found, return "Unknown Class"
            return "Unknown Class";
        }

        private string GetDescriptiveTestName()
        {
            try
            {
                var testMethod = GetType().GetMethod(TestContext.TestName!);
                var testMethodAttribute = testMethod?.GetCustomAttribute<TestMethodAttribute>();

                if (testMethodAttribute != null && !string.IsNullOrEmpty(testMethodAttribute.DisplayName))
                {
                    var baseDisplayName = testMethodAttribute.DisplayName;

                    if (testMethod?.GetCustomAttributes<DataRowAttribute>().Any() == true)
                    {
                        var methodName = testMethod.Name;
                        var currentCount = s_dataRowCounters.AddOrUpdate(methodName, 1, (key, value) => value + 1);

                        return $"{baseDisplayName} - Scenario {currentCount}";
                    }

                    return baseDisplayName;
                }

                return TestContext.TestName ?? "Unknown Test";
            }
            catch (Exception)
            {
                return TestContext.TestName ?? "Unknown Test";
            }
        }

        /// <summary>
        /// This is run after all tests in a test class are done
        /// </summary>
        /// <returns></returns>
        [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
        public static async Task ClassCleanup()
        {
            if (s_testReports.Count == 0)
            {
                return;
            }

            // Create directory first
            var tracesDir = Path.Combine(Environment.CurrentDirectory, "playwright-traces");
            Directory.CreateDirectory(tracesDir);

            // Group tests by class name first, then by outcome
            var testsByClass = s_testReports
                .GroupBy(kvp => ExtractClassName(kvp.Value))
                .OrderBy(group => group.Key)
                .ToList();

            var totalTests = s_testReports.Count;
            var passedTests = s_testReports.Count(kvp => ExtractOutcome(kvp.Value) == "Passed");
            var failedTests = s_testReports.Count(kvp => ExtractOutcome(kvp.Value) == "Failed");

            var html = $$"""
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="Content-Security-Policy" content="default-src 'none'; style-src https://unpkg.com/simpledotcss@2.3.3/simple.min.css 'sha256-l0D//z1BZPnhAdIJ0lA8dsfuil0AB4xBpnOa/BhNVoU=' 'unsafe-inline';">
    <title>KISS E2E Test Report</title>
    <link rel="stylesheet" href="https://unpkg.com/simpledotcss@2.3.3/simple.min.css">
    <style>
        [data-outcome=Failed] { border-left: 4px solid #dc3545; }
        [data-outcome=Passed] { border-left: 4px solid #28a745; }
        
        .test-group { margin-bottom: 2rem; }
        .class-group { margin-bottom: 3rem; border: 2px solid var(--border); border-radius: 10px; overflow: hidden; }
        .class-header { 
            background: linear-gradient(135deg, var(--accent), var(--accent-bg)); 
            color: var(--accent-text);
            padding: 1.5rem; 
            margin: 0;
            border-bottom: 2px solid var(--border);
        }
        .class-header h2 { margin: 0; color: inherit; }
        .class-content { padding: 1.5rem; }
        
        .outcome-group { margin-bottom: 2rem; }
        .group-header { 
            background: var(--accent-bg); 
            padding: 1rem; 
            border-radius: 8px; 
            margin-bottom: 1rem;
        }
        
        .test-item { 
            background: var(--bg); 
            border: 1px solid var(--border); 
            border-radius: 6px; 
            padding: 1rem; 
            margin-bottom: 0.5rem; 
        }
        
        .test-header { 
            display: flex; 
            justify-content: space-between; 
            align-items: center; 
            margin-bottom: 0.5rem; 
        }
        .test-header h3 { margin: 0; }
        .test-header h4 { margin: 0; font-size: 1.1rem; }
        
        .outcome-badge { 
            padding: 0.25rem 0.5rem; 
            border-radius: 4px; 
            font-size: 0.8rem; 
            font-weight: bold; 
        }
        .outcome-passed { background: #d4edda; color: #155724; }
        .outcome-failed { background: #f8d7da; color: #721c24; }
        
        .test-details code {
            background: var(--accent-bg);
            padding: 0.2rem 0.4rem;
            border-radius: 3px;
            font-size: 0.85rem;
        }
        
        .summary { 
            background: var(--accent-bg); 
            padding: 1.5rem; 
            border-radius: 8px; 
            margin-bottom: 2rem; 
            text-align: center;
        }
        
        /* Enhanced styles for failed tests */
        .error-details {
            background: #f8f9fa;
            border: 1px solid #e9ecef;
            border-radius: 4px;
            padding: 1rem;
            margin: 1rem 0;
        }
        
        .error-message {
            background: #f8d7da;
            border: 1px solid #f5c6cb;
            border-radius: 4px;
            padding: 0.75rem;
            margin: 0.5rem 0 0 0;
            font-family: 'Courier New', monospace;
            font-size: 0.85rem;
            color: #721c24;
            white-space: pre-wrap;
            word-break: break-word;
            max-height: 200px;
            overflow-y: auto;
        }
        
        .test-item[data-outcome="Failed"] {
            background: #fff5f5;
        }
        
        .test-item[data-outcome="Passed"] {
            background: #f0fff4;
        }
        
        .class-stats {
            display: flex;
            gap: 1rem;
            margin-top: 0.5rem;
            font-size: 0.9rem;
        }
        .stat { padding: 0.25rem 0.5rem; border-radius: 4px; }
        .stat-passed { background: #d4edda; color: #155724; }
        .stat-failed { background: #f8d7da; color: #721c24; }
        .stat-total { background: var(--accent-bg); }
    </style>
</head>
<body>
    <header>
        <h1>🧪 KISS End-to-End Test Report</h1>
        <p>Generated on {{DateTime.Now:yyyy-MM-dd HH:mm:ss}}</p>
    </header>
    
    <main>
        <section class="summary">
            <h2>📊 Test Summary</h2>
            <p><strong>Total:</strong> {{totalTests}} | <strong style="color: #28a745;">Passed:</strong> {{passedTests}} | <strong style="color: #dc3545;">Failed:</strong> {{failedTests}}</p>
            <p><strong>Test Classes:</strong> {{testsByClass.Count}}</p>
        </section>

        {{string.Join("", testsByClass.Select(classGroup => GenerateClassSection(classGroup.Key, classGroup)))}}
    </main>
</body>
</html>
""";

            var htmlFilePath = Path.Combine(tracesDir, "index.html");
            using var writer = File.CreateText(htmlFilePath);
            await writer.WriteLineAsync(html);
        }

        private static string GenerateClassSection(string className, IGrouping<string, KeyValuePair<string, string>> classTests)
        {
            var friendlyClassName = GetFriendlyClassName(className);
            var totalInClass = classTests.Count();
            var passedInClass = classTests.Count(test => ExtractOutcome(test.Value) == "Passed");
            var failedInClass = classTests.Count(test => ExtractOutcome(test.Value) == "Failed");

            var groupedByOutcome = classTests
                .GroupBy(test => ExtractOutcome(test.Value))
                .OrderBy(group => GetOutcomeOrder(group.Key))
                .ToList();

            var outcomeGroupsHtml = string.Join("", groupedByOutcome.Select(outcomeGroup => GenerateOutcomeGroupSection(outcomeGroup.Key, outcomeGroup)));

            return $"""
    <section class="class-group">
        <div class="class-header">
            <h2>📋 {HtmlEncoder.Default.Encode(friendlyClassName)}</h2>
            <div class="class-stats">
                <span class="stat stat-total">Total: {totalInClass}</span>
                <span class="stat stat-passed">Passed: {passedInClass}</span>
                <span class="stat stat-failed">Failed: {failedInClass}</span>
            </div>
        </div>
        <div class="class-content">
            {outcomeGroupsHtml}
        </div>
    </section>
    """;
        }

        private static string GenerateOutcomeGroupSection(string outcome, IGrouping<string, KeyValuePair<string, string>> tests)
        {
            var icon = outcome switch
            {
                "Passed" => "✅",
                "Failed" => "❌",
                _ => "🔍"
            };

            var testItems = string.Join("", tests.Select(test => $"""
        <div class="test-item" data-outcome="{outcome}">
            {test.Value}
        </div>
    """));

            return $"""
    <div class="outcome-group">
        <div class="group-header">
            <h3>{icon} {outcome} Tests ({tests.Count()})</h3>
        </div>
        <div class="outcome-content">
            {testItems}
        </div>
    </div>
    """;
        }

        private static string GetFriendlyClassName(string className)
        {
            // Generic approach - works with any class name
            return className switch
            {
                "AfhandelingFormScenarios" => "Afhandeling Form Tests",
                "AnonymousContactmomentScenarios" => "Anonymous Contactmoment Tests",
                "Unknown Class" => "Miscellaneous Tests",
                _ => className.Replace("Scenarios", " Tests")
                             .Replace("Test", " Tests")
                             .Replace("  ", " ")
                             .Trim()
            };
        }

        private static int GetOutcomeOrder(string outcome)
        {
            return outcome switch
            {
                "Passed" => 1,
                "Failed" => 2,
                _ => 3
            };
        }

        private static string ExtractOutcome(string html)
        {
            var match = System.Text.RegularExpressions.Regex.Match(html, @"data-outcome=""([^""]+)""");
            return match.Success ? match.Groups[1].Value : "Unknown";
        }

        protected async Task CaptureScreenshotAsync(string testName)
        {
            var screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
            Directory.CreateDirectory(screenshotDir);

            var path = Path.Combine(screenshotDir, $"{testName}.png");
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
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

        protected void RegisterCleanup(Func<Task> cleanupFunc)
        {
            _cleanupActions.Add(cleanupFunc);
        }
    }
}
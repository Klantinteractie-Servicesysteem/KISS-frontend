using System.Collections.Concurrent;

namespace Kiss.Bff.EndToEndTest.Infrastructure
{
    internal static class HtmlReport
    {
        private static readonly ConcurrentDictionary<string, (string, ConcurrentDictionary<string, string>)> s_reports = [];

        public static bool TryAdd(string className, string testName, string html)
        {
            var (_, testClass) = s_reports.GetOrAdd(className, (_) => (className.Split('.').ElementAt(^1),[]));
            return testClass.TryAdd(testName, html);
        }

        public static IEnumerable<KeyValuePair<string, string>> GetClassNames() => s_reports.Select(r => new KeyValuePair<string, string>(r.Key, r.Value.Item1));

        public static IEnumerable<KeyValuePair<string, string>> GetByClassName(string className) => s_reports.TryGetValue(className, out var tests)
            ? tests.Item2
            : [];
    }
}

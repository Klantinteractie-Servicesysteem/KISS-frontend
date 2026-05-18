namespace Kiss.Elastic.Sync.IntegrationTest
{
    public static class Extensions
    {
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> values)
            => new FakeAsyncEnumerable<T>(values);

        private class FakeAsyncEnumerable<T>(IEnumerable<T> enumerable) : IAsyncEnumerable<T>
        {
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new FakeAsyncEnumerator<T>(enumerable.GetEnumerator());
        }

        private class FakeAsyncEnumerator<T>(IEnumerator<T> enumerator) : IAsyncEnumerator<T>
        {
            public T Current => enumerator.Current;

            public ValueTask DisposeAsync()
            {
                enumerator.Dispose();
                return new();
            }

            public ValueTask<bool> MoveNextAsync() => new(enumerator.MoveNext());
        }
    }
}

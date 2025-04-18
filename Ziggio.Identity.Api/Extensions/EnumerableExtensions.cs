namespace Ziggio.Identity.Api.Extensions {
    public static class EnumerableExtensions {
        public static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> values) {
            if (values == null) {
                throw new ArgumentNullException(nameof(values));
            }

            return ExecuteAsync();

            async Task<List<T>> ExecuteAsync() {
                var list = new List<T>();

                await foreach (var value in values) {
                    list.Add(value);
                }

                return list;
            }
        }
    }
}

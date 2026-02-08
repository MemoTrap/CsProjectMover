namespace ProjectMover.Global.Lib {
  public static class DictionaryExtensions
  {
    public static TValue GetOrAdd<TKey, TValue>(
      this Dictionary<TKey, TValue> dict,
      TKey key,
      Func<TKey, TValue> create,
      Func<TKey, TValue, bool>? recreate = null
    ) where TKey : notnull {
      if (dict.TryGetValue(key, out var value) &&
          (recreate is null || !recreate(key, value!)))
        return value!;

      value = create(key);
      dict[key] = value;
      return value!;
    }

    public static async Task<TValue> GetOrAddAsync<TKey, TValue>(
      this Dictionary<TKey, TValue> dict,
      TKey key,
      Func<TKey, Task<TValue>> createAsync,
      Func<TKey, TValue, bool>? recreate = null
    ) where TKey : notnull {
      if (dict.TryGetValue(key, out var value) &&
          (recreate is null || !recreate(key, value!)))
        return value!;

      value = await createAsync(key).ConfigureAwait(false);
      dict[key] = value;
      return value!;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
      where TKey : notnull
      => dict.TryGetValue(key, out var value) ? value! : default!;
  }
}

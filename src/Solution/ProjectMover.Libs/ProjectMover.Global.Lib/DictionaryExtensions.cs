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

    public static TValue GetOrAdd<TKey, TValue>(
      this Dictionary<TKey, TValue> dict,
      TKey key,
      TValue value = default
    ) where TKey : notnull
      where TValue : struct {
      dict[key] = value;
      return value!;
    }

    /// <summary>
    /// Updates the dictionary entry for the specified key with the given value, 
    /// or adds the key with its default value if the value is default.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The value type of the dictionary, which must be a struct.</typeparam>
    /// <param name="dict">The dictionary to update.</param>
    /// <param name="key">The key whose value should be updated or added.</param>
    /// <param name="value">The value to set for the specified key, or default to add the key with its default value.</param>
    public static void AddOrUpdate<TKey, TValue>(
      this Dictionary<TKey, TValue> dict,
      TKey key,
      TValue value = default
    ) where TKey : notnull
      where TValue : struct {
      if (value.IsDefault ())
        dict.GetOrAdd (key);
      else 
        dict[key] = value;
    }

    public static bool IsDefault<T> (this T value) where T : struct {
      return EqualityComparer<T>.Default.Equals (value, default);
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

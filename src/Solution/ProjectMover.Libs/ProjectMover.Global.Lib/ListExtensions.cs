namespace ProjectMover.Global.Lib {

  public record SubsequenceDifference<T> (
    int StartIndex,
    IReadOnlyList<T> FirstSegments,
    IReadOnlyList<T> SecondSegments
  ) {
    public bool HasDifference =>
        FirstSegments.Count != 0 || SecondSegments.Count != 0;
  }


  public static class ListExtensions {

    public static SubsequenceDifference<T>
      FindFirstSubsequenceDifference<T> (
        this IReadOnlyList<T> first,
        IReadOnlyList<T> second,
        int skipLast = 0,
        IEqualityComparer<T>? comparer = null
    ) {

      comparer ??= EqualityComparer<T>.Default;
      int firstCount = first.getEffectiveCount (skipLast);
      int secondCount = second.getEffectiveCount (skipLast);

      int minLength = Math.Min (firstCount, secondCount);

      int start = 0;

      // Find first differing index
      while (start < minLength &&
             comparer.Equals (first[start], second[start])) {
        start++;
      }

      // No difference and same length
      if (start == firstCount && start == secondCount)
        return new SubsequenceDifference<T> (
          0,
          [],
          []
        );
      

      // If one ended but the other continues
      if (start == minLength) {
        return new (
            start,
            first.Skip (start).clone (first),
            second.Skip (start).clone (second)
        );
      }

      // Walk forward until sequences realign
      for (int i = start; i <= firstCount; i++) {
        for (int j = start; j <= secondCount; j++) {
          int remainingFirst = firstCount - i;
          int remainingSecond = secondCount - j;

          if (remainingFirst != remainingSecond)
            continue;

          bool suffixMatches = true;

          for (int k = 0; k < remainingFirst; k++) {
            if (!comparer.Equals (first[i + k], second[j + k])) {
              suffixMatches = false;
              break;
            }
          }

          if (suffixMatches) {
            return new SubsequenceDifference<T> (
                start,
                first.Skip (start).Take (i - start).clone (first),
                second.Skip (start).Take (j - start).clone (second));
          }
        }
      }

      // fallback (should not normally happen)
      return new SubsequenceDifference<T> (
          start,
          first.Skip (start).clone (first),
          second.Skip (start).clone (second));

    }

    public static IReadOnlyList<T> ReplaceSubsequence<T> (
      this IReadOnlyList<T> source,
      SubsequenceDifference<T> diff,
      int skipLast = 0,
      IEqualityComparer<T>? comparer = null
    ) => source.ReplaceSubsequence (diff.StartIndex, diff.FirstSegments, diff.SecondSegments, skipLast, comparer);

    public static IReadOnlyList<T> ReplaceSubsequence<T> (
      this IReadOnlyList<T> source,
      int startIndex,
      IReadOnlyList<T> oldSegments,
      IReadOnlyList<T> newSegments,
      int skipLast = 0,
      IEqualityComparer<T>? comparer = null
    ) {

      comparer ??= EqualityComparer<T>.Default;
      int sourceCount = getEffectiveCount (source, skipLast);

      // Index-based only if oldSegments is empty
      if (!oldSegments.Any ()) {
                
        if (startIndex < 0 || startIndex > sourceCount)
          throw new ArgumentOutOfRangeException (nameof (startIndex));

        var result = new List<T> (
            sourceCount - oldSegments.Count + newSegments.Count);

        // before
        for (int i = 0; i < startIndex; i++)
          result.Add (source[i]);

        // insert replacement
        foreach (var item in newSegments)
          result.Add (item);

        // after (skip removed segment)
        for (int i = startIndex + oldSegments.Count; i < source.Count; i++)
          result.Add (source[i]);

        return result.clone (source);
      
      } else {

        for (int i = 0; i <= sourceCount - oldSegments.Count; i++) {
          bool match = true;

          for (int j = 0; j < oldSegments.Count; j++) {
            if (!comparer.Equals (source[i + j], oldSegments[j])) {
              match = false;
              break;
            }
          }

          if (match) {
            var result = new List<T> (
                sourceCount - oldSegments.Count + newSegments.Count);

            // before
            for (int k = 0; k < i; k++)
              result.Add (source[k]);

            // replacement
            foreach (var item in newSegments)
              result.Add (item);

            // after
            for (int k = i + oldSegments.Count; k < source.Count; k++)
              result.Add (source[k]);

            return result;
          }
        }

        return source.clone (); // unchanged, but copied
      }
    }

    public static IReadOnlyList<T> CommonStub<T> (
      this IReadOnlyList<T> first,
      IReadOnlyList<T> second,
      IEqualityComparer<T>? comparer = null
    ) {

      comparer ??= EqualityComparer<T>.Default;

      int minLength = Math.Min (first.Count, second.Count);

      int i = 0;

      while (i < minLength &&
             comparer.Equals (first[i], second[i])) {
        i++;
      }

      if (i == 0)
        return [];

      return first.Take (i).clone (first);
    }

    private static IReadOnlyList<T> clone<T> (this IEnumerable<T> result, IEnumerable<T>? source = null) {
      Type type = (source ?? result).GetType();
      return type.IsArray ? result.ToArray () : result.ToList ();
    }

    private static int getEffectiveCount<T> (this IReadOnlyList<T> source, int skipLast) {
      ArgumentOutOfRangeException.ThrowIfNegative (skipLast);

      int effective = source.Count - skipLast;
      return effective > 0 ? effective : 0;
    }
  }
}

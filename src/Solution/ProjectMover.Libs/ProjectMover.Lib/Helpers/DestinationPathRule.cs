namespace ProjectMover.Lib.Helpers {

  /// <summary>
  /// Represents a rule for mapping a source project path to a designated destination path, 
  /// supporting adaptation based on user alterations.
  /// </summary>
  internal class DestinationPathRule {
    private readonly IReadOnlyList<string> _presetRootSegments;
    private string[]? _sourceSegments;

    // Indirect substitution, not obvious to user:
    // We compare the user path with the original source path,
    // not the designated destination path.
    // This keeps it simple!
    SubsequenceDifference<string>? _differenceUserOverSource;

    public DestinationPathRule (string presetDestinationRoot) {
      if (string.IsNullOrWhiteSpace (presetDestinationRoot))
        throw new ArgumentException (null, nameof (presetDestinationRoot));

      _presetRootSegments = presetDestinationRoot.SplitPath();
    }

    // ------------------------------------------------------------
    // APPLY
    // ------------------------------------------------------------
    public string Apply (string projectSourceFullPath) {
      _sourceSegments = projectSourceFullPath.SplitPath ();

      IReadOnlyList<string> destSegments;
      if (_differenceUserOverSource is null) {
        // initial state

        // common ground
        IReadOnlyList<string> commonStub = _presetRootSegments
          .CommonStub (_sourceSegments);

        // trailing extra segments in source
        var destExtraSegments = _sourceSegments
          .Skip (commonStub.Count)
          .Take (_sourceSegments.Length - commonStub.Count)
          .ToList ();

        // initially only: Common from preset + trailng extras from source
        List<string> destSegs = [];
        destSegs.AddRange (_presetRootSegments);
        destSegs.AddRange (destExtraSegments);
        destSegments = destSegs;

      } else {
        // apply replacement to source segments
        destSegments = _sourceSegments.ReplaceSubsequence (
          _differenceUserOverSource,
          1,
          StringComparer.OrdinalIgnoreCase);
      }

      var designatedTargetFullPath = destSegments.JoinPath ();
      return designatedTargetFullPath;
    }

    // ------------------------------------------------------------
    // ADAPT
    // ------------------------------------------------------------
    public void Adapt (string? userTargetFullPath) {
      if (_sourceSegments == null)
        return; // cannot adapt yet

      if (userTargetFullPath == null)
        return; // accepted → nothing to learn

      // compare user path with source segments
      _differenceUserOverSource = _sourceSegments
        .FindFirstSubsequenceDifference (
          userTargetFullPath.SplitPath(), 
          1, 
          StringComparer.OrdinalIgnoreCase);
    }

  }
}

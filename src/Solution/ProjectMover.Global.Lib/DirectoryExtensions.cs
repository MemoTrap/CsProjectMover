namespace ProjectMover.Global.Lib {

  public static class DirectoryExtensions {
    public static IEnumerable<FileInfo> EnumerateFiles (
        this DirectoryInfo directory,
        IEnumerable<string> searchPatterns,
        SearchOption searchOption = SearchOption.TopDirectoryOnly
    ) {
      foreach (var pattern in searchPatterns) {
        foreach (var file in directory.EnumerateFiles (pattern, searchOption)) {
          yield return file;
        }
      }
    }

    public static IEnumerable<string> EnumerateFiles (
          this string directoryPath,
          IEnumerable<string> searchPatterns,
          SearchOption searchOption = SearchOption.TopDirectoryOnly) {
      ArgumentNullException.ThrowIfNull (directoryPath);
      ArgumentNullException.ThrowIfNull (searchPatterns);

      foreach (var pattern in searchPatterns) {
        foreach (var file in Directory.EnumerateFiles (
                     directoryPath,
                     pattern,
                     searchOption)) {
          yield return file;
        }
      }
    }
  }
}

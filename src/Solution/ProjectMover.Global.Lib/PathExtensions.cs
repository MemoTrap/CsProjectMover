namespace ProjectMover.Global.Lib {
  public static class PathExtensions {
    public static string ToAbsolutePath (this string relativePath, string baseDir) {
      if (Path.IsPathRooted (relativePath))
        return Path.GetFullPath (relativePath);
      return Path.GetFullPath (Path.Combine (baseDir, relativePath));
    }

    public static string ToRelativePath (this string absolutePath, string baseDir) {
      if (!Path.IsPathRooted (absolutePath))
        absolutePath = absolutePath.ToAbsolutePath (baseDir);
      var absPath = Path.GetFullPath (absolutePath);
      var absBase = Path.GetFullPath (baseDir);
      var relativePath = Path.GetRelativePath (absBase, absPath);
      return relativePath;
    }

    public static string ToNewDestinationPath (
        this string originalPath,
        string oldBasePath,
        string newBasePath) {
      var absOriginal = Path.GetFullPath (originalPath);
      var absOldBase = Path.GetFullPath (oldBasePath);
      var absNewBase = Path.GetFullPath (newBasePath);
      if (!absOriginal.IsSubPathOf (absOldBase))
        throw new ArgumentException (
            "Original path is not a subpath of the old base path.",
            nameof (originalPath));
      var relativePart = Path.GetRelativePath (absOldBase, absOriginal);
      var newAbsolutePath = Path.Combine (absNewBase, relativePart);
      return newAbsolutePath;
    }

    public static bool IsExistingSubPathOf (this string potentialSubPath, string basePath) {

      if (Directory.Exists (potentialSubPath))
        return potentialSubPath.IsSubPathOf (basePath);

      if (File.Exists (potentialSubPath)) {
        var dirOfFile = Path.GetDirectoryName (potentialSubPath);
        return dirOfFile?.IsSubPathOf (basePath) ?? false;
      }

      return false;
    }


    public static bool IsSubPathOf (this string potentialSubPath, string basePath) {
      var normalizedSubpath = Path.GetFullPath (potentialSubPath)
        .TrimEnd (Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        + Path.DirectorySeparatorChar;
      var normalizedBasePath = Path.GetFullPath (basePath)
        .TrimEnd (Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
        + Path.DirectorySeparatorChar;
      return normalizedSubpath.StartsWith (normalizedBasePath, StringComparison.OrdinalIgnoreCase);
    }

    public static string GetLastDirectoryName (this string path) {
      var normalizedPath = path.TrimEnd (Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
      return Path.GetFileName (normalizedPath);
    }


    /// <summary>
    /// Replaces the file extension of <paramref name="path"/> with <paramref name="newExt"/>.
    /// </summary>
    /// <param name="path">Original file path.</param>
    /// <param name="newExt">New extension (with or without leading dot).</param>
    /// <returns>Path with the new extension.</returns>
    public static string ReplaceFileExtensionWith (this string path, string newExt) {

      // Ensure extension starts with '.'
      if (!newExt.StartsWith ('.'))
        newExt = "." + newExt;

      // Combine directory + filename without extension + new extension
      return Path.Combine (
          Path.GetDirectoryName (path) ?? string.Empty,
          Path.GetFileNameWithoutExtension (path) + newExt
      );
    }

    public static bool IsValidFileOrFolderName (this string name) {
      if (string.IsNullOrWhiteSpace (name))
        return false;

      return name.IndexOfAny (Path.GetInvalidFileNameChars ()) < 0;
    }

    public static bool IsValidPath (this string path) {
      try {
        _ = Path.GetFullPath (path);
        return true;
      } catch {
        return false;
      }
    }

  }
}

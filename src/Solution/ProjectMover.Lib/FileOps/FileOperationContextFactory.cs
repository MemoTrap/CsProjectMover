#pragma warning disable IDE0079
#pragma warning disable CA2208

namespace ProjectMover.Lib.FileOps;

internal static class FileOperationContextFactory {
  public async static Task<IFileOperationContext> CreateWithDryRunAwarenessAsync (IParameters parameters, CancellationToken ct) {
    if (CsProjectMover.DryRun)
      return new NullFileOperationContext ();
    else
      return await CreateAsync (parameters, ct);
  }

  public async static Task<IFileOperationContext> CreateAsync (IParameters parameters, CancellationToken ct) {
    if (parameters.RootFolder is null)
      throw new ArgumentException ("Root folder must be set in parameters", nameof (parameters));

    return parameters.FileOperations switch {
      EFileOperations.Direct => new DirectFileOperationContext (),
      EFileOperations.Svn => await createSvnFileOperationContextAsync (),
      EFileOperations.Git => throw new NotSupportedException ("Git not yet supported"),
      _ => throw new ArgumentOutOfRangeException (nameof(parameters.FileOperations), parameters.FileOperations.ToString()),
    };

    async Task<SvnFileOperationContext> createSvnFileOperationContextAsync () {
      var versionResult = await SvnClient.RunAsync (new SvnCommandBuilder ().Add ("--version"), ct);
      if (!versionResult.IsSuccess || versionResult.HasErrors)
        throw new InvalidOperationException ("SVN is not installed or not in PATH.");

      string _ = await SvnClient.GetWorkingCopyRootAsync (parameters.RootFolder!, ct);
      // if we get here we do have an SVN working copy,
      // however, our root folder is sufficient for all our SVN operations.
      return new SvnFileOperationContext (parameters.RootFolder);
    }
  }
}


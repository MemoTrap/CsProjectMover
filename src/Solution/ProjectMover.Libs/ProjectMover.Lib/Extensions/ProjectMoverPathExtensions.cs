#pragma warning disable IDE0305

namespace ProjectMover.Lib.Extensions {
  
  public static class ProjectMoverPathExtensions {
    public static string ToParaPath (this string path, IParameters parameters) {
      if (parameters.AbsPathsInUserCommunication)
        return path.ToAbsolutePath (parameters.RootFolder!);
      else
        return path.ToRelativePath (parameters.RootFolder!);
    }

    internal static Parameters ToAbsolutePath (this Parameters parameters) {
      if (parameters.RootFolder is null)
        return parameters;
      return parameters with {
        ProjectFolderOrFile = parameters.ProjectFolderOrFile?.ToAbsolutePath (parameters.RootFolder),
        DestinationFolder = parameters.DestinationFolder?.ToAbsolutePath (parameters.RootFolder),
        SolutionFolderOrFile = parameters.SolutionFolderOrFile?.ToAbsolutePath (parameters.RootFolder),
      };
    }

    internal static ProjectDecisionContext ToAbsolutePath (this ProjectDecisionContext context, IParameters parameters) {
      if (parameters.RootFolder is null)
        return context;
      return context with {
        CurrentProjectFolder = context.CurrentProjectFolder.ToAbsolutePath (parameters.RootFolder),
        DesignatedNewProjectFolder = context.DesignatedNewProjectFolder.ToAbsolutePath (parameters.RootFolder),
        SelectableSolutions = context.SelectableSolutions
          .Select (p => p.ToAbsolutePath (parameters.RootFolder))
          .ToArray (),
        SelectableDependentProjectRoots = context.SelectableDependentProjectRoots
          .Select (p => p.ToAbsolutePath (parameters.RootFolder))
          .ToArray (),
      };
    }

    internal static ProjectUserDecision ToAbsolutePath (this ProjectUserDecision decision, IParameters parameters) {
      if (parameters.RootFolder is null)
        return decision;
      return decision with {
        NewProjectFolder = decision.NewProjectFolder?.ToAbsolutePath (parameters.RootFolder),
        SelectedSolutions = decision.SelectedSolutions?
          .Select (p => p.ToAbsolutePath (parameters.RootFolder))
          .ToArray (),
        SelectedDependentProjectRoots = decision.SelectedDependentProjectRoots?
          .Select (p => p.ToAbsolutePath (parameters.RootFolder))
          .ToArray (),
      };
    }
  }
}

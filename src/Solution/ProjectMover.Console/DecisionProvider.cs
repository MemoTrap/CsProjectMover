
using ProjectMover.Lib.Api;
using ProjectMover.Lib.Misc;
using ProjectMover.Global.Lib;

namespace ProjectMover.ConsApp {
  internal sealed class ConsoleProjectDecisionProvider : IProjectDecisionProvider {

    // No preselection with this implementation
    public IEnumerable<string>? PreSelect (IEnumerable<string> projectPaths) => null;


    public ProjectUserDecision Decide (ProjectDecisionContext context) {
      // final blank line
      using var _ = new ResourceGuard (() => Console.WriteLine ());
      
      string caption1 = context.CopyModeHint ? "Copy" : "Move/Rename";
      string caption2 = $" {(context.Preselected ? "details per preselected" : "decision per")} project";
      string caption3 = $", {context.IndexInBatch}/{context.TotalInBatch}";
      string caption4 = context.RetryReason is null ? string.Empty : ", retry";
      string caption = caption1 + caption2 + caption3 + caption4 + ":";

      Console.WriteLine ();
      Console.WriteLine (caption);

      if (context.RetryReason is not null) {
        Console.WriteLine ($"  !!! {context.RetryReason} !!!");
        Console.WriteLine ();
      }

      Console.WriteLine ($"Project name:           {context.ProjectName} ({context.ProjectType})");
      Console.WriteLine ($"Current project folder: {context.CurrentProjectFolder}");
      Console.WriteLine ($"New project folder:     {context.DesignatedNewProjectFolder}");

      if (context.CopyModeHint && context.ProjectType == EProjectType.CSharp) {
        Console.WriteLine ($"Current assembly name:  {context.CurrentAssemblyName}");
        Console.WriteLine ($"New assembly name:      {context.SuggestedAssemblyName}");    
      }

      Console.WriteLine ();

      if (!context.Preselected) {
        Console.Write ("Include this project? [y/N]: ");
        var include = Console.ReadLine ();
        if (!string.Equals (include, "y", StringComparison.OrdinalIgnoreCase))
          return new ProjectUserDecision (false, null, null, null, null, null);
      }

      Console.Write ("New project name (leave empty to keep): ");
      var newProjectName = Console.ReadLine ();
      if (string.IsNullOrWhiteSpace (newProjectName))
        newProjectName = null;

      Console.Write ("New project folder (leave empty to keep): ");
      var newProjectFolder = Console.ReadLine ();
      if (string.IsNullOrWhiteSpace (newProjectFolder))
        newProjectFolder = null;
      
      List<string>? selectedDependentProjectRoots = null;
      List<string>? selectedSolutions = null;
      string? newAssemblyName = null;
      if (context.CopyModeHint) { 
        if (context.ProjectType == EProjectType.CSharp) {
          Console.Write ("New assembly name (leave empty to keep): ");
          newAssemblyName = Console.ReadLine ();
          if (string.IsNullOrWhiteSpace (newAssemblyName))
            newAssemblyName = null;
        }

      if (context.SelectableDependentProjectRoots.Any ()) {
          Console.WriteLine ();
          Console.WriteLine ("Select dependent project roots that shall reference the copied project:");
          Console.WriteLine ("(Each project root may affect additional projects, included automatically)");
          selectedDependentProjectRoots = [];
          for (int i = 0; i < context.SelectableDependentProjectRoots.Count; i++) {
            Console.WriteLine (
              $"  {i + 1}/{context.SelectableDependentProjectRoots.Count} {context.SelectableDependentProjectRoots[i]}");
            Console.Write ("  Include this project root? [y/N]: ");
            var inclRoot = Console.ReadLine ();
            if (string.Equals (inclRoot, "y", StringComparison.OrdinalIgnoreCase))
              selectedDependentProjectRoots.Add (context.SelectableDependentProjectRoots[i]);
          }
        }

        if (context.SelectableSolutions.Any ()) {
          Console.WriteLine ();
          Console.WriteLine ("Select solutions that shall reference the copied project:");
          Console.WriteLine ("(Other solutions may be included automatically by selected dependent project roots)");
          selectedSolutions = [];
          for (int i = 0; i < context.SelectableSolutions.Count; i++) {
            Console.WriteLine ($"  {i + 1}/{context.SelectableSolutions.Count} {context.SelectableSolutions[i]}");
            Console.Write ("  Include this solution? [y/N]: ");
            var inclRoot = Console.ReadLine ();
            if (string.Equals (inclRoot, "y", StringComparison.OrdinalIgnoreCase))
              selectedSolutions.Add (context.SelectableSolutions[i]);
          }
        }
      }

      var result = new ProjectUserDecision (
        Include: true, 
        NewProjectName: newProjectName, 
        NewProjectFolder: newProjectFolder, 
        NewAssemblyName: newAssemblyName,
        SelectedSolutions: selectedSolutions,
        SelectedDependentProjectRoots: selectedDependentProjectRoots
      );
      return result;

    }

  }
}

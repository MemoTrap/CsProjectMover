using static ProjectMover.Lib.Misc.ProjectDependencyGraph;

namespace ProjectMover.Lib.Misc {
  internal class DependencyPropagation {
    private readonly IReadOnlyList<ProjectFile> _candidateProjects;
    private readonly DependentSolutionsAndProjects _dependencies;


    private IReadOnlyDictionary<SolutionFile, IReadOnlyCollection<ProjectFile>>? _projectsBySolution;
    private ProjectDependencyGraph _projectDependencyGraph;

    public ProjectDependencyGraph ProjectDependencyGraph => _projectDependencyGraph;

    public DependencyPropagation (
      IReadOnlyList<ProjectFile> candidateProjects,
      DependentSolutionsAndProjects dependencies
    ) {
      _candidateProjects = candidateProjects;
      _dependencies = dependencies;
      _projectDependencyGraph = new ProjectDependencyGraph (_candidateProjects, dependencies.Projects);

      init ();
    }

    private void init () {
      Dictionary<SolutionFile, HashSet<ProjectFile>> projectsBySolution
        = [];

      var allProjects = _candidateProjects
        .Union (_dependencies.Projects)
        .ToList ();
      foreach (var sln in _dependencies.Solutions) {
        var projects = projectsBySolution.GetOrAdd (sln, _ => []);
        foreach (var projPath in sln.ProjectsAbsPath) {
          var proj = getProject (projPath);
          if (proj is null)
            continue;

          projects.Add (proj);
        }
      }

      _projectsBySolution = projectsBySolution.ToDictionary (
        kvp => kvp.Key,
        kvp => (IReadOnlyCollection<ProjectFile>)kvp.Value);

      ProjectFile? getProject (string path) {
        return allProjects.FirstOrDefault (p => p.AbsolutePath.Equals (
        path, StringComparison.OrdinalIgnoreCase));
    }

    }

    public (
      IReadOnlyCollection<ProjectFile> AffectedProjects,
      IReadOnlyCollection<SolutionFile> AffectedSolutions
    )
    ComputeCopyPropagation (
      ProjectFile projectToCopy,
      IEnumerable<ProjectFile> selectedAffectedProjects,
      IEnumerable<SolutionFile> selectedAffectedSolutions
    ) {
      if (_projectsBySolution is null)
        throw new InvalidOperationException ("Projects by solution have not been built");

      // these will accumulate the results
      HashSet<ProjectFile>? affectedProjects = [];
      HashSet<SolutionFile>? affectedSolutions = [];

      // with this we start further investigation
      IReadOnlyCollection<ProjectFile> affectedNewProjects = selectedAffectedProjects.ToList ();
      IReadOnlyCollection<SolutionFile> affectedNewSolutions = selectedAffectedSolutions.ToList ();

      // in each iteration we gather temp. results
      HashSet<ProjectFile> iterationProjects;

      bool changed;

      do {
        iterationProjects = [];
        changed = false;

        // Affected projects determine affected solutions
        // We should find some as long as there are new projects to check
        // explicit solutions
        var projectsBySolution = _projectsBySolution
          .Where (kvp =>
            affectedNewSolutions.Contains (kvp.Key) ||
            kvp.Value
             .Intersect (affectedNewProjects)
             .Any ()
           )
          .ToList ();




        if (!projectsBySolution.Any ())
          continue;

        // we'll add those 
        var solutions =
          projectsBySolution
           .Select (kvp => kvp.Key)
           .ToList ();
        affectedSolutions.UnionWith (solutions);


        // any project in any newly discovered solution may be affected 
        var projectsFromSolutions = projectsBySolution
          .SelectMany (kvp => kvp.Value)
          .Distinct ()
          .ToList ();
        projectsFromSolutions = projectsFromSolutions
          .Except (affectedProjects)
          .ToList ();

        var projects = projectsFromSolutions
          .Union (affectedNewProjects)
          .ToList ();

        // potential projects
        foreach (var project in projects) {

          var paths = _projectDependencyGraph
            .GetPaths (project, projectToCopy, ERefKind.References);

          // Projects is paths are definitely affected, due to propagation
          var projectsInPaths = paths
            .SelectMany (p => p)
            .Distinct ()
            .Except ([projectToCopy])
            .ToList ();

          // However, they may already have been accounted for
          projectsInPaths = projectsInPaths.Except (affectedProjects).ToList ();


          if (!projectsInPaths.Any ())
            continue;

          changed = true;

          iterationProjects.UnionWith (projectsInPaths);


        }

        // for the next iteration, do some shuffling

        // these have been dealt with 
        affectedProjects.UnionWith (affectedNewProjects);

        // these will be investigated with in the next iteration
        affectedNewProjects = iterationProjects.ToList ();

        // new solutions only for the first iteraration
        affectedNewSolutions = [];


      } while (changed);

      return (affectedProjects, affectedSolutions);


    }

    public IReadOnlyList<ProjectSelectionGroup> BuildSelectionGroups (
      ProjectFile targetProject,
      ERefKind refKind
    ) => ProjectDependencyGraph.BuildSelectionGroups (targetProject, refKind);

  }
}

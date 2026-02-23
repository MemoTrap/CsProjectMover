#pragma warning disable IDE0079
#pragma warning disable CA1859

using static ProjectMover.Lib.Misc.ProjectDependencyGraph;

namespace ProjectMover.Lib.Misc {
  internal class ProjectDependencyGraph {
    public enum ERefKind {
      ReferencedBy,
      References
    }

    private readonly Dictionary<string, Node> _nodesByFile =
        new (StringComparer.OrdinalIgnoreCase);
    private Dictionary<ProjectFile, Node> _nodesByProject = [];

    public sealed class Node {
      public ProjectFile Project { get; }
      public string TargetAbsRefPath { get; }
      public HashSet<Node> References { get; } = [];
      public HashSet<Node> ReferencedBy { get; } = [];

      public Node (ProjectFile project) {
        Project = project;
        TargetAbsRefPath = targetAbsRefPath (project);
      }

      public override string ToString () => $"{Project.Name}, refs={References.Count}, refBy={ReferencedBy.Count}";
    }

    public IReadOnlyCollection<Node> Nodes => _nodesByFile.Values;


    
    public ProjectDependencyGraph (ProjectFile project, IEnumerable<ProjectFile> dependentProjects) :
      this ([project], dependentProjects)
    { }

    public ProjectDependencyGraph (IEnumerable<ProjectFile> targetProjects, IEnumerable<ProjectFile> dependentProjects) :
      this (dependentProjects.Union (targetProjects))
    { }
    public ProjectDependencyGraph (IEnumerable<ProjectFile> projects) {
      build (projects);
    }

    private void build (
      IEnumerable<ProjectFile> projects
    ) {
      _nodesByFile.Clear ();

      // Register all projects
      foreach (var proj in projects)
        addNode (proj);

      // Build MUST-FOLLOW edges:
      // referenced → referencing
      foreach (var proj in projects) {

        IEnumerable<string> referencePaths = proj switch {
          CsProjectFile cs =>
            cs.ProjectReferencesAbsPath
              .Concat (cs.SharedProjectImportsAbsPath)
              .ToArray(),

          _ => []
        };

        foreach (string rp in referencePaths) {
          var referencedNode = _nodesByFile.GetValueOrDefault (rp);
          var referencingNode = _nodesByFile.GetValueOrDefault (proj.AbsolutePath);
          if (referencedNode is null || referencingNode is null)
            continue;

          referencedNode.ReferencedBy.Add (referencingNode);
          referencingNode.References.Add (referencedNode);          
        }
      }

      _nodesByProject = Nodes.ToDictionary (n => n.Project, n => n);
    }


    private void addNode (ProjectFile project) {
      var node = new Node(project); 
      _nodesByFile.TryAdd (
          node.TargetAbsRefPath,
          node);
    }




    public IReadOnlyList<ProjectSelectionGroup> BuildSelectionGroups (
      ProjectFile targetProject,
      ERefKind refKind
    ) {
      var root = _nodesByProject.GetValueOrDefault (targetProject);
      if (root is null)
        return [];

      var groups = new List<ProjectSelectionGroup> ();

      IEnumerable<Node> refs = applicableRefs (root, refKind);

      foreach (var dependent in refs) {
        var closure = getDependencyClosure (dependent.Project, refKind);

        groups.Add (new ProjectSelectionGroup (
            Root: dependent.Project,
            Members: closure
        ));
      }

      return groups;
    }

    private IReadOnlySet<ProjectFile> getDependencyClosure (ProjectFile root, ERefKind refKind) {
      if (!_nodesByProject.TryGetValue (root, out var start))
        throw new InvalidOperationException ($"Project not in graph: '{root.AbsolutePath}'");

      var result = new HashSet<Node> ();
      var stack = new Stack<Node> ();
      stack.Push (start);

      while (stack.Count > 0) {
        var node = stack.Pop ();
        if (!result.Add (node))
          continue;

        IEnumerable<Node> refs = applicableRefs (node, refKind);

        foreach (var dependent in refs)
          stack.Push (dependent);
      }

      return result.Select (n => n.Project).ToHashSet ();
    }

    public IReadOnlyList<ProjectFile> DirectReferences (
      ProjectFile targetProject,
      ERefKind refKind
    ) {
      var root = _nodesByProject.GetValueOrDefault (targetProject);
      if (root is null)
        return [];

      IEnumerable<Node> refs = applicableRefs (root, refKind);

      return refs.Select(r => r.Project).ToList();
    }

    public IReadOnlyCollection<IReadOnlyList<ProjectFile>> GetPaths (
      ProjectFile fromProject,
      ProjectFile toProject,
      ERefKind refKind
    ) {
      Node? fromNode = _nodesByProject.GetValueOrDefault (fromProject);
      if (fromNode is null)
        return [];

      List<IReadOnlyList<ProjectFile>> paths = [];
      Stack<ProjectFile> path = [];

      traverse (fromNode);

      return paths;

      void traverse ( Node node ) {
        // circular?
        if (path.Contains (node.Project))
          return;

        using var rg = new ResourceGuard (
          () => path.Push (node.Project),
          () => path.Pop ()
        ); 

        if (ReferenceEquals (node.Project, toProject)) {
          var p = path.ToList ();
          p.Reverse ();
          paths.Add (p);
          return;
        }

        IEnumerable<Node> refs = applicableRefs (node, refKind);

        foreach ( Node @ref in refs ) 
          traverse (@ref);

      }
    }

    private static IEnumerable<Node> applicableRefs (Node node, ERefKind refKind) {
      return refKind switch {
        ERefKind.References => node.References,
        ERefKind.ReferencedBy => node.ReferencedBy,
        _ => []
      };
    }

    private static string targetAbsRefPath (ProjectFile proj) {
      return proj switch {
        SharedProjectFile sh => sh.ProjItemsAbsPath,
        _ => proj.AbsolutePath
      };

    }
  }


}


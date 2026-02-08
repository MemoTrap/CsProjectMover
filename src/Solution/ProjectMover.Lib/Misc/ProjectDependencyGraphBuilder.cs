#pragma warning disable IDE0079
#pragma warning disable CA1859

namespace ProjectMover.Lib.Misc {
  internal class ProjectDependencyGraphBuilder {

    private readonly Dictionary<string, Node> _nodes =
        new (StringComparer.OrdinalIgnoreCase);

    public sealed class Node (ProjectFile project) {
      public ProjectFile Project { get; } = project;
      public HashSet<Node> References { get; } = [];
      public HashSet<Node> ReferencedBy { get; } = [];

      public override string ToString () => $"{Project.Name}, refs={References.Count}, refBy={ReferencedBy.Count}";
    }

    public IReadOnlyCollection<Node> Nodes => _nodes.Values;

#if xxx
    // TODO: add implementation for shared projects
    public void Build (
        CsProjectFile targetProject,
        IReadOnlyList<CsProjectFile> dependentProjects
    ) {
      _nodes.Clear ();

      // 1. Register all projects
      addNode (targetProject);

      foreach (var proj in dependentProjects)
        addNode (proj);

      // 2. Add edges (filtered to our set)
      foreach (var node in _nodes.Values) {
        foreach (var pr in node.Project.ProjectReferences) {
          if (_nodes.TryGetValue (pr.AbsolutePath, out var referenced)) {
            node.References.Add (referenced);
            referenced.ReferencedBy.Add (node);
          }
        }
      }
    }

    public void Build (ProjectFile targetProject, IReadOnlyList<ProjectFile> dependentProjects) {
      _nodes.Clear ();

      // 1. Register all projects
      addNode (targetProject);
      foreach (var proj in dependentProjects)
        addNode (proj);

      // 2. Add edges
      foreach (var node in _nodes.Values) {
        IEnumerable<string> references = node.Project switch {
          CsProjectFile cs => cs.ProjectReferencesAbsPath.Concat (cs.SharedProjectImportsAbsPath),
          SharedProjectFile _ => Array.Empty<string> (),
          _ => Array.Empty<string> ()
        };

        foreach (var refPath in references) {
          if (_nodes.TryGetValue (refPath, out var referenced)) {
            node.References.Add (referenced);
            referenced.ReferencedBy.Add (node);
          }
        }
      }
    }

#endif

    public void Build (
        ProjectFile targetProject,
        IReadOnlyList<ProjectFile> dependentProjects
    ) {
      _nodes.Clear ();


      string targetAbsRefPath = targetProject switch {
        SharedProjectFile sh => sh.ProjItemsAbsPath,
        _ => targetProject.AbsolutePath
      };

      // Register target + all dependents
      addNode (targetProject);
      foreach (var proj in dependentProjects)
        addNode (proj);

      // Build MUST-FOLLOW edges:
      // referenced → referencing
      foreach (var proj in dependentProjects) {

        IEnumerable<string> referencePaths = proj switch {
          CsProjectFile cs =>
            cs.ProjectReferencesAbsPath
              .Concat (cs.SharedProjectImportsAbsPath)
              .ToArray(),

          _ => []
        };

        foreach (string rp in referencePaths) {
          string refPath = rp;
          if (refPath.Equals (targetAbsRefPath, StringComparison.OrdinalIgnoreCase))
            refPath = targetProject.AbsolutePath;
          var referencedNode = _nodes.GetValueOrDefault (refPath);
          var referencingNode = _nodes.GetValueOrDefault (proj.AbsolutePath);
          if (referencedNode is null || referencingNode is null)
            continue;

          referencedNode.ReferencedBy.Add (referencingNode);
          referencingNode.References.Add (referencedNode);          
        }
      }
    }


    private void addNode (ProjectFile project) {
      _nodes.TryAdd (
          project.AbsolutePath,
          new Node (project));
    }


    private IReadOnlySet<ProjectFile> getDependencyClosure (ProjectFile root) {
      if (!_nodes.TryGetValue (root.AbsolutePath, out var start))
        throw new InvalidOperationException ("Project not in graph.");

      var result = new HashSet<Node> ();
      var stack = new Stack<Node> ();
      stack.Push (start);

      while (stack.Count > 0) {
        var node = stack.Pop ();
        if (!result.Add (node))
          continue;

        foreach (var dependent in node.ReferencedBy)
          stack.Push (dependent);
      }

      return result.Select (n => n.Project).ToHashSet ();
    }


    public IReadOnlyList<ProjectSelectionGroup> BuildSelectionGroups (
      ProjectFile targetProject
    ) {
      var root = _nodes[targetProject.AbsolutePath];

      var groups = new List<ProjectSelectionGroup> ();

      foreach (var dependent in root.ReferencedBy) {
        var closure = getDependencyClosure (dependent.Project);

        groups.Add (new ProjectSelectionGroup (
            Root: dependent.Project,
            Members: closure
        ));
      }

      return groups;
    }
  }


}


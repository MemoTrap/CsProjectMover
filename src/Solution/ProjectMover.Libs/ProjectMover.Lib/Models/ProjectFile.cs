#pragma warning disable IDE0305

namespace ProjectMover.Lib.Models {

  internal abstract class ProjectFile (string projectFilePath, string extension) : FileBase (projectFilePath, extension) {

    public IReadOnlyList<string> ProjectReferencesAbsPath { get; protected set; } = [];

    public virtual IReadOnlyList<string> SharedProjectImportsAbsPath => [];

    public IReadOnlyList<string> AllProjectReferencesAbsPath => 
      ProjectReferencesAbsPath.Union(SharedProjectImportsAbsPath).ToList();

    public abstract EProjectType ProjectType { get; }

    public abstract bool RequiresGuid { get; }

    public Guid OrigProjectGuid { get; protected set; }
    public Guid ProjectGuid { get; protected set; }

    public virtual Guid ProjectRefGuid (ProjectFile projectReference) => default;

    public abstract void ApplyProjectPlan (ProjectOperationPlan plan);

    protected abstract string Extension { get; } 
  }

}

#pragma warning disable IDE0130

using ProjectMover.Lib.Api;

namespace ProjectMover.Tests {
  using static Const;

  record ContextWithDecision (
    ProjectDecisionContext Context,
    ProjectUserDecision Decision
  );

  internal sealed class ScriptedDecisionProvider (
    Dictionary<string, ProjectUserDecision> decisionsByProjectName
  ) : IProjectDecisionProvider {

    private readonly Dictionary<string, ProjectUserDecision> _decisions = decisionsByProjectName;
    public Dictionary<string, ContextWithDecision> LoggedContextDecisionsByProjectName { get; } = [];

    public ProjectUserDecision Decide (ProjectDecisionContext context) {
      ProjectUserDecision? decision = null;
      
      if (context.RetryReason is not null)
        decision = new ProjectUserDecision ();

      if (decision is null)
        _decisions.TryGetValue (context.ProjectName, out decision);

      if (decision is null)
        _decisions.TryGetValue (WILDCARD, out decision);

      // default: skip unless explicitly scripted
      decision ??= new ProjectUserDecision ();

      LoggedContextDecisionsByProjectName[context.ProjectName] = new ContextWithDecision (context, decision);
      return decision;
    }

    public IEnumerable<string>? PreSelect (IEnumerable<string> projectPaths) => null;
  }
}

using ProjectMover.Lib.Helpers;

namespace ProjectMover.Tests.Unit {
  [TestClass]
  public sealed class Tests04_DestPathRule {
    [TestMethod]
    public void DestFolderRule01_TypicalCases () {
      string presetDestinationRoot = @"Playground\ProjectMover.TestField2\Application";

      var destinationRule = new DestinationPathRule (presetDestinationRoot);

      // project 1
      string projectSourceFullPath1 = @"Playground\ProjectMover.TestField2\RpcAppTemplate\Client\ClientLib";
      string expDesignatedTargetFullPath1 = @"Playground\ProjectMover.TestField2\Application\RpcAppTemplate\Client\ClientLib";
      string userTargetFullPath1 = @"Playground\ProjectMover.TestField2\Application\Client\ClientLib";

      string actDesignatedTargetFullPath1 = destinationRule.Apply (projectSourceFullPath1);
      destinationRule.Adapt (userTargetFullPath1);
      
      Assert.AreEqual (expDesignatedTargetFullPath1, actDesignatedTargetFullPath1, "Project 1");

      
      // project 2
      string projectSourceFullPath2 = @"Playground\ProjectMover.TestField2\RpcAppTemplate\Client\ClientProxy";
      string expDesignatedTargetFullPath2 = @"Playground\ProjectMover.TestField2\Application\Client\ClientProxy";
      string? userTargetFullPath2 = null;
     
      string actDesignatedTargetFullPath2 = destinationRule.Apply (projectSourceFullPath2);
      destinationRule.Adapt (userTargetFullPath2);
      
      Assert.AreEqual (expDesignatedTargetFullPath2, actDesignatedTargetFullPath2, "Project 2");
      
      // project 3
      string projectSourceFullPath3 = @"Playground\ProjectMover.TestField2\RpcAppTemplate\Common\CommonBcl";
      string expDesignatedTargetFullPath3 = @"Playground\ProjectMover.TestField2\Application\Common\CommonBcl";
      string? userTargetFullPath3 = null;
     
      string actDesignatedTargetFullPath3 = destinationRule.Apply (projectSourceFullPath3);
      destinationRule.Adapt (userTargetFullPath3);
      
      Assert.AreEqual (expDesignatedTargetFullPath3, actDesignatedTargetFullPath3, "Project 3");
      
      // project 4
      string projectSourceFullPath4 = projectSourceFullPath3;
      string expDesignatedTargetFullPath4 = expDesignatedTargetFullPath3;
      string? userTargetFullPath4 = @"Playground\ProjectMover.TestField2\Shared\SharedBcl";

      string actDesignatedTargetFullPath4 = destinationRule.Apply (projectSourceFullPath4);
      destinationRule.Adapt (userTargetFullPath4);
      
      Assert.AreEqual (expDesignatedTargetFullPath4, actDesignatedTargetFullPath4, "Project 4");
      
      // project 5
      string projectSourceFullPath5 = @"Playground\ProjectMover.TestField2\RpcAppTemplate\Common\CommonContract";
      string expDesignatedTargetFullPath5 = @"Playground\ProjectMover.TestField2\Shared\CommonContract";
      string? userTargetFullPath5 = @"Playground\ProjectMover.TestField2\Shared\SharedContract";

      string actDesignatedTargetFullPath5 = destinationRule.Apply (projectSourceFullPath5);
      destinationRule.Adapt (userTargetFullPath5);
      
      Assert.AreEqual (expDesignatedTargetFullPath5, actDesignatedTargetFullPath5, "Project 5");
    }
  }
}

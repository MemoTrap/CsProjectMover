using ProjectMover.Global.Lib;

namespace ProjectMover.Tests.Unit {
  [TestClass]
  public sealed class Tests03_Subsequences {
    
    [TestMethod]
    public void Subsequences01_Diff_TypicalCases () {

      const string test1 = "second shorter than first";
      
      const string firstPath = @"Playground\ProjectMover.TestField2\Application\RpcAppTemplate\Client\ClientLib";
      const string secondPath = @"Playground\ProjectMover.TestField2\Application\Client\ClientLib";
      
      const string removed = "RpcAppTemplate";
          
      var diff1 = firstPath
        .SplitPath ()
        .FindFirstSubsequenceDifference (secondPath.SplitPath (), 1, StringComparer.OrdinalIgnoreCase);

      Assert.IsTrue (diff1.HasDifference, test1);

      Assert.AreEqual (3, diff1.StartIndex, test1);
      Assert.AreEqual (1, diff1.FirstSegments.Count, test1);
      Assert.AreEqual (0, diff1.SecondSegments.Count, test1);
      Assert.AreEqual (removed, diff1.FirstSegments[0], test1);
      
      
      
      const string test2 = "second longer than first";
      
      var diff2 = secondPath
        .SplitPath ()
        .FindFirstSubsequenceDifference (firstPath.SplitPath (), 1, StringComparer.OrdinalIgnoreCase);

      Assert.IsTrue (diff2.HasDifference, test2);

      Assert.AreEqual (3, diff2.StartIndex, test2);
      Assert.AreEqual (0, diff2.FirstSegments.Count, test2);
      Assert.AreEqual (1, diff2.SecondSegments.Count, test2);
      Assert.AreEqual (removed, diff2.SecondSegments[0], test2);



      const string test3 = "difference";
      
      const string secondPath2 = @"Playground\ProjectMover.TestField2\elsewhere\Client\ClientLib";
      
      const string subseq1 = @"Application\RpcAppTemplate";
      const string subseq2 = "elsewhere";
            
      var diff3 = firstPath
        .SplitPath ()
        .FindFirstSubsequenceDifference (secondPath2.SplitPath (), 1, StringComparer.OrdinalIgnoreCase);

      Assert.IsTrue (diff3.HasDifference, test3);

      Assert.AreEqual (2, diff3.StartIndex, test3);
      Assert.AreEqual (2, diff3.FirstSegments.Count, test3);
      Assert.AreEqual (1, diff3.SecondSegments.Count, test3);
      Assert.AreEqual (subseq1, diff3.FirstSegments.JoinPath(), test3);
      Assert.AreEqual (subseq2, diff3.SecondSegments.JoinPath(), test3);
      
      
      
      const string test4 = "equality";

      var diff4 = firstPath
        .SplitPath ()
        .FindFirstSubsequenceDifference (firstPath.SplitPath (), 1, StringComparer.OrdinalIgnoreCase);

      Assert.IsFalse (diff4.HasDifference, test4);
    }

    [TestMethod]
    public void Subsequences02_Replace_TypicalCases () {

      
      const string test1 = "replacement without second on same subpath";
      
      const string firstPath1 = @"Playground\ProjectMover.TestField2\Application\RpcAppTemplate\Client\ClientLib";
      const string secondPath1 = @"Playground\ProjectMover.TestField2\Application\Client\ClientLib";
      
      const string sourcePath1 = @"Playground\ProjectMover.TestField2\Application\RpcAppTemplate\Client\ClientProxy";
      const string expDestPath1 = @"Playground\ProjectMover.TestField2\Application\Client\ClientProxy";

      var diff1 = firstPath1
        .SplitPath ()
        .FindFirstSubsequenceDifference (secondPath1.SplitPath (), 1, StringComparer.OrdinalIgnoreCase);

      var actDestPath1 = sourcePath1
        .SplitPath ()
        .ReplaceSubsequence (diff1, 1, StringComparer.OrdinalIgnoreCase)
        .JoinPath ();

      Assert.AreEqual (expDestPath1, actDestPath1, test1);




      const string test2 = "replacement without second on other subpath";

      const string sourcePath2 = @"Playground\ProjectMover.TestField2\Application\RpcAppTemplate\Common\CommonBcl";
      const string expDestPath2 = @"Playground\ProjectMover.TestField2\Application\Common\CommonBcl";

      var actDestPath2 = sourcePath2
        .SplitPath ()
        .ReplaceSubsequence (diff1, 1, StringComparer.OrdinalIgnoreCase)
        .JoinPath ();

      Assert.AreEqual (expDestPath2, actDestPath2, test2);




      const string test3 = "replacement with first and second";
      
      const string firstPath2 = sourcePath2;
      const string secondPath2 = @"Playground\ProjectMover.TestField2\Shared\SharedBcl";

      const string sourcePath3 = @"Playground\ProjectMover.TestField2\Application\RpcAppTemplate\Common\CommonContract";
      const string expDestPath3 = @"Playground\ProjectMover.TestField2\Shared\CommonContract";

      var diff2 = firstPath2
        .SplitPath ()
        .FindFirstSubsequenceDifference (secondPath2.SplitPath (), 1, StringComparer.OrdinalIgnoreCase);

      var actDestPath3 = sourcePath3
        .SplitPath ()
        .ReplaceSubsequence (diff2, 1, StringComparer.OrdinalIgnoreCase)
        .JoinPath ();

      Assert.AreEqual (expDestPath3, actDestPath3, test3);




      const string test4 = "replacement without first on other subpath";

      const string firstPath3 = sourcePath2;
      const string secondPath3 = @"Playground\ProjectMover.TestField2\Application\Others\RpcAppTemplate\Common\CommonBcl";

      const string sourcePath4 = sourcePath3;
      const string expDestPath4 = @"Playground\ProjectMover.TestField2\Application\Others\RpcAppTemplate\Common\CommonContract";

      var diff3 = firstPath3
        .SplitPath ()
        .FindFirstSubsequenceDifference (secondPath3.SplitPath (), 1, StringComparer.OrdinalIgnoreCase);
      
      var actDestPath4 = sourcePath4
        .SplitPath ()
        .ReplaceSubsequence (diff3, 1, StringComparer.OrdinalIgnoreCase)
        .JoinPath ();

      Assert.AreEqual (expDestPath4, actDestPath4, test4);



      const string test5 = "equality";

      var diff4 = firstPath1
        .SplitPath ()
        .FindFirstSubsequenceDifference (firstPath1.SplitPath (), 1, StringComparer.OrdinalIgnoreCase);
      
      var actDestPath5 = sourcePath1
        .SplitPath ()
        .ReplaceSubsequence (diff4, 1, StringComparer.OrdinalIgnoreCase)
        .JoinPath ();

      Assert.AreEqual (sourcePath1, actDestPath5, test5);
    }
  }
}

#pragma warning disable IDE0079
#pragma warning disable IDE0130
#pragma warning disable SYSLIB1045

using System.Text.RegularExpressions;

namespace ProjectMover.Tests {
  internal static class ConfirmationAssertions {
    private static readonly Regex _confirmationHeaderRegex =
        new (
            @"^QUESTION:\s*.*operation summary",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    private static readonly Regex _absPathRegex =
        new(
            @"^\s*-\s*(?<path>[A-Za-z]:\\.*?)(?=\s*$|\s*\()",
            RegexOptions.Compiled | RegexOptions.Multiline);

    public static void AssertConfirmationMessage (
        IEnumerable<string> callbackMessages,
        IEnumerable<string> expectedProjectPaths,
        IEnumerable<string> expectedSolutionPaths) {
      var message = findConfirmationMessage (callbackMessages);

      if (message is null) {
        Assert.Fail ("No 'confirmation' message found in callback messages.");
      }

      assertConfirmationMessageContents (
          message,
          expectedProjectPaths,
          expectedSolutionPaths);
    }

    public static void AssertNoConfirmationMessage (
        IEnumerable<string> callbackMessages
    ) {
      var message = findConfirmationMessage (callbackMessages);
      if (message is not null) 
        Assert.Fail ("'Confirmation' message found in callback messages.");
      
      message = findDoneMessage (callbackMessages);
      if (message is null) 
        Assert.Fail ("No 'done' message found in callback messages.");
    }

    private static string? findConfirmationMessage (IEnumerable<string> callbackMessages) {
      var matches = callbackMessages
          .Where (m => _confirmationHeaderRegex.IsMatch (m))
          .ToList ();

      if (matches.Count == 0)
        return null;

      if (matches.Count > 1)
        Assert.Fail (
            "Multiple 'confirmation' messages found:\n\n" +
            string.Join ("\n\n---\n\n", matches));

      return matches[0];
    }
    
    private static string? findDoneMessage (IEnumerable<string> callbackMessages) {
      var matches = callbackMessages
          .Where (m => m.StartsWith("INFORMATION") && m.Contains("Done"))
          .ToList ();
      if (matches.Count > 1)
        Assert.Fail (
            "Multiple 'done' messages found:\n\n" +
            string.Join ("\n\n---\n\n", matches));

      return matches.FirstOrDefault();
    }

    private static void assertConfirmationMessageContents (
        string confirmationMessage,
        IEnumerable<string> expectedProjectPaths,
        IEnumerable<string> expectedSolutionPaths
    ) {
      var expected =
          expectedProjectPaths
              .Concat (expectedSolutionPaths)
              .Select (Path.GetFullPath)
              .ToHashSet (StringComparer.OrdinalIgnoreCase);

      var foundPaths = 
        _absPathRegex.Matches (confirmationMessage)
            .Select (m => Path.GetFullPath (m.Groups["path"].Value))
            .ToHashSet (StringComparer.OrdinalIgnoreCase);

      var missing = expected.Except (foundPaths).ToList ();
      if (missing.Count > 0) {
        Assert.Fail (
            "Confirmation message is missing expected paths:\n" +
            string.Join (Environment.NewLine, missing) +
            "\n\nMessage was:\n" + confirmationMessage);
      }

      var unexpected = foundPaths.Except (expected).ToList ();
      if (unexpected.Count > 0) {
        Assert.Fail (
            "Confirmation message contains unexpected paths:\n" +
            string.Join (Environment.NewLine, unexpected) +
            "\n\nMessage was:\n" + confirmationMessage);
      }

      StringAssert.Contains (
          confirmationMessage,
          "Proceed?",
          "Confirmation message does not contain 'Proceed?'");
    }
  }
}

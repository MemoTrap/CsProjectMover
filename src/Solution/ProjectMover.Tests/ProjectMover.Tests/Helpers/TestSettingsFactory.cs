#pragma warning disable IDE0130

using ProjectMover.Lib.Api;
using ProjectMover.Lib.Misc;

namespace ProjectMover.Tests {
  internal static class TestParametersFactory {

    public static Parameters MoveSingleProject (
        TestFieldFixture field,
        string projectCsprojName,
        string destinationFolder,
        bool useSvn = false
    ) {
      return new Parameters {
        MultiMode = EMultiMode.Solutions,
        RootFolder = field.Root,
        ProjectFolderOrFile = field.Project (projectCsprojName),
        DestinationFolder = destinationFolder,
        Copy = false,
        AbsPathsInUserCommunication = true,
        FileOperations = useSvn ? EFileOperations.Svn : EFileOperations.Direct
      };
    }

    
    public static Parameters MoveMultiProjects (
        TestFieldFixture field,
        string projectFolder,
        string destinationFolder,
        bool useSvn = false
    ) {
      return new Parameters {
        MultiMode = EMultiMode.Solutions | EMultiMode.Projects,
        RootFolder = field.Root,
        ProjectFolderOrFile = Path.Combine(field.Root, projectFolder),
        DestinationFolder = destinationFolder,
        Copy = false,
        AbsPathsInUserCommunication = true,
        FileOperations = useSvn ? EFileOperations.Svn : EFileOperations.Direct
      };
    }

    public static Parameters CopySingleProject (
        TestFieldFixture field,
        string projectCsprojName,
        string destinationFolder,
        bool useSvn = false
    ) {
      return new Parameters {
        MultiMode = EMultiMode.Solutions,
        RootFolder = field.Root,
        ProjectFolderOrFile = field.Project (projectCsprojName),
        DestinationFolder = destinationFolder,
        Copy = true,
        AbsPathsInUserCommunication = true,
        FileOperations = useSvn ? EFileOperations.Svn : EFileOperations.Direct
      };
    }

    public static Parameters CopyMultiProjects (
        TestFieldFixture field,
        string projectFolder,
        string destinationFolder,
        string solutionFolder,
        bool useSvn = false
    ) {
      return new Parameters {
        MultiMode = EMultiMode.Solutions | EMultiMode.Projects,
        RootFolder = field.Root,
        ProjectFolderOrFile = projectFolder,
        DestinationFolder = destinationFolder,
        SolutionFolderOrFile = solutionFolder,
        Copy = true,
        AbsPathsInUserCommunication = true,
        FileOperations = useSvn ? EFileOperations.Svn : EFileOperations.Direct
      };
    }
  }
}

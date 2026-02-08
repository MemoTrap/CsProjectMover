# C# Project Mover
A standalone utility to move or copy C# project files and folders while maintaining references in dependent projects and Visual Studio solutions

[![GitHub All Releases](https://img.shields.io/github/downloads/MemoTrap/CsProjectMover/total)](...)
[![GitHub](https://img.shields.io/github/license/MemoTrap/CsProjectMover)](...)
[![](https://img.shields.io/badge/platform-Windows-blue)](...)
[![](https://img.shields.io/badge/language-C%23-blue)](...)
[![Visual Studio](https://img.shields.io/badge/IDE-Visual%20Studio-purple?logo=visualstudio)](...)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/MemoTrap/CsProjectMover)](https://github.com/MemoTrap/CsProjectMover/releases/latest)


![](res/mainwnd.png?raw=true)



**C# Project Mover** is a Windows desktop tool for **moving or copying .NET projects** while preserving their internal structure and build integrity.

It is designed to safely relocate **modern and legacy C#/.NET projects** into new folder layouts, repositories, or solution structures — without breaking references, metadata, or implicit project files.

---

## Overview

Moving a .NET project is deceptively complex.

Beyond the obvious `.csproj` file and its containing folder, projects will be referenced by solutions. This will affect:
- Implicitly included files
- Explicitly included files
- Relative paths in solutions
- Relative paths in referencing projects
- Project GUIDs in for copies of legacy C# projects and Shared projects  

**C# Project Mover** automates this process and helps ensure that a moved or copied project remains **buildable and structurally correct**.

---

## Key Features

### Project Relocation
- Move/rename **or** copy C#/.NET projects to a new location
- Supports single-project and multi-project scenarios
- Finds dependencies via Visual Studio Solution files

### .NET Project Awareness
- Detects and processes:
  - `.csproj` files (C# projects)
  - `.shproj` files (Shared projects)
  - `.sln`files (VS solutions)


### Legacy & Modern Compatibility
- Supports **classic .NET Framework projects**
- Supports **SDK-style projects**
- Works with mixed project layouts

### Safe Folder Selection
- Assists users in selecting valid target directories
- Automatically resolves invalid or non-existent paths
- Prevents accidental use of stale or unrelated folders

### Windows Desktop UI
- Built as a **Windows Forms application**
- Clear, dialog-driven workflow
- Dedicated dialogs for:
  - Project selection
  - Target folder selection
  - Decision / confirmation steps


### Non-Destructive by Design
- Explicit choice between *move* and *copy*
- No silent overwrites
- Designed to make project changes transparent and predictable

### SVN Integration

**C# Project Mover** supports working with Subversion (SVN)–controlled directories by invoking an external, shell-accessible SVN client. All version control operations are executed via standard SVN commands; no SVN libraries are embedded. **C# Project Mover** does not perform commits on its own. In case of errors during an operation, it will automatically revert affected working copy changes where possible, helping to leave the repository in a consistent state.


### Processing Model and Dependencies

**C# Project Mover** intentionally avoids heavyweight dependencies and code-model frameworks. Solution files (`.sln`) are processed using heuristic text parsing and regular expressions, while project files (`.csproj` and `.shproj`) are handled via XML processing. No external NuGet packages are used, and there is no dependency on Roslyn or compiler services. All functionality is implemented using the .NET Base Class Library (BCL) only, ensuring minimal complexity, predictable behavior, and long-term maintainability.

---

## Typical Use Cases

- Restructuring a solution into a new repository layout
- Synchronizing project and folder names
- Synchronizing project and assembly names
- Applying template projects by copying them into an application layout
- Migrating projects between repositories or repository subtrees
- Cleaning up historical project layouts

---

## Platform & Requirements

- **Operating System:** Windows
- **Framework:** .NET (Windows Forms), currently .Net 8
- **Target Audience:** Developers working with C#/.NET projects and Visual Studio or Visual 
- **Version Control:** A shell-accessible Subversion (SVN) command-line client must be installed and available on the system path


---

## Download
Windows setup package:

**[CsProjectMover-1.0.0-Setup.exe](https://github.com/MemoTrap/CsProjectMover/releases/download/v1.0.0/CsProjectMover-1.0.0-Setup.exe)**


### Automatic Updates

**C# Project Mover** will automatically check for new releases on GitHub. When a newer version is available, the application offers to update itself, allowing users to stay current without manually downloading an installer.


---



## [User Guide](res/UserGuide.md)

## [Developer Guide](res/DevGuide.md)

## Acknowledgements

ProjectMover builds on practical experience with real-world .NET project structures and aims to reduce the risk and effort involved in reorganizing them.


Parts of the implementation reuse and adapt several source files from the
[BookLibConnect](https://github.com/audiamus/BookLibConnect) project by audiamus.


---

## Disclaimer

**NO WARRANTY**

This software is provided "as is", without warranty of any kind, express or implied,
including but not limited to the warranties of merchantability, fitness for a
particular purpose and noninfringement.

In no event shall the authors be liable for any claim, damages or other liability.

Use at your own risk.
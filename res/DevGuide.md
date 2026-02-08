# C# Project Mover — Developer Guide

This document is intended for developers who want to understand, extend, or contribute to **C# Project Mover**.

The focus is on architecture, testing strategy, and contribution guidelines rather than end-user usage.

---

## Overview

**C# Project Mover** is a Windows Forms application for moving or copying C#/.NET projects while preserving references in dependent projects and Visual Studio solutions.

The project intentionally favors:
- Explicit behavior over hidden automation
- Predictable text- and XML-based processing
- Minimal dependencies (Base Class Library only)

The codebase is structured to make project-file handling, solution parsing, and filesystem operations easy to reason about and test.

---

## Design Principles

- **Plain BCL only**  
  No external NuGet packages are used.

- **No Roslyn or code model dependencies**  
  All processing is based on:
  - Heuristic text parsing and regular expressions for `.sln` files
  - XML processing for `.csproj` and `.shproj` files

- **Explicit workflows**  
  The application favors clear user decisions over implicit behavior.

- **Tool-first, IDE-agnostic**  
  While designed for Visual Studio project formats, the tool does not rely on Visual Studio automation APIs.

---

## Repository Structure (High-Level)

Typical structure:

- /res
- /src
- /src/InnoSetup
- /src/Solution
- /src/Solution/(app projects)
- /src/Solution/ProjectMover.Tests
- /src/TestFields


Exact structure may evolve, but the separation between application logic and tests is intentional.

---

## Test Fields

The repository contains **three dedicated test fields**, each representing scaled down  real-world usage scenarios. These are not synthetic micro-tests, but end-to-end project structures, albeit with utmost simplification. 

### Test Field #1 — SDK-Style Projects

This test field contains modern **SDK-style C# projects**.

Purpose:
- Validate handling of implicit and explicit file inclusion
- Verify reference updates in SDK-style `.csproj` files
- Validate handling of Shared project `.shproj` and `.projitems` files
- Ensure correct behavior with modern Visual Studio solutions
- Validate move and copy modes
- Validate plain file system operation and SVN file operations

---

### Test Field #2 — Template-to-Application Copy Scenario

This test field models **template projects** that are copied into an actual application solution in an SVN environment.

Purpose:
- Validate *copy* (not move) semantics
- Ensure references are updated correctly when projects are duplicated
- Support scenarios where template projects are reused across multiple applications

This reflects a common real-world workflow where projects serve as reusable building blocks.

---

### Test Field #3 — Legacy Projects

This test field contains **classic .NET Framework projects**, including:

- Explicit file lists
- `Properties` folders
- `AssemblyInfo.cs` files not listed in the project file

Purpose:
- Ensure legacy conventions are preserved
- Validate behavior when implicit assumptions are present in older project formats
- Avoid regressions when working with historical codebases
- Validate GUID handling

---

## Test Execution and SVN Behavior

### Non-SVN Tests

Tests that do **not** involve SVN:

- Run in temporary directories
- Use isolated filesystem copies
- Automatically clean up all test data after execution

These tests can be run on any machine without special setup.

---

### SVN-Based Tests

Tests involving SVN behave differently by design.

Requirements:
- Tests must run inside an **SVN working copy**
- SVN write access is required
- A shell-accessible SVN client must be available

Behavior:
- Tests will **commit an initial test playground**
- This allows the test suite to later:
  - Modify files
  - Move or copy projects
  - Revert all changes back to a known baseline

This mirrors the real application’s safety model but requires explicit opt-in by the developer.

***Note:*** *Unlike the application itself, SVN-based tests **do** perform a commit, but only once, as part of first test setup.*

---

## Git Integration

Git integration is currently **not implemented**.

This is a deliberate omission:
- Git is only used for hosting the repository on GitHub
- The author does not actively use Git beyond that context

However:

- The codebase contains **stubs and extension points** intended for future Git integration
- Contributions adding Git support are welcome

Developers interested in Git support should find clear starting points in the existing structure.

---

## Building the Project

The project targets:

- **.NET 8**
- **Windows**
- **Windows Forms**

Build using standard .NET tooling:

`dotnet build` or Visual Studio

## Debug hints

- Besides the GUI app, there is also a simple console app, not fully developed, that can be modified to development needs. It's not intended for publication.

- The GUI app supports a command line argument `-dry-run` that bypasses all file operations in step 5. Instead, the modified files are written to `FileBase` property `DryRunSave` and can be inspected there.

---

## Contributing

Contributions are welcome, especially in the following areas:

- Git integration
- Additional test fields covering new real-world scenarios
- Improvements to solution or project parsing robustness
- Documentation and examples

When contributing:

- Preserve the existing design principles
- Avoid introducing external dependencies
- Keep behavior explicit and testable


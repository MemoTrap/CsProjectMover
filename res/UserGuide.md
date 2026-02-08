# C# Project Mover — User Guide

**C# Project Mover** is a Windows desktop utility for moving or copying C#/.NET projects while preserving references in solutions and dependent projects. This guide walks through the typical workflow using the Windows Forms interface.

There will also be *Tool Tips* in all dialogs as additional help.  

---

## 1. Launching ProjectMover

Start the application from your installation directory or desktop icon. 

![](mainwnd.png?raw=true)


The main window displays the following key options:

- **Project Mode**
  - *Single project / single solution* — copy or move one project contained in a single solution.
  - *Single project / multiple solutions* — copy or move a project referenced by multiple solutions.
  - *Multiple projects / multiple solution* — copy or move several projects at once, across multiple solutions.

- **Operation Type**
  - *Move / rename* — physically move projects to the new location or rename them.
  - *Copy* — create duplicates in the target folder while keeping the originals intact.

- **SVN Integration**
  - Check this box if working with Subversion-controlled directories. Requires a shell-accessible SVN client. ProjectMover will not commit automatically and will attempt to revert changes on error.

---

## 2. Selecting Root and Target Folders

- **Root folder**: Choose the top-level folder containing the projects to be moved or copied.
- **Paths relative to root folder / Absolute paths**: Choose whether project paths in the operation should remain relative or be converted to absolute paths.
- **Root folder for projects**: Specify the starting folder for project discovery. Alternatively, specify the single project to process.
- **Destination folder**: Choose where the projects will be copied or moved.
- **Root folder for solutions**: Optionally specify the starting folder for solution discovery. Click *Rescan* to refresh the list for a new run.

**Recursion Depth:** Limits how deeply ProjectMover searches for nested projects within the projects root folder.

Click **Run** to start the discovery process. 

***Note:*** *Nothing will be modified in projects and solutions until individual and global confirmation* 

---

## 3. Project Pre-selection

After discovery of candidate projects (if there is more than one candidate), **C# Project Mover** opens a **Preselect dialog**. 

![](preselectwnd.png?raw=true)

Pre-selection is optional. If at least one project is selected, only the preselected projects will be offered in the next step.  

---

## 4. Project Details

**C# Project Mover** presents the **Copy / Move Details dialog** for each project:

![](detailswnd.png?raw=true)

- **Current project folder**: The original location of the project.
- **New project folder**: The target location where the project will be moved or copied.
- **Project name / assembly name**: Optionally override the project or assembly names. Leaving fields empty defaults to the original names.

- **Dependent project roots**: Lists other projects that reference the current project, which can be selected to be affected. Check boxes for the projects that should update their references automatically. *(**Copy** mode only)*
- **Other related solutions**: Lists solutions that reference the project, which can be selected to be affected. *(**Copy** mode only)*

Buttons:
- **OK** — Apply changes and continue to the next project.
- **Skip** — Skip this project without changes.
- **Cancel** — Abort the operation.


## 5. Final confirmation

Before executing, **C# Project Mover** shows a message box with a summary of affected projects and solutions. 

![](confirmbox.png?raw=true)

The lists include both implicitly selected (as dependencies) and explicitly selected projects and solutions, for a final review.   

---

## 6. Running the Operation

Once all projects have been reviewed:

1. **C# Project Mover** applies the move or copy operation.
2. References in dependent projects and solutions are updated to point to the new location.
3. If SVN integration is enabled, **C# Project Mover** will invoke the SVN client to add, create, move, or delete files and folders, and revert changes on error, keeping your working copy consistent. *(**Note:** C# Project Mover will not commit any changes!)*

---

## 7. Post-Operation

- Verify that all projects compile correctly in Visual Studio.
- If you used the **Copy** operation, you now have duplicate projects that can be safely moved, refactored, or reorganized.
- Original projects remain untouched unless **Move / rename** was selected.
- If SVN is used, commit the changes.

---

## Tips

- Use the **Reset** button on any dialog to restore default folder paths and project names.
- Limit recursion depth to avoid accidentally processing unrelated projects.
- Always ensure SVN working copies are clean before running operations if SVN integration is enabled.

### Legacy projects with multiple project files in a single folder or sub-folders
- **C# Project Mover** will only accept a single project per folder and no projects further down sub-folders.
- To clean legacy structures start with the sub-folders and **move** the projects away from them.
- With multiple projects in a single folder temporarily rename other project files (to something not `.csproj`) so that C# Project Mover does not find them. **Copy** the remaining single project to a new location. Repeat for the other projects in the folder. 

---


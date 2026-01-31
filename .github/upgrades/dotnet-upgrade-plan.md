# .NET 8.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 8.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 8.0 upgrade.
3. Upgrade Retro Achievement Tracker\Retro Achievement Tracker.csproj
4. Upgrade Retro Achievement Tracker.Tests\Retro Achievement Tracker.Tests.csproj
5. Run unit tests to validate upgrade in the projects listed below:
   - Retro Achievement Tracker.Tests\Retro Achievement Tracker.Tests.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

| Project name                                   | Description                                     |
|:-----------------------------------------------|:-----------------------------------------------:|
| Installer\Installer.vdproj                     | Visual Studio Installer project, not applicable |

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                        | Current Version | New Version | Description                                        |
|:------------------------------------|:---------------:|:-----------:|:---------------------------------------------------|
| MediaToolkit                        | 1.1.0.1         |             | Remove - No supported version found for .NET 8     |
| Newtonsoft.Json                     | 13.0.3          | 13.0.4      | Recommended update for .NET 8.0                    |

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### Retro Achievement Tracker\Retro Achievement Tracker.csproj modifications

Project file changes:
- Convert project file from legacy format to SDK-style format

Project properties changes:
- Target framework should be changed from `net472` to `net8.0-windows`

NuGet packages changes:
- MediaToolkit should be removed (*no supported version for .NET 8*)
- Newtonsoft.Json should be updated from `13.0.3` to `13.0.4` (*recommended for .NET 8.0*)

Other changes:
- Windows Forms application - ensure `<UseWindowsForms>true</UseWindowsForms>` is added to the project file
- Review code that uses MediaToolkit and find alternative or remove functionality

#### Retro Achievement Tracker.Tests\Retro Achievement Tracker.Tests.csproj modifications

Project properties changes:
- Target framework should be changed from `net8.0` to `net8.0-windows` (to match main project)

Other changes:
- Update project reference to main project after conversion

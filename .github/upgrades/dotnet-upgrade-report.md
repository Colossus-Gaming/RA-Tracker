# .NET 8.0 Upgrade Report

## Summary

The Retro Achievement Tracker solution was successfully upgraded from .NET Framework 4.7.2 to .NET 8.0 (Windows). All 111 unit tests pass.

## Project target framework modifications

| Project name                                              | Old Target Framework | New Target Framework | Commits                          |
|:----------------------------------------------------------|:--------------------:|:--------------------:|:---------------------------------|
| Retro Achievement Tracker\Retro Achievement Tracker.csproj| net472               | net8.0-windows       | 8df97b7a, 813f47b9, de2a6322     |
| Retro Achievement Tracker.Tests\Retro Achievement Tracker.Tests.csproj | net8.0   | net8.0-windows       | (already SDK-style)              |

## NuGet Packages

| Package Name                              | Old Version | New Version | Commit Id  |
|:------------------------------------------|:-----------:|:-----------:|:-----------|
| MediaToolkit                              | 1.1.0.1     | (removed)   | 813f47b9   |
| Newtonsoft.Json                           | 13.0.3      | 13.0.4      | 813f47b9   |
| Microsoft.AspNetCore.SystemWebAdapters    | -           | 2.2.1       | 813f47b9   |
| CoreWCF.Primitives                        | -           | 1.8.0       | 813f47b9   |
| CoreWCF.ConfigurationManager              | -           | 1.8.0       | 813f47b9   |
| CoreWCF.Http                              | -           | 1.8.0       | 813f47b9   |
| CoreWCF.WebHttp                           | -           | 1.8.0       | 813f47b9   |
| CoreWCF.NetTcp                            | -           | 1.8.0       | 813f47b9   |

## All commits

| Commit ID  | Description                                                                                      |
|:-----------|:-------------------------------------------------------------------------------------------------|
| e607dc5f   | Commit upgrade plan                                                                              |
| 8df97b7a   | Migrate project to SDK-style and .NET 8; remove AssemblyInfo                                     |
| 813f47b9   | Update Retro Achievement Tracker.csproj dependencies                                             |
| 5ce3d3c6   | Remove MediaToolkit.Model using directive                                                        |
| 14545610   | Add MediaToolkit namespaces (intermediate fix)                                                   |
| 023b1a33   | Remove MediaToolkit.Model using directive                                                        |
| 433fc77f   | Remove MediaToolkit using directive                                                              |
| ed4833f9   | Add Timer alias in AlertsController.cs                                                           |
| 2740bb89   | Disambiguate Timer in MainWindow.cs                                                              |
| c1beb28c   | Disambiguate Timer type in MainWindow.cs                                                         |
| de2a6322   | Removed MediaToolkit dependency - replaced MediaHelper.cs with .NET 8 compatible implementation  |
| 1ab407c9   | Add MediaToolkit.Model using directive (intermediate fix)                                        |

## Project feature upgrades

### Retro Achievement Tracker

Here is what changed for the project during upgrade:

- **Project converted to SDK-style format**: The legacy .csproj format was migrated to the modern SDK-style format, enabling better tooling support and simplified project management.
- **MediaToolkit removal**: The MediaToolkit package (1.1.0.1) was removed as it has no .NET 8 compatible version. The `MediaHelper` class was refactored to return a default video width (1028px) for custom notification scaling. Video duration should be configured via the UI settings instead.
- **Timer ambiguity resolved**: Added explicit type qualifications (`System.Windows.Forms.Timer`) in `MainWindow.cs` and `AlertsController.cs` to resolve ambiguity between `System.Windows.Forms.Timer` and `System.Threading.Timer`.
- **Legacy assembly references removed**: Removed obsolete .NET Framework assembly references (System.Web, System.ServiceModel, System.Design, etc.) as these are now provided by the SDK or replaced by NuGet packages.
- **AssemblyInfo.cs removed**: Assembly metadata is now handled by the SDK project system.

## Test Results

| Project                        | Passed | Failed | Skipped |
|:-------------------------------|:------:|:------:|:-------:|
| Retro Achievement Tracker.Tests| 111    | 0      | 0       |

## Next steps

- **Consider MediaToolkit alternatives**: If you need actual video dimension detection for custom notifications, consider using alternatives like:
  - **FFMpegCore** - A modern wrapper for FFmpeg with .NET 8 support
  - **MediaInfo.DotNetWrapper** - For reading media file metadata
  - Or allow users to manually specify video dimensions in the settings UI
- **Test the Windows Forms application**: Run the application manually to verify all features work correctly, especially the custom notification video scaling.
- **Review CoreWCF packages**: The upgrade added CoreWCF packages - verify these are needed or remove if not used.

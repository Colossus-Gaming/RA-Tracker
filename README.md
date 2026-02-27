# RA Tracker Layout Manager

RetroAchievements overlay tracker focused on the WPF (.NET 8) implementation.

## Current Components

- `Retro Achievement Tracker.WPF`: main app (active)
- `Retro Achievement Tracker.Tests`: automated tests
- `Installer`: Visual Studio setup project

Legacy WinForms/WebView2 sources were removed from this repository.

## Quick Start

Prerequisites:
- Windows
- .NET 8 SDK
- Visual Studio 2022 (recommended for WPF + installer workflows)

Build and test:

```powershell
dotnet restore
dotnet build
dotnet test "Retro Achievement Tracker.Tests/Retro Achievement Tracker.Tests.csproj"
```

Run the app:

```powershell
dotnet run --project "Retro Achievement Tracker.WPF/Retro Achievement Tracker.WPF.csproj"
```

## Repository Layout

- `Retro Achievement Tracker.WPF/`: WPF app source
- `Retro Achievement Tracker.Tests/`: NUnit test suite
- `Installer/`: `.vdproj` installer project
- `docs/`: guides, plans, and historical notes

## Documentation

- API docs and guides: [docs/README.md](docs/README.md)
- Active implementation plan: [docs/plans/subset-notifications-plan.md](docs/plans/subset-notifications-plan.md)

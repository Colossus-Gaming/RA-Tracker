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

## Credentials

Enter your username, Web API key, and password in the app, or supply them via environment variables (handy for repeated v2 testing). When set, env vars override the saved settings and are never written to disk:

```powershell
$env:RA_USERNAME = "your-username"
$env:RA_API_KEY  = "your-web-api-key"   # RetroAchievements control panel -> Keys
$env:RA_PASSWORD = "your-password"      # used for the behind-the-scenes session login (v2 / Cloudflare)
```

## Subset Tracking

Multi-set games (Core / Bonus / Specialty / Exclusive) are tracked per set: achievements are tagged with their set membership on the v2 path and grouped into per-set lists. The public v1 API has no subset model, so on V1 fallback a game shows a single Core set. See [docs/guides/v2-status.md](docs/guides/v2-status.md) for the current API reality.

## Repository Layout

- `Retro Achievement Tracker.WPF/`: WPF app source
- `Retro Achievement Tracker.Tests/`: NUnit test suite
- `Installer/`: `.vdproj` installer project
- `docs/`: guides, plans, and historical notes

## Documentation

- API docs and guides: [docs/README.md](docs/README.md)
- Active implementation plan: [docs/plans/subset-notifications-plan.md](docs/plans/subset-notifications-plan.md)

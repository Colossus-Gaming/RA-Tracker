# RA Tracker Layout Manager

RetroAchievements overlay tracker for streamers, built on WPF (.NET 8).

## Install

1. Download **`RATracker-Setup-<version>.exe`** from the [latest release](https://github.com/Colossus-Gaming/RA-Tracker/releases/latest).
2. Double-click it and follow the wizard — it will ask where you want the app installed.

That's the whole install. There is nothing else to set up — the .NET runtime is bundled inside the
installer, so you do not need the .NET SDK, Visual Studio, or any command line. Setup adds a Desktop
icon and a Start Menu entry.

Windows may show a **"Windows protected your PC"** prompt, because the build is not code-signed yet.
Choose **More info → Run anyway**.

### Where it installs

The wizard defaults to `%LOCALAPPDATA%\ColossusGaming.RATracker`, and you can browse to anywhere your
Windows account can write to — another drive works fine.

Locations that need administrator rights, such as `C:\Program Files`, are rejected on purpose. The
app updates itself silently without elevation, so installing somewhere it cannot write would appear
to work and then quietly break every future update.

To script an unattended install, the wizard accepts the usual Inno Setup switches:

```powershell
.\RATracker-Setup-1.9.1.exe /VERYSILENT /DIR="D:\Apps\RATracker"
```

### Updates

The app updates itself. It checks the latest release on startup, downloads anything new in the
background, and installs it the next time you close the app — so an update can never restart you
mid-stream. When one is waiting you will see a note in the header.

## Credentials

Enter your username, Web API key, and password in the app, or supply them via environment variables (handy for repeated v2 testing). When set, env vars override the saved settings and are never written to disk:

```powershell
$env:RA_USERNAME = "your-username"
$env:RA_API_KEY  = "your-web-api-key"   # RetroAchievements control panel -> Keys
$env:RA_PASSWORD = "your-password"      # used for the behind-the-scenes session login (v2 / Cloudflare)
```

## Subset Tracking

Multi-set games (Core / Bonus / Specialty / Exclusive) are tracked per set: achievements are tagged with their set membership on the v2 path and grouped into per-set lists. The public v1 API has no subset model, so on V1 fallback a game shows a single Core set. See [docs/guides/v2-status.md](docs/guides/v2-status.md) for the current API reality.

## Development

Only needed if you are working on the app itself — users should install from the release above.

Prerequisites: Windows and the .NET 8 SDK.

```powershell
dotnet restore
dotnet build
dotnet test "Retro Achievement Tracker.Tests/Retro Achievement Tracker.Tests.csproj"
dotnet run --project "Retro Achievement Tracker.WPF/Retro Achievement Tracker.WPF.csproj"
```

Releases are produced by [`.github/workflows/release.yml`](.github/workflows/release.yml) when a
`v*` tag is pushed on `main`: it publishes a self-contained build, packages it with Velopack, and
uploads the installer. Developer tooling (the `--probe-*` commands, file logging, the debug game
pin) compiles only in Debug builds, so a Release build never carries it.

## Repository Layout

- `Retro Achievement Tracker.WPF/`: WPF app source
- `Retro Achievement Tracker.Tests/`: NUnit test suite
- `installer/`: Inno Setup wizard that wraps the Velopack installer
- `.github/workflows/`: CI and release automation
- `docs/`: guides, plans, and historical notes

## Documentation

- API docs and guides: [docs/README.md](docs/README.md)
- Active implementation plan: [docs/plans/subset-notifications-plan.md](docs/plans/subset-notifications-plan.md)

# Copilot Instructions — RA Tracker Layout Manager

## Project

WPF desktop app (.NET 8) tracking RetroAchievements user progress with overlay windows for OBS streaming. The active project is `Retro Achievement Tracker.WPF/`; tests are in `Retro Achievement Tracker.Tests/`.

## Prerequisites

- Windows 10/11
- .NET 8 SDK
- Microsoft Edge WebView2 Runtime (used for login + guides)

## Build

```powershell
dotnet restore
dotnet build "Retro Achievement Tracker.WPF/Retro Achievement Tracker.WPF.csproj" -c Debug
```

## Test

Fast (unit + service tests only):
```powershell
dotnet test "Retro Achievement Tracker.Tests/Retro Achievement Tracker.Tests.csproj" --filter "Category!=Integration"
```

Integration tests (launches real app via FlaUI — requires display):
```powershell
dotnet test "Retro Achievement Tracker.Tests/Retro Achievement Tracker.Tests.csproj" --filter "Category=Integration"
```

All tests:
```powershell
dotnet test "Retro Achievement Tracker.Tests/Retro Achievement Tracker.Tests.csproj"
```

Baseline: 278 unit tests pass (`Category!=Integration`), 0 failing.

## Credentials (Environment Variables)

For local dev/testing, set these instead of using the UI. When set (non-empty) they override the settings file and are never persisted:

```powershell
$env:RA_USERNAME = "your-username"
$env:RA_API_KEY  = "your-web-api-key"   # control panel -> Keys
$env:RA_PASSWORD = "your-password"      # for the WebView2 session login (v2 / Cloudflare bypass)
```

Read by `Services/EnvironmentCredentials.cs`, applied in `MainViewModel`.

## Project Structure

```
Retro Achievement Tracker.WPF/
  Controls/         # Custom WPF controls
  Converters/       # XAML value converters
  Http/V2/          # V2 API client, JSON:API parser, query builder
  Models/           # AppSettings, Achievement, GameInfo, UserSummary
  Services/         # Business logic (polling, progress, settings, session)
  ViewModels/       # MVVM ViewModels
  Views/            # Overlay windows + LoginWindow
  MainWindow.xaml   # Main control panel
  App.xaml          # Application entry point

Retro Achievement Tracker.Tests/
  IntegrationTests/ # FlaUI UI automation tests (Category=Integration)
  V2ApiTests/       # V2 client, mapper, progress-service, subset mapping tests
  ViewModelTests/   # ViewModel unit tests (multi-set, focus set name, env override)
  ModelTests/       # Real-model tests (Achievement, GameInfo, UserSummary, UserGameProgress)
  ServiceTests/     # EnvironmentCredentials, SettingsService encryption, subset grouping
  ConverterTests/   # XAML value converters
```

Tests target the **real** WPF/model types via the project reference — do not create duplicate stand-in model classes in the test project.

## Key Conventions

- **MVVM**: ViewModels extend `ViewModelBase`; commands use `RelayCommand`
- **Settings**: JSON-based via `SettingsService.cs` → `%APPDATA%/RATracker/settings.json`
- **Credentials**: env vars (`RA_USERNAME`/`RA_API_KEY`/`RA_PASSWORD`) override settings; passwords/keys stored with DPAPI
- **Logging**: `System.Diagnostics.Debug.WriteLine` with `[Tag]` prefixes
- **API auth**: Session cookies (WebView2 → CookieContainer) + X-API-Key header
- **V2 primary, V1 fallback**: `HybridProgressService` tries V2 first, falls back to V1
- **Subsets**: achievements are tagged with `SetId`/`SetType`/`SetName` in `V2ProgressService`; `AchievementTrackingService` groups them per set. V1 has no subset model (single Core set). The v2 contract is not publicly published — keep mappers defensive.
- **Overlays**: Each has its own View + ViewModel; data pushed from `MainWindow.xaml.cs`

## Do Not

- Modify V1 API client code (code-frozen)
- Use Python scripts for file manipulation
- Store secrets in source (passwords use DPAPI encryption)
- Treat usernames as permanent identifiers (use ULID from V2)

## Key Files for Common Tasks

| Task | Files |
|------|-------|
| API polling logic | `Services/AchievementTrackingService.cs`, `ViewModels/MainViewModel.cs` |
| V2 API calls | `Http/V2/V2Client.cs`, `Http/V2/V2QueryBuilder.cs` |
| V1 fallback | `Services/HybridProgressService.cs` |
| Session auth | `Views/LoginWindow.xaml.cs`, `Services/SessionService.cs` |
| Settings | `Models/AppSettings.cs`, `Services/SettingsService.cs` |
| Overlay data flow | `MainWindow.xaml.cs` → `PushGameDataToOverlays()` |
| Service creation | `Services/ServiceFactory.cs` |

## Documentation

- Task tracker: `TASKS.md`
- API docs: `docs/README.md`
- V2 API reference: `docs/v2/README.md`
- Test data: `docs/testing/README.md`

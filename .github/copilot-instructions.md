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
  IntegrationTests/ # FlaUI UI automation tests
  V2ApiTests/       # V2 API contract/integration tests
  ViewModelTests/   # ViewModel unit tests
```

## Key Conventions

- **MVVM**: ViewModels extend `ViewModelBase`; commands use `RelayCommand`
- **Settings**: JSON-based via `SettingsService.cs` → `%APPDATA%/RATracker/settings.json`
- **Logging**: `System.Diagnostics.Debug.WriteLine` with `[Tag]` prefixes
- **API auth**: Session cookies (WebView2 → CookieContainer) + X-API-Key header
- **V2 primary, V1 fallback**: `HybridProgressService` tries V2 first, falls back to V1
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

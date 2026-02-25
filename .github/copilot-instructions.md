# Retro Achievements Layout Manager - Development Documentation

---

## AI Assistant Guidelines

> **IMPORTANT: Instructions for GitHub Copilot and AI Assistants**

### Preferred Approach for Code Changes

1. **DO NOT use terminal commands or PowerShell scripts** unless absolutely necessary
   - Prefer using direct file editing tools (`replace_string_in_file`, `create_file`, `get_file`)
   - Avoid `run_command_in_terminal` for tasks that can be accomplished with file operations

2. **DO NOT use Python scripts** for file manipulation or code generation
   - This workspace contains both a .NET Framework 4.7.2 Windows Forms project (legacy) and a .NET 8 WPF project (new)
   - All tooling should stay within the C# / .NET ecosystem

3. **Prefer built-in tools over external commands:**
   - Use `get_file` to read file contents instead of `cat` or `Get-Content`
   - Use `replace_string_in_file` or `multi_replace_string_in_file` instead of `sed`, `awk`, or regex replace scripts
   - Use `create_file` instead of `echo > file` or `New-Item`
   - Use `file_search` instead of `find`, `ls`, or `Get-ChildItem`
   - Use `run_build` instead of `dotnet build` or `msbuild` commands

4. **When terminal commands ARE necessary:**
   - Git operations (commit, push, pull, branch)
   - NuGet package restore if build fails
   - Running the application for testing

5. **Follow the existing code patterns** documented in this file rather than introducing new paradigms

---

## WPF Migration Status (.NET 8)

> **IMPORTANT: Active Migration in Progress**

The project is being migrated from Windows Forms (.NET Framework 4.7.2) to WPF (.NET 8). The WPF version is in `Retro Achievement Tracker.WPF/`.

### Migration Progress

| Phase | Description | Status |
|-------|-------------|--------|
| 1-6 | Core infrastructure, models, services, API clients | ✅ Complete |
| 7 | StreamLabelService with multi-set support | ✅ Complete |
| 8 | Stream Labels UI in Settings tab | ✅ Complete |
| 9 | Overlay Windows wiring & auto-launch | ✅ Complete |

### Implemented Overlays (WPF)

| Overlay | View | ViewModel | Auto-Launch |
|---------|------|-----------|-------------|
| Focus | `FocusOverlay.xaml` | `FocusViewModel.cs` | ✅ |
| Alerts | `AlertsOverlay.xaml` | `AlertsViewModel.cs` | ✅ |
| User Info | `UserInfoOverlay.xaml` | `UserInfoViewModel.cs` | ✅ |
| Game Info | `GameInfoOverlay.xaml` | `GameInfoViewModel.cs` | ✅ |
| Game Progress | `GameProgressOverlay.xaml` | `GameProgressViewModel.cs` | ✅ |
| Recent Unlocks | `RecentUnlocksOverlay.xaml` | `RecentUnlocksViewModel.cs` | ✅ |
| Related Media | `RelatedMediaOverlay.xaml` | `RelatedMediaViewModel.cs` | ✅ |
| Achievement List | `AchievementListOverlay.xaml` | `AchievementListViewModel.cs` | ✅ |

### WPF Project Structure

```
Retro Achievement Tracker.WPF/
├── Controls/            # Custom WPF controls (OutlinedTextBlock)
├── Converters/          # Value converters for XAML bindings
├── Http/V2/             # V2 API client, JSON:API parser, query builder
│   ├── JsonApi/         # JSON:API document parsing
│   └── Mappers/         # V2 resource to model mappers
├── Models/              # AppSettings, AchievementSet, GameInfo, UserSummary
├── Services/            # Service layer for API, settings, stream labels
│   ├── AchievementTrackingService.cs  # Polling and unlock detection
│   ├── HybridProgressService.cs       # V1/V2 hybrid with fallback
│   ├── IProgressService.cs            # Progress abstraction interface
│   ├── ServiceFactory.cs              # Creates services based on feature flags
│   ├── SettingsService.cs             # JSON settings persistence
│   └── StreamLabelService.cs          # OBS text file generation
├── ViewModels/          # MVVM ViewModels for all views
├── Views/               # Overlay window XAML files
├── MainWindow.xaml      # Main control panel
└── App.xaml             # Application resources and startup
```

### Key WPF Differences from WinForms

| Feature | WinForms (Legacy) | WPF (New) |
|---------|-------------------|-----------|
| Overlay Rendering | WebView2 + HTML/CSS/JS | Native XAML + WPF animations |
| State Management | Singleton Controllers | MVVM with ViewModels |
| Settings | `Properties.Settings.Default` | JSON-based `SettingsService` |
| Data Binding | Manual event handlers | XAML bindings + INotifyPropertyChanged |
| Animations | JavaScript animations | WPF Storyboards |

### Working with WPF Code

When modifying the WPF project:

1. **ViewModels**: Extend `ViewModelBase` for property change notifications
2. **Commands**: Use `RelayCommand` for ICommand implementations
3. **Overlays**: Follow the pattern in `FocusOverlay.xaml`/`.cs` for new overlays
4. **Settings**: Add new settings to `AppSettings.cs` and `SettingsService.cs`
5. **Styles**: Use the color resources defined in `MainWindow.xaml` (BackgroundDark, AccentBlue, etc.)

### WPF Services Layer

The WPF project uses a robust services architecture:

#### Service Factory (`ServiceFactory.cs`)
Creates service instances based on feature flags:
- `GetMetadataService()` - V2 metadata operations (systems, games, users)
- `GetProgressService()` - V1/V2 hybrid progress operations
- `GetTrackingService()` - Polling and unlock detection

#### Feature Flags (`IFeatureFlagService.cs`)
Controls API version selection at runtime:
```csharp
public interface IFeatureFlagService
{
    bool UseV2ForMetadata { get; }     // Default: true
    bool UseV2ForProgress { get; }     // Default: true  
    bool UseV2ForUserLookup { get; }   // Default: true
    bool EnableMultiSet { get; }       // Default: true
    bool EnableV1Fallback { get; }     // Default: true
    bool EnableApiLogging { get; }     // Default: false
}
```

#### Progress Service (`IProgressService.cs`)
Abstraction for fetching user progress:
- `GetUserGameProgressAsync()` - Game progress with unlock status
- `GetUserRecentAchievementsAsync()` - Recently unlocked achievements
- `GetUserRecentlyPlayedGamesAsync()` - Recently played games
- `DetectNewUnlocks()` - Compare states to find new unlocks
- `GetUserSummaryAsync()` - User rank and points

#### Hybrid Progress Service (`HybridProgressService.cs`)
Implements `IProgressService` with automatic V1 fallback:
- Attempts V2 API first when `UseV2ForProgress` is true
- Falls back to V1 API on failure when `EnableV1Fallback` is true
- Includes logging and observability

---

## Project Overview

**Retro Achievements Layout Manager** is a Windows Forms desktop application built on **.NET Framework 4.7.2** that integrates with the [RetroAchievements.org](https://retroachievements.org) Web API to track user progress in retro games during livestreaming sessions. The application provides customizable overlay windows and stream labels for OBS/streaming software integration.

### Key Features
- Real-time polling of RetroAchievements API for user progress
- Achievement unlock notifications with customizable animations
- Multiple overlay windows (Focus, Alerts, User Info, Game Info, Game Progress, Recent Unlocks, Achievement List, Related Media)
- Stream label file generation for OBS text sources
- LaunchBox integration for enhanced game media
- Auto-update functionality via GitHub releases

---

## Architecture Overview

### Project Structure

```
Retro Achievement Tracker/
├── Controllers/          # Singleton controllers managing overlay windows and state
├── Forms/               # Windows Forms UI components
├── Http/                # API client for RetroAchievements
├── Models/              # Data models and JSON converters
├── Properties/          # Settings, resources, and assembly info
├── Resources/           # HTML templates, images, and icons
├── images/              # Static image assets
└── video/               # Notification video assets (webm)
```

### Design Patterns

1. **Singleton Pattern**: All controllers use the singleton pattern for global state management
2. **MVC-like Structure**: Controllers manage business logic, Forms handle UI, Models represent data
3. **Observer Pattern**: Timer-based polling with event-driven UI updates

---

## Core Components

### Entry Point
- **`Program.cs`**: Standard WinForms entry point launching `MainWindow`

### Main Window (`Forms/MainWindow.cs`)
The central hub managing:
- User authentication (Username + Web API Key)
- API polling timer (30-second intervals)
- Settings persistence via `Properties.Settings`
- Coordination of all overlay controllers

#### Class: `MainWindow`

The `MainWindow` class is a partial Windows Form class that serves as the application's primary control center. It orchestrates API polling, achievement tracking, overlay window management, and user settings.

##### Private Fields

| Field | Type | Description |
|-------|------|-------------|
| `ShouldRun` | `bool` | Controls whether the polling timer should continue running |
| `IsChanging` | `bool` | Prevents recursive event handling during UI updates |
| `IsBooting` | `bool` | Indicates application is in startup phase (auto-launching windows) |
| `IsStarting` | `bool` | Indicates the start button was just clicked |
| `CurrentlyViewingIndex` | `int` | Index of currently selected achievement in the focus selector |
| `UserAndGameTimerCounter` | `int` | Countdown ticks until next API poll (60 ticks = 30 seconds) |
| `MaxCheevoCount` | `int` | Tracks maximum unlocked achievements to detect new unlocks |
| `UserSummary` | `UserSummary` | Cached user profile data from API |
| `GameInfoAndProgress` | `GameInfo` | Cached game and achievement progress data |
| `CurrentlyViewingAchievement` | `Achievement` | The achievement currently displayed in the focus selector |
| `OldUnlockedAchievements` | `List<Achievement>` | Previous poll's unlocked achievements for comparison |
| `UserAndGameUpdateTimer` | `Timer` | Windows Forms timer for API polling |
| `RetroAchievementsAPIClient` | `RetroAchievementAPIClient` | HTTP client instance for API calls |

##### Computed Properties

| Property | Type | Description |
|----------|------|-------------|
| `LockedAchievements` | `List<Achievement>` | Achievements without a `DateEarned` value |
| `UnlockedAchievements` | `List<Achievement>` | Achievements with a `DateEarned` value |
| `Username` | `string` | Persisted RA username from settings |
| `WebAPIKey` | `string` | Persisted RA Web API key from settings |
| `PreviouslyPlayedGameId` | `long` | Last played game ID for manual search fallback |

##### Lifecycle Methods

| Method | Description |
|--------|-------------|
| `MainWindow()` | Constructor - initializes state flags, calls `AutoUpdate()` and `InitializeComponent()` |
| `OnShown(EventArgs)` | Sets up timer, loads properties, creates folders, optionally auto-starts polling |
| `OnClosed(EventArgs)` | Saves credentials, clears stream labels, closes all overlay windows |

##### Core Polling Logic

| Method | Description |
|--------|-------------|
| `UpdateFromSite(object, EventArgs)` | **Main polling handler** - Called every 500ms by timer. Manages countdown, fetches API data, detects game changes and new achievements |
| `StartTimer()` | Initializes and starts the polling timer with 500ms interval |
| `UpdateGameProgress(bool sameGame)` | Processes achievement changes, triggers notifications, updates all overlays. Returns `true` if user stats need refresh |
| `CanStart()` | Validates username and API key are provided |

##### Achievement Focus Management

| Method | Description |
|--------|-------------|
| `FindNewFocus()` | Selects next focus achievement based on `RefocusBehavior` setting when current focus is unlocked |
| `UpdateCurrentlyViewingAchievement()` | Updates UI controls for the achievement selector panel |
| `SetFocus()` | Pushes current achievement to `FocusController` and stream labels |
| `MoveFocusIndexPrev_Click` | Navigates to previous locked achievement |
| `MoveFocusIndexNext_Click` | Navigates to next locked achievement |

##### UI Update Methods

| Method | Description |
|--------|-------------|
| `UpdateUserInfo()` | Refreshes user info UI labels and `UserInfoController` |
| `UpdateGameInfo()` | Refreshes game info UI labels, progress displays, and all related controllers |
| `UpdateFocusButtons()` | Enables/disables prev/next/set buttons based on locked achievements |
| `UpdateActivePollingLabel(string)` | Updates the status label with current polling state |
| `LoadProperties()` | Loads all saved settings into UI controls on startup |

##### Event Handlers - Settings Changes

| Method | Trigger | Description |
|--------|---------|-------------|
| `FontColorPictureBox_Click` | Color picker clicks | Opens color dialog, updates controller colors |
| `FontFamilyComboBox_SelectedIndexChanged` | Font dropdown changes | Updates controller font families |
| `CustomNumericUpDown_ValueChanged` | Numeric up/down changes | Updates outline sizes, positions, animation timings |
| `FeatureEnablementCheckBox_CheckedChanged` | Checkbox toggles | Toggles auto-launch, borders, outlines, field visibility |
| `AdvancedCheckBox_Click` | Advanced mode toggle | Switches between simple/advanced font settings |
| `OverrideTextBox_TextChanged` | Text field changes | Updates custom label names |

##### Event Handlers - Window Management

| Method | Trigger | Description |
|--------|---------|-------------|
| `ShowWindowButton_Click` | "Open Window" buttons | Opens corresponding overlay window via controller |
| `StartButton_Click` | Start button | Initializes API client, enables polling |
| `StopButton_Click` | Stop button | Stops polling, resets UI state |

##### Event Handlers - Alerts Configuration

| Method | Trigger | Description |
|--------|---------|-------------|
| `CustomAlertsCheckBox_CheckedChanged` | Alert checkboxes | Toggles achievement/mastery alerts, custom files, edit mode |
| `SelectCustomAlertButton_Click` | File select buttons | Opens file dialog for custom notification videos |
| `ShowAlertButton_Click` | Play buttons | Triggers test achievement/mastery notifications |
| `NotificationAnimationComboBox_SelectedIndexChanged` | Animation dropdowns | Sets animation direction (UP, DOWN, LEFT, RIGHT, STATIC) |

##### Event Handlers - Radio Button Groups

| Method | Trigger | Description |
|--------|---------|-------------|
| `DividerCharacter_RadioButtonClicked` | Divider radio buttons | Sets progress divider character (/, :, .) |
| `RefocusBehavior_RadioButtonCheckChanged` | Refocus radio buttons | Sets behavior when focused achievement is unlocked |
| `RelatedMedia_RadioButtonCheckChanged` | Media source radio buttons | Selects RA or LaunchBox media source |

##### LaunchBox Integration

| Method | Description |
|--------|-------------|
| `SetRelatedMediaPathButton_Click` | Opens folder browser to select LaunchBox installation path |
| `UpdateLaunchBoxIntegrationState()` | Enables/disables LaunchBox media options based on path validity |
| `UpdateLaunchBoxReferences()` | Parses LaunchBox XML data to find matching game media files |

##### Utility Methods

| Method | Description |
|--------|-------------|
| `CreateFolders()` | Creates `stream-labels/` subdirectories on startup |
| `AutoUpdate()` | Checks GitHub for application updates via AutoUpdater.NET |
| `SetFontFamilyBox(ComboBox, FontFamily)` | Populates font family dropdown and selects current value |
| `BrowserSensitiveControl_Click` | Opens RetroAchievements URLs in browser when clicking profile/game images |
| `ManualSearchButton_Click` | Fetches game by ID for offline/fallback mode |

##### State Management Pattern

The `IsChanging` flag prevents infinite loops when programmatically updating UI controls:

```csharp
private void SomeEventHandler(object sender, EventArgs e)
{
    if (!IsChanging)
    {
        IsChanging = true;
        // Make changes that might trigger other events
        SomeController.Instance.Property = newValue;
        IsChanging = false;
    }
}
```

##### Polling Flow Diagram

```
StartButton_Click()
    │
    ▼
StartTimer() ──► Timer.Tick (500ms)
                    │
                    ▼
              UpdateFromSite()
                    │
                    ├─► Countdown not zero? Decrement & return
                    │
                    ▼
              GetUserSummary() [if null]
                    │
                    ▼
              GetRecentlyPlayedGames()
                    │
                    ▼
              GetRecentAchievements()
                    │
                    ├─► Same game & no new unlocks? Skip
                    │
                    ▼
              GetGameInfoAndProgress()
                    │
                    ▼
              UpdateGameProgress()
                    │
                    ├─► Detect new unlocks
                    ├─► Queue notifications
                    ├─► Update stream labels
                    ├─► Refresh overlays
                    │
                    ▼
              StartTimer() [restart countdown]
```

### API Client (`Http/RetroAchievementAPIClient.cs`)
Async HTTP client using `System.Net.Http.HttpClient` with endpoints:

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GetUserSummary()` | `API_GetUserSummary.php` | User profile, rank, points |
| `GetGameInfoAndProgress()` | `API_GetGameInfoAndUserProgress.php` | Game details with user progress |
| `GetGameInfoExtended()` | `API_GetGameExtended.php` | Extended game info (manual search) |
| `GetRecentlyPlayedGames()` | `API_GetUserRecentlyPlayedGames.php` | Recently played games list |
| `GetRecentAchievements()` | `API_GetUserRecentAchievements.php` | Recent achievement unlocks |
| `GetRankAndScore()` | `API_GetUserRankAndScore.php` | User ranking data |

### Controllers

| Controller | Purpose | Singleton Access |
|------------|---------|------------------|
| `FocusController` | Manages focused achievement display | `FocusController.Instance` |
| `AlertsController` | Achievement/mastery notification popups | `AlertsController.Instance` |
| `UserInfoController` | User profile overlay | `UserInfoController.Instance` |
| `GameInfoController` | Game metadata overlay | `GameInfoController.Instance` |
| `GameProgressController` | Progress statistics overlay | `GameProgressController.Instance` |
| `RecentUnlocksController` | Recent achievements list | `RecentUnlocksController.Instance` |
| `AchievementListController` | Full achievement list view | `AchievementListController.Instance` |
| `RelatedMediaController` | Game box art/screenshots | `RelatedMediaController.Instance` |
| `StreamLabelController` | File-based stream labels for OBS | `StreamLabelController.Instance` |

### Models

| Model | Description |
|-------|-------------|
| `Achievement` | Achievement data with `IEquatable`, `IComparable`, `ICloneable` |
| `GameInfo` | Game metadata with console ID to name mapping |
| `UserSummary` | User profile and statistics |
| `UserRankAndScore` | Rank and score data |
| `NotificationRequest` | Queue item for notification system |
| `Constants` | API URLs and static strings |
| `MediaHelper` | Video/media utility functions |

### JSON Converters
Custom `JsonConverter` implementations for deserializing RA API responses:
- `AchievementConverter`
- `GameInfoConverter`
- `UserSummaryConverter`
- `UserRankAndScoreConverter`

---

## Technology Stack

### Framework & Runtime
- **.NET Framework 4.7.2**
- **Windows Forms** for desktop UI
- **WebView2** for HTML/CSS/JS overlay rendering

### NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Web.WebView2` | 1.0.2739.15 | Chromium-based web view for overlays |
| `Newtonsoft.Json` | 13.0.3 | JSON serialization/deserialization |
| `Autoupdater.NET.Official` | 1.9.2 | Auto-update from GitHub releases |
| `HtmlAgilityPack` | 1.11.65 | HTML parsing |
| `MediaToolkit` | 1.1.0.1 | Video file processing |

---

## Overlay System

### WebView2 Integration
Overlay windows use Microsoft Edge WebView2 to render HTML/CSS/JS content:

```csharp
// Example from FocusWindow.cs
await webView21.EnsureCoreWebView2Async(null);
webView21.CoreWebView2.SetVirtualHostNameToFolderMapping(
    "appassets.tracker", @"images", 
    CoreWebView2HostResourceAccessKind.DenyCors);
webView21.NavigateToString(Resources.focus_window);
```

### JavaScript Interop
Controllers communicate with WebView2 via `ExecuteScriptAsync`:

```csharp
webView21.ExecuteScriptAsync($"addAchievement({JsonConvert.SerializeObject(achievement)});");
webView21.ExecuteScriptAsync("fadeInFocus();");
```

### HTML Templates
Located in `Resources/`:
- `focus-window.html`
- `alerts-window.html`
- `user-info-window.html`
- `game-info-window.html`
- `game-progress-window.html`
- `recent-achievements-window.html`
- `achievement-list-window.html`
- `related-media-window.html`

---

## Stream Labels

The `StreamLabelController` writes text files to the `stream-labels/` directory for OBS integration:

```
stream-labels/
├── user-info/
│   ├── rank.txt
│   ├── points.txt
│   ├── true-points.txt
│   ├── ratio.txt
│   └── data.json
├── game-info/
│   ├── title.txt
│   ├── console.txt
│   ├── developer.txt
│   ├── publisher.txt
│   ├── genre.txt
│   ├── released.txt
│   ├── achievements.txt
│   ├── points.txt
│   ├── completed.txt
│   └── data.json
├── focus/
│   ├── title.txt
│   ├── description.txt
│   ├── points.txt
│   └── data.json
├── last-five/
│   └── last-{1-5}-{title|description|points|data}.{txt|json}
└── alerts/
    └── data.json
```

---

## Settings Persistence

User settings are stored via `Properties.Settings.Default`:

```csharp
// Saving
Settings.Default.ra_username = value;
Settings.Default.Save();

// Loading
string username = Settings.Default.ra_username;
```

Settings include:
- API credentials
- Window positions and sizes
- Font families, colors, outlines
- Auto-launch preferences
- Custom notification file paths

---

## RetroAchievements API

### Base URLs
- API: `https://retroachievements.org`
- Media: `http://media.retroachievements.org`
- Profile Pictures: `https://retroachievements.org/UserPic/{username}.png`

### Authentication
All API calls require:
- `z` - Username
- `y` - Web API Key (from RA account settings)
- `u` - Target username (usually same as `z`)

### Rate Limiting
The application polls every 30 seconds (60 timer ticks × 500ms interval).

---

## Console ID Mapping

The `GameInfo.ConsoleId` property automatically maps numeric IDs to console names:

| ID | Console |
|----|---------|
| 1 | Sega Genesis |
| 2 | Nintendo 64 |
| 3 | SNES |
| 7 | NES |
| 12 | PlayStation |
| 27 | Arcade |
| ... | (see `GameInfo.cs` for full list) |

---

## Build & Deployment

### Build Configurations
- `Debug|AnyCPU` - Development build
- `Release|AnyCPU` - Production build
- `Debug|x86`, `Debug|x64`, `Release|x86` - Platform-specific builds

### Auto-Update
Updates are distributed via GitHub releases:
```
https://github.com/Colossus-Gaming/retroachievements-layout-manager/releases/download/release-management/ra-layout-manager-release.xml
```

### Output Directories
- Debug: `bin/Debug/`
- Release: `bin/Release/`
- Required runtime files: `video/` folder with notification webm files

---

## Common Development Tasks

### Adding a New Overlay Window
1. Create `Forms/NewWindow.cs` with WebView2 control
2. Create `Controllers/NewController.cs` as singleton
3. Add HTML template to `Resources/`
4. Register in `MainWindow.cs` for lifecycle management

### Adding a New API Endpoint
1. Add URL constant to `Models/Constants.cs`
2. Implement method in `Http/RetroAchievementAPIClient.cs`
3. Create/update model classes and JSON converters

### Modifying Stream Labels
Update `StreamLabelController.cs` methods:
- `WriteFocusStreamLabels()`
- `WriteUserInfoStreamLabels()`
- `WriteGameInfoStreamLabels()`
- `WriteGameProgressStreamLabels()`
- `WriteLastFiveStreamLabels()`
- `WriteAlertsStreamLabels()`

---

## Code Conventions

### Naming
- **Private fields**: `_camelCase` or `CamelCase` (inconsistent - match surrounding code)
- **Properties**: `PascalCase`
- **Methods**: `PascalCase`
- **Constants**: `SCREAMING_SNAKE_CASE`

### Settings Access Pattern
```csharp
public string PropertyName
{
    get => Settings.Default.setting_name;
    set
    {
        Settings.Default.setting_name = value;
        Settings.Default.Save();
        SetAllSettings(); // Refresh UI
    }
}
```

### Async Patterns
- Use `async/await` for API calls and WebView2 operations
- Timer-based polling uses `System.Windows.Forms.Timer`

---

## Dependencies & Prerequisites

### Runtime Requirements
- Windows 10/11
- .NET Framework 4.7.2
- Microsoft Edge WebView2 Runtime

### Development Requirements
- Visual Studio 2019+ with .NET desktop development workload
- WebView2 SDK

---

## Services Layer (Testability)

### AchievementTrackingService (`Services/AchievementTrackingService.cs`)

A standalone service class designed for **testability** - it encapsulates the core polling and achievement detection logic independently of the UI.

#### Purpose
- Extracts business logic from `MainWindow` for unit testing
- Provides event-based notifications for achievement unlocks, game changes, and mastery
- Can be mocked or stubbed in tests

#### Key Features

| Feature | Description |
|---------|-------------|
| Event-driven | Fires events instead of directly updating UI |
| Testable | No dependencies on Windows Forms controls |
| Async polling | `PollAsync()` method for API calls |
| Focus logic | `FindNextFocus()` for achievement navigation |

#### Events

| Event | Args | Description |
|-------|------|-------------|
| `AchievementsUnlocked` | `AchievementsUnlockedEventArgs` | Fired when new achievements are detected |
| `GameChanged` | `GameChangedEventArgs` | Fired when player switches to a different game |
| `GameMastered` | `GameMasteredEventArgs` | Fired when all achievements are unlocked |
| `UserInfoUpdated` | `UserInfoUpdatedEventArgs` | Fired when user rank/points change |
| `PollingStatusChanged` | `PollingStatusEventArgs` | Fired with status messages for UI display |

#### Usage Example

```csharp
// Create service
var trackingService = new AchievementTrackingService(username, apiKey);

// Subscribe to events
trackingService.AchievementsUnlocked += (s, e) => 
{
    foreach (var achievement in e.Achievements)
    {
        Console.WriteLine($"Unlocked: {achievement.Title}");
    }
};

trackingService.GameMastered += (s, e) =>
{
    Console.WriteLine($"Mastered: {e.Game.Title}!");
};

// Poll for updates
var result = await trackingService.PollAsync();
if (result.Success && result.TriggeredNotifications)
{
    // Handle notifications
}
```

#### PollingResult Class

```csharp
public class PollingResult
{
    public bool Success { get; set; }           // Poll completed without errors
    public bool UserUpdated { get; set; }       // User info was refreshed
    public bool GameUpdated { get; set; }       // Game info was refreshed
    public bool TriggeredNotifications { get; set; } // New achievements detected
    public string ErrorMessage { get; set; }    // Error details if failed
}
```

---

## Refactoring Guide (Testability & Readability)

### Current State
`MainWindow.cs` is a large (~2500+ lines) monolithic class handling:
- API polling logic
- Achievement tracking/detection
- UI event handlers (many)
- Settings loading/saving
- LaunchBox integration

### Recommended Refactoring Strategy

#### 1. Use `AchievementTrackingService` for Core Logic
Move polling and detection logic out of `MainWindow`:

```csharp
// Instead of inline polling logic in MainWindow
private AchievementTrackingService _trackingService;

private void StartButton_Click(object sender, EventArgs e)
{
    _trackingService = new AchievementTrackingService(Username, WebAPIKey);
    _trackingService.AchievementsUnlocked += OnAchievementsUnlocked;
    _trackingService.GameChanged += OnGameChanged;
    // Start timer that calls _trackingService.PollAsync()
}
```

#### 2. Split into Partial Classes (Readability)
Organize `MainWindow` using partial classes by feature:

```
Forms/
├── MainWindow.cs              # Core lifecycle, fields, coordination
├── MainWindow.Designer.cs     # (existing - auto-generated)
├── MainWindow.Focus.cs        # Focus overlay handlers & logic
├── MainWindow.Alerts.cs       # Alerts overlay handlers & logic
├── MainWindow.UserInfo.cs     # User info settings
├── MainWindow.GameInfo.cs     # Game info settings
├── MainWindow.GameProgress.cs # Progress settings
├── MainWindow.RecentUnlocks.cs# Recent unlocks settings
├── MainWindow.RelatedMedia.cs # LaunchBox integration (~300 lines)
```

#### 3. Extract Large Methods
`UpdateLaunchBoxReferences()` is ~300 lines and should be extracted to a helper class:

```csharp
// Services/LaunchBoxIntegrationService.cs
public class LaunchBoxIntegrationService
{
    public Dictionary<string, string> FindGameMedia(string launchBoxPath, GameInfo game);
}
```

### Benefits of Refactoring

| Benefit | Description |
|---------|-------------|
| **Testability** | Core logic can be unit tested without UI |
| **Readability** | Smaller files, focused on single responsibility |
| **Maintainability** | Changes to one feature don't risk breaking others |
| **Team collaboration** | Multiple developers can work on different partial classes |

---

## Known Patterns & Anti-Patterns

### Patterns to Follow
- Singleton controllers with `Instance` property
- Settings auto-save on property set
- WebView2 JavaScript interop for UI updates
- Async API calls with proper error handling
- **Use `AchievementTrackingService` for testable business logic**

### Areas for Improvement
- Large `MainWindow.cs` could be refactored into partial classes
- Some code duplication in controller settings (consider base class)
- Mixed use of `Settings.Default` direct access and properties
- `UpdateLaunchBoxReferences()` should be extracted to a service

---

## Troubleshooting

### Common Issues
1. **WebView2 not loading**: Ensure Microsoft Edge WebView2 Runtime is installed
2. **API errors**: Verify API key is valid at retroachievements.org
3. **Stream labels not updating**: Check write permissions in application directory

### Logging
Console output via `Console.WriteLine()` - consider adding structured logging for production debugging.

# Retro Achievement Tracker - WPF (.NET 8)

This is the modern WPF implementation of Retro Achievement Tracker using native XAML rendering instead of WebView2/HTML/JavaScript.

## Features

- ? All 8 overlay windows fully implemented with MVVM pattern
- ? V2 API integration with automatic V1 fallback
- ? Multi-set achievement support (Core + Bonus sets)
- ? Stream label file generation for OBS integration
- ? JSON-based settings persistence
- ? Position Mode for easy overlay positioning

## Running the Application

1. Set `Retro Achievement Tracker.WPF` as the startup project
2. Press F5 to run
3. Enter your RetroAchievements username and API key
4. Click **Start** to begin polling

## Overlay Windows

| Overlay | Description |
|---------|-------------|
| Focus | Currently focused achievement with animations |
| Alerts | Achievement unlock and mastery notifications |
| User Info | User profile, rank, points |
| Game Info | Game metadata (title, console, developer) |
| Game Progress | Achievement and point completion |
| Recent Unlocks | Last 5 unlocked achievements with timestamps |
| Achievement List | Full achievement list (locked/unlocked) |
| Related Media | Game box art and screenshots |

## Native WPF Animations

Overlays use WPF Storyboards with various easing functions:

- **Badge**: Slides in from the left with a BackEase bounce effect
- **Title**: Slides in from the right with CubicEase
- **Line**: Slides in from the right (delayed)
- **Points**: Slides in from the left
- **Description**: Slides in from the right (most delayed)

### Outlined Text Effect

The `OutlinedTextBlock` custom control recreates the CSS `text-stroke` effect by:
1. Rendering the text 8 times at offset positions (stroke layer)
2. Rendering the main text on top

### MVVM Pattern

- `ViewModelBase` - INotifyPropertyChanged base class
- `RelayCommand` - ICommand implementation
- Each overlay has its own ViewModel (e.g., `FocusViewModel`)
- Data binding connects Views to ViewModels

## Project Structure

```
Retro Achievement Tracker.WPF/
??? Controls/
?   ??? OutlinedTextBlock.xaml(.cs)  # Custom text control with stroke
??? Converters/
?   ??? EnumToBoolConverter.cs       # XAML value converters
??? Http/V2/
?   ??? JsonApi/                     # JSON:API document parsing
?   ??? Mappers/                     # V2 resource mappers
?   ??? V2Client.cs                  # V2 API client
?   ??? V2QueryBuilder.cs            # Query parameter builder
??? Models/
?   ??? Achievement.cs               # Achievement model
?   ??? AchievementSet.cs            # Multi-set support
?   ??? AppSettings.cs               # JSON settings model
?   ??? GameInfo.cs                  # Game metadata
??? Services/
?   ??? AchievementTrackingService.cs  # Polling and unlock detection
?   ??? HybridProgressService.cs       # V1/V2 hybrid with fallback
?   ??? IProgressService.cs            # Progress abstraction
?   ??? ServiceFactory.cs              # Service creation with feature flags
?   ??? SettingsService.cs             # JSON settings persistence
?   ??? StreamLabelService.cs          # OBS text file generation
?   ??? V2ProgressService.cs           # V2 API implementation
??? ViewModels/
?   ??? MainViewModel.cs              # Main control panel
?   ??? FocusViewModel.cs             # Focus overlay
?   ??? AlertsViewModel.cs            # Alerts overlay
?   ??? ...                           # Other overlay ViewModels
??? Views/
?   ??? FocusOverlay.xaml(.cs)        # Focus overlay window
?   ??? AlertsOverlay.xaml(.cs)       # Alerts overlay window
?   ??? ...                           # Other overlay windows
??? MainWindow.xaml(.cs)              # Main control panel
??? App.xaml(.cs)                     # Application resources
```

## Benefits Over WebView2

| Aspect | WebView2 (Legacy) | WPF Native (Current) |
|--------|-------------------|----------------------|
| Memory | ~150-300MB per window | ~30-50MB total |
| Startup | 2-4 seconds | <1 second |
| Animation FPS | Good (JS) | Excellent (GPU) |
| Transparency | Workarounds needed | Native support |
| Languages | HTML+CSS+JS+C# | XAML+C# only |

## API Integration

The application uses a hybrid V1/V2 API approach:

- **V2 API**: Used for metadata (systems, games, users) and progress
- **V1 Fallback**: Automatically falls back to V1 if V2 fails
- **Feature Flags**: Control API version selection via `IFeatureFlagService`

## Multi-Set Support

Games with multiple achievement sets (Core, Bonus, etc.) are fully supported:

- Achievement set selector in the main UI
- Per-set progress tracking
- Stream labels include set name when applicable
- Set completion notifications

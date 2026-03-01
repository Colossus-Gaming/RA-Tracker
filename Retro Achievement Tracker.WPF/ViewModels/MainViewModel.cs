using System.Collections.ObjectModel;
using System.Windows.Input;
using RATracker.Models;
using RATracker.WPF.Models;
using RATracker.WPF.Services;

namespace RATracker.WPF.ViewModels;

/// <summary>
/// Represents a timezone for display in the UI.
/// </summary>
public class TimezoneItem
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public TimeSpan BaseUtcOffset { get; set; }

    public override string ToString() => DisplayName;
}

/// <summary>
/// Main ViewModel for the application control panel.
/// Manages API polling, achievement tracking, and overlay window coordination.
/// Uses AchievementTrackingService for polling and unlock detection.
/// Settings are persisted via SettingsService.
/// </summary>
public class MainViewModel : ViewModelBase
{
    #region Private Fields

    private string _username = string.Empty;
    private string _apiKey = string.Empty;
    private string _password = string.Empty;
    private bool _rememberPassword;
    private bool _isSessionAuthenticated;
    private string _sessionStatus = "Not connected";
    private string _statusMessage = "Enter your RetroAchievements credentials to begin.";
    private string _statusIcon = "?";
    private bool _isPolling;
    private bool _canStart;
    private int _pollingCountdown;

    private UserSummary? _userSummary;
    private GameInfo? _currentGame;
    private Achievement? _currentFocusAchievement;
    private int _currentFocusIndex = -1;
    private RefocusBehaviorEnum _refocusBehavior = RefocusBehaviorEnum.GO_TO_NEXT;
    private AchievementSet? _selectedAchievementSet;

    private System.Windows.Threading.DispatcherTimer? _pollingTimer;
    private ServiceFactory? _serviceFactory;
    private AchievementTrackingService? _trackingService;
    private readonly SettingsService _settingsService;
    private readonly StreamLabelService _streamLabelService;

    // Auto-launch settings (backed by settings service)
    private bool _autoStart;
    private bool _autoLaunchFocus = true;
    private bool _autoLaunchAlerts = true;
    private bool _autoLaunchUserInfo;
    private bool _autoLaunchGameInfo;
    private bool _autoLaunchGameProgress;
    private bool _autoLaunchRecentUnlocks;
    private bool _autoLaunchAchievementList;
    private bool _autoLaunchRelatedMedia;

    // Feature flags
    private bool _enableApiLogging;
    private bool _enableStreamLabels;
    private bool _positionModeEnabled;

    // Recent Unlocks timezone settings
    private bool _recentUnlocksAutoDetectTimezone = true;
    private TimezoneItem? _selectedTimezone;

    // Track whether we're loading settings to avoid save loops
    private bool _isLoadingSettings;

    #endregion

    #region Constructor

    public MainViewModel()
    {
        // Initialize settings service
        _settingsService = SettingsService.Instance;

        // Initialize stream label service
        _streamLabelService = StreamLabelService.Instance;

        // Initialize commands
        StartCommand = new RelayCommand(Start, () => CanStart && !IsPolling);
        StopCommand = new RelayCommand(Stop, () => IsPolling);
        TogglePollingCommand = new RelayCommand(TogglePolling, () => CanStart || IsPolling);
        OpenFocusOverlayCommand = new RelayCommand(OpenFocusOverlay);
        OpenAlertsOverlayCommand = new RelayCommand(OpenAlertsOverlay);
        OpenUserInfoOverlayCommand = new RelayCommand(OpenUserInfoOverlay);
        OpenGameInfoOverlayCommand = new RelayCommand(OpenGameInfoOverlay);
        OpenGameProgressOverlayCommand = new RelayCommand(OpenGameProgressOverlay);
        OpenRecentUnlocksOverlayCommand = new RelayCommand(OpenRecentUnlocksOverlay);
        OpenAchievementListOverlayCommand = new RelayCommand(OpenAchievementListOverlay);
        OpenRelatedMediaOverlayCommand = new RelayCommand(OpenRelatedMediaOverlay);

        PreviousFocusCommand = new RelayCommand(PreviousFocus, () => CanNavigateFocus);
        NextFocusCommand = new RelayCommand(NextFocus, () => CanNavigateFocus);
        SetFocusCommand = new RelayCommand(SetFocus, () => CurrentFocusAchievement != null);

        TestAchievementAlertCommand = new RelayCommand(TestAchievementAlert);
        TestMasteryAlertCommand = new RelayCommand(TestMasteryAlert);
        TestSubsetAchievementAlertCommand = new RelayCommand(TestSubsetAchievementAlert);
        TestSubsetMasteryAlertCommand = new RelayCommand(TestSubsetMasteryAlert);

        // Initialize collections
        LockedAchievements = new ObservableCollection<Achievement>();
        UnlockedAchievements = new ObservableCollection<Achievement>();
        RecentUnlocks = new ObservableCollection<Achievement>();
        AvailableAchievementSets = new ObservableCollection<AchievementSet>();
        AvailableTimezones = new ObservableCollection<TimezoneItem>();

        // Initialize available timezones
        InitializeTimezones();

        // Load settings
        LoadSettingsFromService();

        // Load sample data for design time preview and as placeholder at runtime
        // This provides placeholder images until real data is loaded from the API
        LoadSampleData();
    }

    /// <summary>
    /// Gets whether the application is running in design mode (XAML designer).
    /// </summary>
    private static bool IsInDesignMode =>
        System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());

    /// <summary>
    /// Loads all settings from the SettingsService into the ViewModel properties.
    /// </summary>
    private void LoadSettingsFromService()
    {
        _isLoadingSettings = true;

        try
        {
            var settings = _settingsService.Settings;

            // Credentials
            _username = settings.Username;
            _apiKey = _settingsService.GetApiKey();
            _password = _settingsService.GetPassword();
            _rememberPassword = settings.RememberPassword;

            // Auto-launch settings
            _autoStart = settings.AutoStart;
            _autoLaunchFocus = settings.AutoLaunchFocus;
            _autoLaunchAlerts = settings.AutoLaunchAlerts;
            _autoLaunchUserInfo = settings.AutoLaunchUserInfo;
            _autoLaunchGameInfo = settings.AutoLaunchGameInfo;
            _autoLaunchGameProgress = settings.AutoLaunchGameProgress;
            _autoLaunchRecentUnlocks = settings.AutoLaunchRecentUnlocks;
            _autoLaunchAchievementList = settings.AutoLaunchAchievementList;
            _autoLaunchRelatedMedia = settings.AutoLaunchRelatedMedia;

            // Feature flags
            _enableApiLogging = settings.EnableApiLogging;
            _enableStreamLabels = settings.EnableStreamLabels;
            _positionModeEnabled = settings.PositionModeEnabled;

            // Configure stream label service
            _streamLabelService.IsEnabled = _enableStreamLabels;

            // Refocus behavior
            if (Enum.TryParse<RefocusBehaviorEnum>(settings.RefocusBehavior, out var behavior))
            {
                _refocusBehavior = behavior;
            }

            // Recent Unlocks timezone settings
            _recentUnlocksAutoDetectTimezone = settings.RecentUnlocksAutoDetectTimezone;
            if (!string.IsNullOrEmpty(settings.RecentUnlocksTimezoneId))
            {
                _selectedTimezone = AvailableTimezones.FirstOrDefault(t => t.Id == settings.RecentUnlocksTimezoneId);
            }
            // If no timezone saved or not found, default to local timezone
            _selectedTimezone ??= AvailableTimezones.FirstOrDefault(t => t.Id == TimeZoneInfo.Local.Id)
                               ?? AvailableTimezones.FirstOrDefault();

            // Update CanStart based on loaded credentials
            UpdateCanStart();

            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Settings loaded - Username: {_username}, AutoStart: {_autoStart}");
        }
        finally
        {
            _isLoadingSettings = false;
        }
    }

    #endregion

    #region Commands

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand TogglePollingCommand { get; }
    public ICommand OpenFocusOverlayCommand { get; }
    public ICommand OpenAlertsOverlayCommand { get; }
    public ICommand OpenUserInfoOverlayCommand { get; }
    public ICommand OpenGameInfoOverlayCommand { get; }
    public ICommand OpenGameProgressOverlayCommand { get; }
    public ICommand OpenRecentUnlocksOverlayCommand { get; }
    public ICommand OpenAchievementListOverlayCommand { get; }
    public ICommand OpenRelatedMediaOverlayCommand { get; }
    public ICommand PreviousFocusCommand { get; }
    public ICommand NextFocusCommand { get; }
    public ICommand SetFocusCommand { get; }
    public ICommand TestAchievementAlertCommand { get; }
    public ICommand TestMasteryAlertCommand { get; }
    public ICommand TestSubsetAchievementAlertCommand { get; }
    public ICommand TestSubsetMasteryAlertCommand { get; }

    #endregion

    #region Credential Properties

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                UpdateCanStart();
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.Username = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public string ApiKey
    {
        get => _apiKey;
        set
        {
            if (SetProperty(ref _apiKey, value))
            {
                UpdateCanStart();
                SaveSettingIfNotLoading(() => _settingsService.SetApiKey(value));
            }
        }
    }

    /// <summary>
    /// The user's password (transient in memory, persisted via SettingsService if RememberPassword is true).
    /// </summary>
    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                UpdateCanStart();
                SaveSettingIfNotLoading(() =>
                    _settingsService.SetPassword(value, _rememberPassword));
            }
        }
    }

    /// <summary>
    /// Whether to persist the password between sessions.
    /// </summary>
    public bool RememberPassword
    {
        get => _rememberPassword;
        set
        {
            if (SetProperty(ref _rememberPassword, value))
            {
                SaveSettingIfNotLoading(() =>
                    _settingsService.SetPassword(_password, value));
            }
        }
    }

    /// <summary>
    /// Whether we have an active browser session for V2 API.
    /// </summary>
    public bool IsSessionAuthenticated
    {
        get => _isSessionAuthenticated;
        set => SetProperty(ref _isSessionAuthenticated, value);
    }

    /// <summary>
    /// Human-readable session status.
    /// </summary>
    public string SessionStatus
    {
        get => _sessionStatus;
        set => SetProperty(ref _sessionStatus, value);
    }

    /// <summary>
    /// Text showing which API version is active (e.g., "V2 API active" or "V1 fallback").
    /// </summary>
    public string ApiStatusText
    {
        get => _apiStatusText;
        set
        {
            if (SetProperty(ref _apiStatusText, value))
                OnPropertyChanged(nameof(HasApiStatus));
        }
    }
    private string _apiStatusText = string.Empty;

    /// <summary>
    /// Whether to show the API status indicator.
    /// </summary>
    public bool HasApiStatus => !string.IsNullOrEmpty(ApiStatusText);

    /// <summary>
    /// Whether the app is currently using V1 as a fallback (V2 failed).
    /// </summary>
    public bool IsUsingV1Fallback
    {
        get => _isUsingV1Fallback;
        set => SetProperty(ref _isUsingV1Fallback, value);
    }
    private bool _isUsingV1Fallback;

    /// <summary>
    /// Helper method to save settings only when not in the loading phase.
    /// </summary>
    private void SaveSettingIfNotLoading(Action saveAction)
    {
        if (!_isLoadingSettings)
        {
            saveAction();
        }
    }

    #endregion

    #region Status Properties

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string StatusIcon
    {
        get => _statusIcon;
        set => SetProperty(ref _statusIcon, value);
    }

    public bool IsPolling
    {
        get => _isPolling;
        private set
        {
            if (SetProperty(ref _isPolling, value))
            {
                OnPropertyChanged(nameof(IsNotPolling));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsNotPolling => !IsPolling;

    public bool CanStart
    {
        get => _canStart;
        private set
        {
            if (SetProperty(ref _canStart, value))
                CommandManager.InvalidateRequerySuggested();
        }
    }

    public int PollingCountdown
    {
        get => _pollingCountdown;
        set => SetProperty(ref _pollingCountdown, value);
    }

    #endregion

    #region User Info Properties

    public UserSummary? UserSummary
    {
        get => _userSummary;
        set
        {
            if (SetProperty(ref _userSummary, value))
            {
                OnPropertiesChanged(
                    nameof(UserDisplayName),
                    nameof(UserMotto),
                    nameof(UserRank),
                    nameof(UserPoints),
                    nameof(UserTruePoints),
                    nameof(UserRatio),
                    nameof(UserProfilePicture),
                    nameof(HasUserInfo));
            }
        }
    }

    public string UserDisplayName => UserSummary?.UserName ?? "Not logged in";
    public string UserMotto => UserSummary?.Motto ?? string.Empty;
    public string UserRank => UserSummary?.Rank > 0 ? $"#{UserSummary.Rank}" : "No Rank";
    public string UserPoints => $"{UserSummary?.TotalPoints ?? 0:N0}";
    public string UserTruePoints => $"({UserSummary?.TotalTruePoints ?? 0:N0})";
    public string UserRatio => UserSummary?.RetroRatio ?? "0.00";
    public string UserProfilePicture => UserSummary != null
        ? $"https://retroachievements.org/UserPic/{UserSummary.UserName}.png"
        : string.Empty;
    public bool HasUserInfo => UserSummary != null;

    #endregion

    #region Game Info Properties

    public GameInfo? CurrentGame
    {
        get => _currentGame;
        set
        {
            if (SetProperty(ref _currentGame, value))
            {
                UpdateAchievementLists();
                OnPropertiesChanged(
                    nameof(GameTitle),
                    nameof(GameConsole),
                    nameof(GameDeveloper),
                    nameof(GamePublisher),
                    nameof(GameGenre),
                    nameof(GameReleased),
                    nameof(GameBadgeUri),
                    nameof(GameAchievementsEarned),
                    nameof(GameAchievementsTotal),
                    nameof(GamePointsEarned),
                    nameof(GamePointsTotal),
                    nameof(GameCompletionPercent),
                    nameof(GameCompletionText),
                    nameof(IsMastered),
                    nameof(HasGameInfo));
            }
        }
    }

    public string GameTitle => CurrentGame?.Title ?? "No game loaded";
    public string GameConsole => CurrentGame?.ConsoleName ?? string.Empty;
    public string GameDeveloper => CurrentGame?.Developer ?? "Unknown";
    public string GamePublisher => CurrentGame?.Publisher ?? "Unknown";
    public string GameGenre => CurrentGame?.Genre ?? "Unknown";
    public string GameReleased => CurrentGame?.Released ?? "Unknown";
    public string GameBadgeUri => CurrentGame?.BadgeUri ?? string.Empty;
    public int GameAchievementsEarned => CurrentGame?.AchievementsEarned ?? 0;
    public int GameAchievementsTotal => CurrentGame?.Achievements?.Count ?? 0;
    public int GamePointsEarned => CurrentGame?.GamePointsEarned ?? 0;
    public int GamePointsTotal => CurrentGame?.GamePointsPossible ?? 0;
    public double GameCompletionPercent => GameAchievementsTotal > 0
        ? (double)GameAchievementsEarned / GameAchievementsTotal * 100
        : 0;
    public string GameCompletionText => IsMastered ? "MASTERED!" : $"{GameCompletionPercent:F1}%";
    public bool IsMastered => GameAchievementsTotal > 0 && GameAchievementsEarned >= GameAchievementsTotal;
    public bool HasGameInfo => CurrentGame != null;

    #endregion

    #region Achievement Collections

    public ObservableCollection<Achievement> LockedAchievements { get; }
    public ObservableCollection<Achievement> UnlockedAchievements { get; }
    public ObservableCollection<Achievement> RecentUnlocks { get; }

    #endregion

    #region Achievement Set Selection

    /// <summary>
    /// Gets the collection of available achievement sets for the current game.
    /// Only populated when the game has multiple sets.
    /// </summary>
    public ObservableCollection<AchievementSet> AvailableAchievementSets { get; }

    /// <summary>
    /// Gets whether the current game has multiple achievement sets.
    /// </summary>
    public bool HasMultipleSets => CurrentGame?.HasMultipleSets ?? false;

    /// <summary>
    /// Gets the name of the currently selected achievement set, or null if only one set.
    /// </summary>
    public string? SelectedSetName => SelectedAchievementSet?.Name;

    /// <summary>
    /// Gets or sets the currently selected achievement set.
    /// When changed, updates the game's SelectedSet and refreshes achievement lists.
    /// </summary>
    public AchievementSet? SelectedAchievementSet
    {
        get => _selectedAchievementSet;
        set
        {
            if (SetProperty(ref _selectedAchievementSet, value))
            {
                // Update the game's selected set
                if (CurrentGame != null && value != null)
                {
                    CurrentGame.SelectedSet = value;
                }

                // Refresh achievement lists to show achievements from the new set
                RefreshAchievementListsForSelectedSet();

                // Notify UI of changes
                OnPropertiesChanged(
                    nameof(SelectedSetName),
                    nameof(GameAchievementsEarned),
                    nameof(GameAchievementsTotal),
                    nameof(GamePointsEarned),
                    nameof(GamePointsTotal),
                    nameof(GameCompletionPercent),
                    nameof(GameCompletionText),
                    nameof(IsMastered));

                // Save the selected set to settings
                SaveSelectedSetToSettings(value);

                System.Diagnostics.Debug.WriteLine($"[MainViewModel] Selected achievement set changed to: {value?.Name ?? "None"}");
            }
        }
    }

    /// <summary>
    /// Updates the available achievement sets collection when the game changes.
    /// </summary>
    private void UpdateAvailableAchievementSets()
    {
        AvailableAchievementSets.Clear();
        _selectedAchievementSet = null;

        if (CurrentGame == null || !CurrentGame.HasMultipleSets)
        {
            OnPropertiesChanged(nameof(HasMultipleSets), nameof(SelectedAchievementSet), nameof(SelectedSetName));
            return;
        }

        // Populate available sets
        foreach (var set in CurrentGame.AchievementSets)
        {
            AvailableAchievementSets.Add(set);
        }

        // Try to restore the previously selected set from settings
        if (!TryRestoreSavedSetSelection())
        {
            // Select the active set (core by default, or the game's current selection)
            _selectedAchievementSet = CurrentGame.ActiveSet ?? CurrentGame.CoreSet ?? CurrentGame.AchievementSets.FirstOrDefault();
        }

        OnPropertiesChanged(nameof(HasMultipleSets), nameof(SelectedAchievementSet), nameof(SelectedSetName));

        System.Diagnostics.Debug.WriteLine($"[MainViewModel] Updated available sets: {AvailableAchievementSets.Count} sets, selected: {_selectedAchievementSet?.Name}");
    }

    /// <summary>
    /// Refreshes the locked/unlocked achievement lists based on the selected set.
    /// </summary>
    private void RefreshAchievementListsForSelectedSet()
    {
        LockedAchievements.Clear();
        UnlockedAchievements.Clear();
        RecentUnlocks.Clear();

        // Get achievements from the selected set (or all achievements if no multi-set)
        var achievements = GetAchievementsForCurrentSet();
        if (achievements == null || achievements.Count == 0) return;

        var locked = achievements.Where(a => !a.DateEarned.HasValue).ToList();
        var unlocked = achievements.Where(a => a.DateEarned.HasValue)
            .OrderByDescending(a => a.DateEarned).ToList();

        foreach (var a in locked) LockedAchievements.Add(a);
        foreach (var a in unlocked) UnlockedAchievements.Add(a);

        // Recent unlocks (last 5)
        foreach (var a in unlocked.Take(5)) RecentUnlocks.Add(a);

        // Reset focus to first locked achievement in the new set
        // Note: We must directly set CurrentFocusAchievement because if the index
        // hasn't changed (e.g., both sets have index 0 as first locked), the
        // SetProperty check would return false and not update the achievement
        if (LockedAchievements.Count > 0)
        {
            _currentFocusIndex = 0;
            CurrentFocusAchievement = LockedAchievements[0];
            FocusChanged?.Invoke(this, CurrentFocusAchievement!);
        }
        else
        {
            CurrentFocusAchievement = null;
        }

        OnPropertyChanged(nameof(CanNavigateFocus));
    }

    /// <summary>
    /// Gets the achievements for the currently selected set, or all achievements if no multi-set support.
    /// </summary>
    private List<Achievement>? GetAchievementsForCurrentSet()
    {
        if (CurrentGame == null) return null;

        // If the game has multiple sets and we have a selection, use that set's achievements
        if (CurrentGame.HasMultipleSets && SelectedAchievementSet != null)
        {
            return SelectedAchievementSet.Achievements;
        }

        // Otherwise, use the game's default achievements (which comes from ActiveSet or direct list)
        return CurrentGame.Achievements;
    }

    /// <summary>
    /// Saves the current game ID and selected set to settings.
    /// </summary>
    private void SaveGameAndSetToSettings()
    {
        if (CurrentGame == null) return;

        SaveSettingIfNotLoading(() =>
        {
            _settingsService.Settings.LastPlayedGameId = CurrentGame.Id;
            if (SelectedAchievementSet != null)
            {
                _settingsService.Settings.LastSelectedSetId = SelectedAchievementSet.Id;
                _settingsService.Settings.LastSelectedSetName = SelectedAchievementSet.Name;
            }
            _settingsService.ScheduleSave();
        });

        System.Diagnostics.Debug.WriteLine($"[MainViewModel] Saved game/set to settings - GameId: {CurrentGame.Id}, SetId: {SelectedAchievementSet?.Id}, SetName: {SelectedAchievementSet?.Name}");
    }

    /// <summary>
    /// Saves the selected set to settings when it changes.
    /// </summary>
    private void SaveSelectedSetToSettings(AchievementSet? set)
    {
        if (set == null) return;

        SaveSettingIfNotLoading(() =>
        {
            _settingsService.Settings.LastSelectedSetId = set.Id;
            _settingsService.Settings.LastSelectedSetName = set.Name;
            _settingsService.ScheduleSave();
        });
    }

    /// <summary>
    /// Tries to restore the previously selected set for a game.
    /// Returns true if a saved set was restored.
    /// </summary>
    private bool TryRestoreSavedSetSelection()
    {
        if (CurrentGame == null || !CurrentGame.HasMultipleSets) return false;

        var settings = _settingsService.Settings;

        // Only restore if this is the same game we had saved
        if (settings.LastPlayedGameId != CurrentGame.Id) return false;

        // Try to find the set by ID first
        if (settings.LastSelectedSetId > 0)
        {
            var setById = AvailableAchievementSets.FirstOrDefault(s => s.Id == settings.LastSelectedSetId);
            if (setById != null)
            {
                _selectedAchievementSet = setById;
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] Restored set selection by ID: {setById.Name}");
                return true;
            }
        }

        // Fallback to finding by name
        if (!string.IsNullOrEmpty(settings.LastSelectedSetName))
        {
            var setByName = AvailableAchievementSets.FirstOrDefault(s => 
                s.Name.Equals(settings.LastSelectedSetName, StringComparison.OrdinalIgnoreCase));
            if (setByName != null)
            {
                _selectedAchievementSet = setByName;
                System.Diagnostics.Debug.WriteLine($"[MainViewModel] Restored set selection by name: {setByName.Name}");
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Focus Properties

    public Achievement? CurrentFocusAchievement
    {
        get => _currentFocusAchievement;
        set
        {
            if (SetProperty(ref _currentFocusAchievement, value))
            {
                OnPropertiesChanged(
                    nameof(FocusTitle),
                    nameof(FocusDescription),
                    nameof(FocusPoints),
                    nameof(FocusBadgeUri),
                    nameof(HasFocusAchievement));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string FocusTitle => CurrentFocusAchievement?.Title ?? "No achievement selected";
    public string FocusDescription => CurrentFocusAchievement?.Description ?? string.Empty;
    public string FocusPoints => CurrentFocusAchievement?.Points.ToString() ?? "0";
    public string FocusBadgeUri => CurrentFocusAchievement?.BadgeUri ?? string.Empty;
    public bool HasFocusAchievement => CurrentFocusAchievement != null;
    public bool CanNavigateFocus => LockedAchievements.Count > 1;

    public int CurrentFocusIndex
    {
        get => _currentFocusIndex;
        set
        {
            if (SetProperty(ref _currentFocusIndex, value))
            {
                if (value >= 0 && value < LockedAchievements.Count)
                    CurrentFocusAchievement = LockedAchievements[value];
            }
        }
    }

    #endregion

    #region Auto-Launch Settings

    public bool AutoStart
    {
        get => _autoStart;
        set
        {
            if (SetProperty(ref _autoStart, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.AutoStart = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool AutoLaunchFocus
    {
        get => _autoLaunchFocus;
        set
        {
            if (SetProperty(ref _autoLaunchFocus, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.AutoLaunchFocus = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool AutoLaunchAlerts
    {
        get => _autoLaunchAlerts;
        set
        {
            if (SetProperty(ref _autoLaunchAlerts, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.AutoLaunchAlerts = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool AutoLaunchUserInfo
    {
        get => _autoLaunchUserInfo;
        set
        {
            if (SetProperty(ref _autoLaunchUserInfo, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.AutoLaunchUserInfo = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool AutoLaunchGameInfo
    {
        get => _autoLaunchGameInfo;
        set
        {
            if (SetProperty(ref _autoLaunchGameInfo, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.AutoLaunchGameInfo = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool AutoLaunchGameProgress
    {
        get => _autoLaunchGameProgress;
        set
        {
            if (SetProperty(ref _autoLaunchGameProgress, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.AutoLaunchGameProgress = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool AutoLaunchRecentUnlocks
    {
        get => _autoLaunchRecentUnlocks;
        set
        {
            if (SetProperty(ref _autoLaunchRecentUnlocks, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.AutoLaunchRecentUnlocks = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool AutoLaunchAchievementList
    {
        get => _autoLaunchAchievementList;
        set
        {
            if (SetProperty(ref _autoLaunchAchievementList, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.AutoLaunchAchievementList = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool AutoLaunchRelatedMedia
    {
        get => _autoLaunchRelatedMedia;
        set
        {
            if (SetProperty(ref _autoLaunchRelatedMedia, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.AutoLaunchRelatedMedia = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to enable API request logging.
    /// </summary>
    public bool EnableApiLogging
    {
        get => _enableApiLogging;
        set
        {
            if (SetProperty(ref _enableApiLogging, value))
            {
                // Recreate service factory if already initialized
                if (_serviceFactory != null && !IsPolling)
                {
                    RecreateServiceFactory();
                }

                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.EnableApiLogging = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    /// <summary>
    /// Gets or sets whether stream label file generation is enabled.
    /// </summary>
    public bool EnableStreamLabels
    {
        get => _enableStreamLabels;
        set
        {
            if (SetProperty(ref _enableStreamLabels, value))
            {
                // Update stream label service
                _streamLabelService.IsEnabled = value;

                // If enabling, write current state immediately
                if (value)
                {
                    WriteAllStreamLabels();
                }

                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.EnableStreamLabels = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    /// <summary>
    /// Gets or sets the refocus behavior when the current focus is unlocked.
    /// </summary>
    public RefocusBehaviorEnum RefocusBehavior
    {
        get => _refocusBehavior;
        set
        {
            if (SetProperty(ref _refocusBehavior, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.RefocusBehavior = value.ToString();
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    /// <summary>
    /// Gets or sets whether Position Mode is enabled for overlay windows.
    /// When enabled, overlays show positioning guides and can be dragged.
    /// When disabled, overlays are fully transparent for OBS capture.
    /// </summary>
    public bool PositionModeEnabled
    {
        get => _positionModeEnabled;
        set
        {
            if (SetProperty(ref _positionModeEnabled, value))
            {
                // Notify all open overlay windows
                PositionModeChanged?.Invoke(this, value);

                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.PositionModeEnabled = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    #endregion

    #region Recent Unlocks Timezone Settings

    /// <summary>
    /// Gets the collection of available timezones.
    /// </summary>
    public ObservableCollection<TimezoneItem> AvailableTimezones { get; }

    /// <summary>
    /// Gets or sets whether to auto-detect the timezone for Recent Unlocks.
    /// </summary>
    public bool RecentUnlocksAutoDetectTimezone
    {
        get => _recentUnlocksAutoDetectTimezone;
        set
        {
            if (SetProperty(ref _recentUnlocksAutoDetectTimezone, value))
            {
                OnPropertyChanged(nameof(IsTimezoneSelectionEnabled));
                OnPropertyChanged(nameof(EffectiveTimezone));
                
                // Notify the Recent Unlocks overlay about the timezone change
                TimezoneChanged?.Invoke(this, GetEffectiveTimezone());

                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.RecentUnlocksAutoDetectTimezone = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected timezone for Recent Unlocks.
    /// </summary>
    public TimezoneItem? SelectedTimezone
    {
        get => _selectedTimezone;
        set
        {
            if (SetProperty(ref _selectedTimezone, value))
            {
                OnPropertyChanged(nameof(EffectiveTimezone));
                
                // Notify the Recent Unlocks overlay about the timezone change
                TimezoneChanged?.Invoke(this, GetEffectiveTimezone());

                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.RecentUnlocksTimezoneId = value?.Id ?? string.Empty;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    /// <summary>
    /// Gets whether the timezone dropdown should be enabled (when auto-detect is off).
    /// </summary>
    public bool IsTimezoneSelectionEnabled => !RecentUnlocksAutoDetectTimezone;

    /// <summary>
    /// Gets a display string for the effective timezone being used.
    /// </summary>
    public string EffectiveTimezone => RecentUnlocksAutoDetectTimezone
        ? $"Auto: {TimeZoneInfo.Local.DisplayName}"
        : SelectedTimezone?.DisplayName ?? "Unknown";

    /// <summary>
    /// Gets the effective TimeZoneInfo based on current settings.
    /// </summary>
    public TimeZoneInfo GetEffectiveTimezone()
    {
        if (RecentUnlocksAutoDetectTimezone)
        {
            return TimeZoneInfo.Local;
        }

        if (SelectedTimezone != null)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(SelectedTimezone.Id);
            }
            catch
            {
                // Fallback to local if the saved timezone is invalid
                return TimeZoneInfo.Local;
            }
        }

        return TimeZoneInfo.Local;
    }

    /// <summary>
    /// Initializes the available timezones collection.
    /// </summary>
    private void InitializeTimezones()
    {
        var timezones = TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => new TimezoneItem
            {
                Id = tz.Id,
                DisplayName = tz.DisplayName,
                BaseUtcOffset = tz.BaseUtcOffset
            })
            .OrderBy(t => t.BaseUtcOffset)
            .ThenBy(t => t.DisplayName);

        foreach (var tz in timezones)
        {
            AvailableTimezones.Add(tz);
        }
    }

    #endregion

    #region Events

    public event EventHandler? FocusOverlayRequested;
    public event EventHandler? AlertsOverlayRequested;
    public event EventHandler? UserInfoOverlayRequested;
    public event EventHandler? GameInfoOverlayRequested;
    public event EventHandler? GameProgressOverlayRequested;
    public event EventHandler? RecentUnlocksOverlayRequested;
    public event EventHandler? AchievementListOverlayRequested;
    public event EventHandler? RelatedMediaOverlayRequested;
    public event EventHandler<Achievement>? AchievementUnlocked;
    public event EventHandler<GameInfo>? GameMastered;
    public event EventHandler<Achievement>? FocusChanged;
    public event EventHandler<TimeZoneInfo>? TimezoneChanged;
    public event EventHandler<bool>? PositionModeChanged;

    #endregion

    #region Sample Data (for Demo)

    private void LoadSampleData()
    {
        // Create sample user
        UserSummary = new UserSummary
        {
            UserName = "DemoUser",
            Motto = "Retro gaming enthusiast!",
            Rank = 1234,
            TotalPoints = 15000,
            TotalTruePoints = 45000
        };

        // Design values for stress-testing long text in UI
        // Game: ~Hack~ Pokemon Emerald Version: Party Randomizer Plus [Subset - Battle Frontier]
        // Achievement: The Absolutely Incredible Master of All Gaming Challenges! (very long description)

        // Create sample achievements for core set - some locked, some unlocked
        var coreAchievements = new List<Achievement>
        {
            new Achievement { Id = 1, Title = "First Steps", Description = "Complete the tutorial", Points = 5, BadgeUri = "https://media.retroachievements.org/Badge/00001.png", DateEarned = DateTime.Now.AddHours(-2) },
            new Achievement { Id = 2, Title = "Explorer", Description = "Discover a hidden area", Points = 10, BadgeUri = "https://media.retroachievements.org/Badge/00002.png", DateEarned = DateTime.Now.AddHours(-1) },
            new Achievement { Id = 999999, Title = "The Absolutely Incredible Master of All Gaming Challenges!", Description = "Defeat the final boss without taking damage, without using healing items, without leveling up past level 10, without equipping any armor or accessories, on Hard difficulty or higher, within 30 minutes of starting the battle, while keeping all party membe", Points = 50, BadgeUri = "https://media.retroachievements.org/Badge/00003.png" },
            new Achievement { Id = 4, Title = "Speed Demon", Description = "Complete level 1 in under 60 seconds", Points = 25, BadgeUri = "https://media.retroachievements.org/Badge/00004.png" },
            new Achievement { Id = 5, Title = "Boss Slayer", Description = "Defeat the first boss", Points = 20, BadgeUri = "https://media.retroachievements.org/Badge/00005.png" },
            new Achievement { Id = 6, Title = "Perfectionist", Description = "Complete a level without taking damage", Points = 50, BadgeUri = "https://media.retroachievements.org/Badge/00006.png" },
        };

        // Create sample achievements for bonus set
        var bonusAchievements = new List<Achievement>
        {
            new Achievement { Id = 101, Title = "Speed Run Master", Description = "Complete the entire game in under 30 minutes", Points = 100, BadgeUri = "https://media.retroachievements.org/Badge/00007.png" },
            new Achievement { Id = 102, Title = "No Death Run", Description = "Complete the game without dying", Points = 150, BadgeUri = "https://media.retroachievements.org/Badge/00008.png" },
            new Achievement { Id = 103, Title = "Secret Hunter", Description = "Find all 10 hidden secrets", Points = 75, BadgeUri = "https://media.retroachievements.org/Badge/00009.png" },
        };

        // Create achievement sets
        var coreSet = new AchievementSet
        {
            Id = 1,
            Name = "Core",
            SetType = AchievementSetType.Core,
            GameId = 99999,
            Achievements = coreAchievements
        };

        var bonusSet = new AchievementSet
        {
            Id = 2,
            Name = "Battle Frontier",
            SetType = AchievementSetType.Bonus,
            GameId = 99999,
            Achievements = bonusAchievements
        };

        // Create sample game with multiple achievement sets using design values
        CurrentGame = new GameInfo
        {
            Id = 99999,
            Title = "~Hack~ Pokemon Emerald Version: Party Randomizer Plus [Subset - Battle Frontier]",
            ConsoleName = "Game Boy Advance",
            Developer = "Game Freak",
            Publisher = "Nintendo",
            Genre = "Role-Playing Game",
            Released = "2004",
            BadgeUri = "https://media.retroachievements.org/Images/000001.png",
            AchievementSets = new List<AchievementSet> { coreSet, bonusSet }
        };

        StatusMessage = "Sample data loaded - ready for demo! (Multi-set game example)";
        StatusIcon = "?";
    }

    #endregion

    #region Command Implementations

    private void UpdateCanStart()
    {
        // Can start with: username + password (session auth) or username + API key (legacy)
        CanStart = !string.IsNullOrWhiteSpace(Username)
            && (!string.IsNullOrWhiteSpace(Password) || !string.IsNullOrWhiteSpace(ApiKey));
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>
    /// Toggles polling on/off. Used by the combined Start/Stop button.
    /// </summary>
    private void TogglePolling()
    {
        if (IsPolling)
        {
            Stop();
        }
        else
        {
            Start();
        }
    }

    /// <summary>
    /// Fires when the ViewModel needs a browser login to establish a session.
    /// MainWindow handles this by showing the LoginWindow.
    /// </summary>
    public event EventHandler? LoginRequired;

    /// <summary>
    /// Called by MainWindow after a successful browser login to continue starting.
    /// </summary>
    public void StartWithSession()
    {
        var session = SessionService.Instance;
        if (!session.IsAuthenticated) return;

        IsSessionAuthenticated = true;
        SessionStatus = session.StatusText;

        var featureFlags = new FeatureFlagService(
            useV2ForMetadata: true,
            useV2ForProgress: true,
            useV2ForUserLookup: true,
            enableApiLogging: EnableApiLogging);

        _serviceFactory = new ServiceFactory(
            Username, ApiKey,
            session.CookieContainer!, session.UserAgent!,
            featureFlags, EnableApiLogging);

        StartPollingWithFactory();
    }

    private void Start()
    {
        if (!CanStart) return;

        var session = SessionService.Instance;

        // If we have a password but no API key or no session, request login
        if (!string.IsNullOrWhiteSpace(Password) && !session.IsAuthenticated)
        {
            LoginRequired?.Invoke(this, EventArgs.Empty);
            return;
        }

        // Create service factory
        var featureFlags = new FeatureFlagService(
            useV2ForMetadata: true,
            useV2ForProgress: true,
            useV2ForUserLookup: true,
            enableApiLogging: EnableApiLogging);

        if (session.IsAuthenticated && session.CookieContainer != null)
        {
            IsSessionAuthenticated = true;
            SessionStatus = session.StatusText;
            _serviceFactory = new ServiceFactory(
                Username, ApiKey,
                session.CookieContainer, session.UserAgent!,
                featureFlags, EnableApiLogging);
        }
        else
        {
            _serviceFactory = new ServiceFactory(Username, ApiKey, featureFlags, EnableApiLogging);
        }

        StartPollingWithFactory();
    }

    private void StartPollingWithFactory()
    {
        _trackingService = _serviceFactory!.GetTrackingService();

        // Subscribe to tracking service events
        _trackingService.AchievementsUnlocked += OnTrackingServiceAchievementsUnlocked;
        _trackingService.GameChanged += OnTrackingServiceGameChanged;
        _trackingService.GameMastered += OnTrackingServiceGameMastered;
        _trackingService.UserInfoUpdated += OnTrackingServiceUserInfoUpdated;
        _trackingService.PollingStatusChanged += OnTrackingServicePollingStatusChanged;

        IsPolling = true;
        StatusIcon = "?";
        StatusMessage = "Polling started";

        // Set API status indicator
        if (_serviceFactory.HasSessionAuth)
        {
            ApiStatusText = "V2 API active";
            IsUsingV1Fallback = false;
        }
        else if (!string.IsNullOrWhiteSpace(ApiKey))
        {
            ApiStatusText = "V1 API (no session)";
            IsUsingV1Fallback = true;
        }
        else
        {
            ApiStatusText = "V1 API";
            IsUsingV1Fallback = true;
        }

        Log($"Started polling — {ApiStatusText}");

        // Start the polling timer
        StartPollingTimer();
    }

    private void RecreateServiceFactory()
    {
        // Unsubscribe from old tracking service
        if (_trackingService != null)
        {
            _trackingService.AchievementsUnlocked -= OnTrackingServiceAchievementsUnlocked;
            _trackingService.GameChanged -= OnTrackingServiceGameChanged;
            _trackingService.GameMastered -= OnTrackingServiceGameMastered;
            _trackingService.UserInfoUpdated -= OnTrackingServiceUserInfoUpdated;
            _trackingService.PollingStatusChanged -= OnTrackingServicePollingStatusChanged;
        }

        _serviceFactory?.Dispose();
        _trackingService = null;

        var hasCredentials = !string.IsNullOrWhiteSpace(Username)
            && (!string.IsNullOrWhiteSpace(ApiKey) || !string.IsNullOrWhiteSpace(Password));
        if (hasCredentials)
        {
            var session = SessionService.Instance;
            var featureFlags = new FeatureFlagService(
                useV2ForMetadata: true,
                useV2ForProgress: true,
                useV2ForUserLookup: true,
                enableApiLogging: EnableApiLogging);

            if (session.IsAuthenticated && session.CookieContainer != null)
            {
                _serviceFactory = new ServiceFactory(
                    Username, ApiKey,
                    session.CookieContainer, session.UserAgent!,
                    featureFlags, EnableApiLogging);
            }
            else
            {
                _serviceFactory = new ServiceFactory(Username, ApiKey, featureFlags, EnableApiLogging);
            }
        }
    }

    private void StartPollingTimer()
    {
        _pollingTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _pollingTimer.Tick += PollingTimer_Tick;
        PollingCountdown = 60; // 30 seconds at 500ms intervals
        _pollingTimer.Start();
    }

    private async void PollingTimer_Tick(object? sender, EventArgs e)
    {
        PollingCountdown--;

        if (PollingCountdown <= 0)
        {
            PollingCountdown = 60; // Reset countdown
            await PollApiAsync();
        }
        else
        {
            StatusMessage = $"Next poll in {PollingCountdown / 2} seconds...";
        }
    }

    private async Task PollApiAsync()
    {
        if (_trackingService == null) return;

        try
        {
            StatusIcon = "??";
            StatusMessage = "Polling RetroAchievements API...";
            Log("Polling API...");

            var result = await _trackingService.PollAsync();

            if (result.Success)
            {
                StatusIcon = "?";
                if (!result.TriggeredNotifications)
                {
                    StatusMessage = "Poll complete - no changes";
                }
                Log($"Poll success — notifications={result.TriggeredNotifications}");

                // Update V1/V2 status based on what actually happened
                if (_serviceFactory?.HasSessionAuth == true)
                {
                    if (ApiStatusText != "V2 API active")
                    {
                        ApiStatusText = "V2 API active";
                        IsUsingV1Fallback = false;
                    }
                }
            }
            else
            {
                StatusIcon = "?";
                StatusMessage = result.ErrorMessage ?? "Poll failed";
                Log($"Poll failed: {result.ErrorMessage}");

                // If V2 failed, check if we fell back to V1
                if (result.ErrorMessage?.Contains("Cloudflare") == true ||
                    result.ErrorMessage?.Contains("403") == true)
                {
                    ApiStatusText = "V1 fallback (V2 blocked)";
                    IsUsingV1Fallback = true;
                    Log("Cloudflare block detected — using V1 fallback");
                }
            }
        }
        catch (Exception ex)
        {
            StatusIcon = "?";
            StatusMessage = $"Error: {ex.Message}";
            Log($"Poll error: {ex}");
        }
    }

    private static void Log(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[MainViewModel] {message}");
    }

    #region Tracking Service Event Handlers

    private void OnTrackingServiceAchievementsUnlocked(object? sender, AchievementsUnlockedEventArgs e)
    {
        // Must dispatch to UI thread
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            StatusIcon = "??";
            StatusMessage = "CHEEVOS POP!";

            // Update current game from tracking service
            if (_trackingService?.CurrentGame != null)
            {
                CurrentGame = _trackingService.CurrentGame;
            }

            // Write alert labels for each unlocked achievement
            var setName = HasMultipleSets ? SelectedSetName : null;
            foreach (var achievement in e.Achievements)
            {
                AchievementUnlocked?.Invoke(this, achievement);
                _streamLabelService.WriteAlertLabels(achievement, setName);
            }

            // Update stream labels with new state
            WriteAllStreamLabels();

            // Check if current focus was unlocked and find new focus
            if (CurrentFocusAchievement != null && 
                e.Achievements.Any(a => a.Id == CurrentFocusAchievement.Id))
            {
                FindNewFocus();
            }
        });
    }

    private void OnTrackingServiceGameChanged(object? sender, GameChangedEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            CurrentGame = e.Game;
            StatusMessage = $"Changed game to [{e.Game.Title}]";

            // Clear and rewrite stream labels for new game
            _streamLabelService.ClearAllLabels();

            // Auto-select first locked achievement for focus
            if (LockedAchievements.Count > 0)
            {
                CurrentFocusIndex = 0;
            }

            // Save the game and set selection to settings
            SaveGameAndSetToSettings();

            // Write all stream labels for new game
            WriteAllStreamLabels();
        });
    }

    private void OnTrackingServiceGameMastered(object? sender, GameMasteredEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            StatusIcon = "??";
            StatusMessage = $"MASTERED: {e.Game.Title}!";
            GameMastered?.Invoke(this, e.Game);

            // Write mastery alert labels
            var setName = HasMultipleSets ? SelectedSetName : null;
            _streamLabelService.WriteAlertLabels(e.Game, setName);
        });
    }

    private void OnTrackingServiceUserInfoUpdated(object? sender, UserInfoUpdatedEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            UserSummary = e.User;

            // Write user info stream labels
            _streamLabelService.WriteUserInfoLabels(e.User);
        });
    }

    private void OnTrackingServicePollingStatusChanged(object? sender, PollingStatusEventArgs e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            StatusMessage = e.Status;
        });
    }

    #endregion

    private void Stop()
    {
        IsPolling = false;
        _pollingTimer?.Stop();
        _pollingTimer = null;

        // Unsubscribe from tracking service events
        if (_trackingService != null)
        {
            _trackingService.AchievementsUnlocked -= OnTrackingServiceAchievementsUnlocked;
            _trackingService.GameChanged -= OnTrackingServiceGameChanged;
            _trackingService.GameMastered -= OnTrackingServiceGameMastered;
            _trackingService.UserInfoUpdated -= OnTrackingServiceUserInfoUpdated;
            _trackingService.PollingStatusChanged -= OnTrackingServicePollingStatusChanged;
            _trackingService.Reset();
        }

        // Clear stream labels when stopping
        _streamLabelService.ClearAllLabels();

        StatusIcon = "?";
        StatusMessage = "Stopped.";
    }

    private void UpdateAchievementLists()
    {
        // Update available achievement sets first (for multi-set games)
        UpdateAvailableAchievementSets();

        LockedAchievements.Clear();
        UnlockedAchievements.Clear();
        RecentUnlocks.Clear();

        // Get achievements from the appropriate source (selected set or default)
        var achievements = GetAchievementsForCurrentSet();
        if (achievements == null || achievements.Count == 0) return;

        var locked = achievements.Where(a => !a.DateEarned.HasValue).ToList();
        var unlocked = achievements.Where(a => a.DateEarned.HasValue)
            .OrderByDescending(a => a.DateEarned).ToList();

        foreach (var a in locked) LockedAchievements.Add(a);
        foreach (var a in unlocked) UnlockedAchievements.Add(a);

        // Recent unlocks (last 5)
        foreach (var a in unlocked.Take(5)) RecentUnlocks.Add(a);

        // Auto-select first locked achievement for focus
        if (CurrentFocusAchievement == null && LockedAchievements.Count > 0)
        {
            CurrentFocusIndex = 0;
        }

        OnPropertyChanged(nameof(CanNavigateFocus));
    }

    /// <summary>
    /// Finds a new focus achievement using the tracking service's FindNextFocus method.
    /// Called when the current focus achievement is unlocked.
    /// </summary>
    private void FindNewFocus()
    {
        if (_trackingService == null || LockedAchievements.Count == 0)
        {
            CurrentFocusAchievement = null;
            return;
        }

        var nextFocus = _trackingService.FindNextFocus(CurrentFocusAchievement, RefocusBehavior);
        if (nextFocus != null)
        {
            // Find the index in our local locked achievements list
            var index = LockedAchievements.ToList().FindIndex(a => a.Id == nextFocus.Id);
            if (index >= 0)
            {
                CurrentFocusIndex = index;
            }
            else
            {
                // Fallback to the achievement from the service
                CurrentFocusAchievement = nextFocus;
            }
            
            FocusChanged?.Invoke(this, CurrentFocusAchievement!);
        }
    }

    private void PreviousFocus()
    {
        if (LockedAchievements.Count == 0) return;

        CurrentFocusIndex = CurrentFocusIndex <= 0
            ? LockedAchievements.Count - 1
            : CurrentFocusIndex - 1;

        FocusChanged?.Invoke(this, CurrentFocusAchievement!);
    }

    private void NextFocus()
    {
        if (LockedAchievements.Count == 0) return;

        CurrentFocusIndex = CurrentFocusIndex >= LockedAchievements.Count - 1
            ? 0
            : CurrentFocusIndex + 1;

        FocusChanged?.Invoke(this, CurrentFocusAchievement!);
    }

    private void SetFocus()
    {
        if (CurrentFocusAchievement != null)
        {
            FocusChanged?.Invoke(this, CurrentFocusAchievement);

            // Write focus stream labels
            var setName = HasMultipleSets ? SelectedSetName : null;
            _streamLabelService.WriteFocusLabels(CurrentFocusAchievement, setName);
        }
    }

    /// <summary>
    /// Writes all current state to stream labels.
    /// Called when stream labels are enabled or game/user data changes.
    /// </summary>
    private void WriteAllStreamLabels()
    {
        if (!_streamLabelService.IsEnabled) return;

        var setName = HasMultipleSets ? SelectedSetName : null;

        // Write user info
        _streamLabelService.WriteUserInfoLabels(UserSummary);

        // Write game info and progress
        _streamLabelService.WriteGameInfoLabels(CurrentGame, setName);
        _streamLabelService.WriteGameProgressLabels(CurrentGame);

        // Write focus
        _streamLabelService.WriteFocusLabels(CurrentFocusAchievement, setName);

        // Write recent unlocks
        _streamLabelService.WriteRecentUnlocksLabels(UnlockedAchievements.ToList());
    }

    // Overlay open commands
    private void OpenFocusOverlay() => FocusOverlayRequested?.Invoke(this, EventArgs.Empty);
    private void OpenAlertsOverlay() => AlertsOverlayRequested?.Invoke(this, EventArgs.Empty);
    private void OpenUserInfoOverlay() => UserInfoOverlayRequested?.Invoke(this, EventArgs.Empty);
    private void OpenGameInfoOverlay() => GameInfoOverlayRequested?.Invoke(this, EventArgs.Empty);
    private void OpenGameProgressOverlay() => GameProgressOverlayRequested?.Invoke(this, EventArgs.Empty);
    private void OpenRecentUnlocksOverlay() => RecentUnlocksOverlayRequested?.Invoke(this, EventArgs.Empty);
    private void OpenAchievementListOverlay() => AchievementListOverlayRequested?.Invoke(this, EventArgs.Empty);
    private void OpenRelatedMediaOverlay() => RelatedMediaOverlayRequested?.Invoke(this, EventArgs.Empty);

    private void TestAchievementAlert()
    {
        // Create a sample core achievement for testing
        var sample = new Achievement
        {
            Title = "Test Achievement",
            Description = "This is a test notification!",
            Points = 10,
            BadgeUri = "https://media.retroachievements.org/Badge/00001.png",
            SetType = AchievementSetType.Core,
            SetName = "Core"
        };
        AchievementUnlocked?.Invoke(this, sample);
    }

    private void TestMasteryAlert()
    {
        if (CurrentGame != null)
        {
            GameMastered?.Invoke(this, CurrentGame);
        }
    }

    private void TestSubsetAchievementAlert()
    {
        // Create a sample subset/bonus achievement for testing
        var sample = new Achievement
        {
            Title = "Bonus Challenge Complete!",
            Description = "This is a subset achievement notification!",
            Points = 25,
            BadgeUri = "https://media.retroachievements.org/Badge/00002.png",
            SetType = AchievementSetType.Bonus,
            SetName = "Bonus"
        };
        AchievementUnlocked?.Invoke(this, sample);
    }

    private void TestSubsetMasteryAlert()
    {
        // Create a sample subset mastery notification
        // Use current game if available, otherwise create sample
        var gameInfo = CurrentGame ?? new GameInfo
        {
            Title = "Super Mario Bros. [Subset - Bonus]",
            ConsoleName = "NES",
            BadgeUri = "https://media.retroachievements.org/Images/000001.png"
        };

        // For subset mastery, we want to indicate it's a subset completion
        var subsetGame = new GameInfo
        {
            Id = gameInfo.Id,
            Title = gameInfo.Title + " [Bonus Set]",
            ConsoleName = gameInfo.ConsoleName,
            BadgeUri = gameInfo.BadgeUri,
            Developer = gameInfo.Developer,
            Publisher = gameInfo.Publisher,
            Genre = gameInfo.Genre,
            Released = gameInfo.Released
        };

        GameMastered?.Invoke(this, subsetGame);
    }

    #endregion

    #region Cleanup

    public void Cleanup()
    {
        _pollingTimer?.Stop();
        _pollingTimer = null;

        // Unsubscribe from tracking service events
        if (_trackingService != null)
        {
            _trackingService.AchievementsUnlocked -= OnTrackingServiceAchievementsUnlocked;
            _trackingService.GameChanged -= OnTrackingServiceGameChanged;
            _trackingService.GameMastered -= OnTrackingServiceGameMastered;
            _trackingService.UserInfoUpdated -= OnTrackingServiceUserInfoUpdated;
            _trackingService.PollingStatusChanged -= OnTrackingServicePollingStatusChanged;
        }

        _trackingService = null;
        _serviceFactory?.Dispose();
        _serviceFactory = null;

        // Clear stream labels on cleanup
        _streamLabelService.ClearAllLabels();

        // Flush any pending settings saves
        _settingsService.Flush();
    }

    #endregion
}

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using RATracker.WPF.Services;
using RATracker.WPF.ViewModels;
using RATracker.WPF.Views;

namespace RATracker.WPF;

/// <summary>
/// Main control panel for the Retro Achievement Tracker WPF application.
/// Handles window lifecycle, settings persistence, and overlay management.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private FocusOverlay? _focusOverlay;
    private AlertsOverlay? _alertsOverlay;
    private UserInfoOverlay? _userInfoOverlay;
    private GameInfoOverlay? _gameInfoOverlay;
    private GameProgressOverlay? _gameProgressOverlay;
    private RecentUnlocksOverlay? _recentUnlocksOverlay;
    private RelatedMediaOverlay? _relatedMediaOverlay;
    private AchievementListOverlay? _achievementListOverlay;
    private bool _isInitializingFocusSettings;
    private bool _isRestoringApiKey;
    private bool _isNavigating;
    private Grid? _currentPage;

    public MainWindow()
    {
        try
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Subscribe to overlay requests
            _viewModel.FocusOverlayRequested += OnFocusOverlayRequested;
            _viewModel.AlertsOverlayRequested += OnAlertsOverlayRequested;
            _viewModel.UserInfoOverlayRequested += OnUserInfoOverlayRequested;
            _viewModel.GameInfoOverlayRequested += OnGameInfoOverlayRequested;
            _viewModel.GameProgressOverlayRequested += OnGameProgressOverlayRequested;
            _viewModel.RecentUnlocksOverlayRequested += OnRecentUnlocksOverlayRequested;
            _viewModel.RelatedMediaOverlayRequested += OnRelatedMediaOverlayRequested;
            _viewModel.AchievementListOverlayRequested += OnAchievementListOverlayRequested;
            _viewModel.AchievementUnlocked += OnAchievementUnlocked;
            _viewModel.GameMastered += OnGameMastered;
            _viewModel.FocusChanged += OnFocusChanged;
            _viewModel.TimezoneChanged += OnTimezoneChanged;
            _viewModel.PositionModeChanged += OnPositionModeChanged;

            // Initialize focus settings sliders after component initialization
            Loaded += MainWindow_Loaded;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MainWindow constructor error: {ex}");
            System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException}");
            System.Diagnostics.Debug.WriteLine($"Inner inner exception: {ex.InnerException?.InnerException}");
            throw;
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Restore API key to password box (PasswordBox doesn't support binding)
        RestoreApiKeyToPasswordBox();

        // Initialize slider values from default ViewModel values for all overlays
        InitializeFocusSettingsFromOverlay();
        InitializeUserInfoSettingsFromOverlay();
        InitializeGameInfoSettingsFromOverlay();
        InitializeGameProgressSettingsFromOverlay();
        InitializeRecentUnlocksSettingsFromOverlay();
        InitializeAchievementListSettingsFromOverlay();
        InitializeRelatedMediaSettingsFromOverlay();

        // Handle auto-start if enabled
        await HandleAutoStartAsync();
    }

    /// <summary>
    /// Restores the saved API key to the password box.
    /// PasswordBox doesn't support data binding for security reasons,
    /// so we must set it manually.
    /// </summary>
    private void RestoreApiKeyToPasswordBox()
    {
        _isRestoringApiKey = true;
        try
        {
            if (!string.IsNullOrEmpty(_viewModel.ApiKey))
            {
                ApiKeyBox.Password = _viewModel.ApiKey;
                System.Diagnostics.Debug.WriteLine("[MainWindow] Restored API key to password box");
            }
        }
        finally
        {
            _isRestoringApiKey = false;
        }
    }

    /// <summary>
    /// Handles auto-start functionality if enabled in settings.
    /// </summary>
    private async Task HandleAutoStartAsync()
    {
        if (_viewModel.AutoStart && _viewModel.CanStart)
        {
            System.Diagnostics.Debug.WriteLine("[MainWindow] Auto-start enabled, starting polling...");

            // Small delay to let the UI fully render
            await Task.Delay(500);

            // Execute the start command
            if (_viewModel.StartCommand.CanExecute(null))
            {
                _viewModel.StartCommand.Execute(null);

                // Auto-launch configured overlay windows
                AutoLaunchOverlays();
            }
        }
    }

    /// <summary>
    /// Opens overlay windows that are configured for auto-launch.
    /// </summary>
    private void AutoLaunchOverlays()
    {
        if (_viewModel.AutoLaunchFocus)
        {
            _viewModel.OpenFocusOverlayCommand.Execute(null);
        }

        if (_viewModel.AutoLaunchAlerts)
        {
            _viewModel.OpenAlertsOverlayCommand.Execute(null);
        }

        if (_viewModel.AutoLaunchUserInfo)
        {
            _viewModel.OpenUserInfoOverlayCommand.Execute(null);
        }

        if (_viewModel.AutoLaunchGameInfo)
        {
            _viewModel.OpenGameInfoOverlayCommand.Execute(null);
        }

        if (_viewModel.AutoLaunchGameProgress)
        {
            _viewModel.OpenGameProgressOverlayCommand.Execute(null);
        }

        if (_viewModel.AutoLaunchRecentUnlocks)
        {
            _viewModel.OpenRecentUnlocksOverlayCommand.Execute(null);
        }

        if (_viewModel.AutoLaunchRelatedMedia)
        {
            _viewModel.OpenRelatedMediaOverlayCommand.Execute(null);
        }

        if (_viewModel.AutoLaunchAchievementList)
        {
            _viewModel.OpenAchievementListOverlayCommand.Execute(null);
        }
    }

    private void ApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        // Don't update ViewModel when we're restoring the saved value
        if (_isRestoringApiKey) return;

        if (sender is PasswordBox passwordBox)
        {
            _viewModel.ApiKey = passwordBox.Password;
        }
    }

    #region Focus Layout Settings

    /// <summary>
    /// Initializes the Focus settings sliders with values from the FocusOverlay ViewModel.
    /// </summary>
    private void InitializeFocusSettingsFromOverlay()
    {
        _isInitializingFocusSettings = true;

        // Ensure overlay exists to get default values
        EnsureFocusOverlayExists();
        var vm = _focusOverlay!.ViewModel;

        // Window size
        FocusWidthSlider.Value = vm.WindowWidth;
        FocusHeightSlider.Value = vm.WindowHeight;

        // Badge settings
        BadgeSizeSlider.Value = vm.BadgeSize;
        BadgeCornerSlider.Value = vm.BadgeCornerRadius;

        // Container settings
        ContainerCornerSlider.Value = vm.ContainerCornerRadius;
        ContainerMarginSlider.Value = vm.ContainerMargin;
        ContentSpacingSlider.Value = vm.ContentSpacing;

        // Element visibility
        ShowBadgeCheckBox.IsChecked = vm.ShowBadge;
        ShowTitleCheckBox.IsChecked = vm.ShowTitle;
        ShowLineCheckBox.IsChecked = vm.ShowLine;
        ShowPointsCheckBox.IsChecked = vm.ShowPoints;
        ShowDescriptionCheckBox.IsChecked = vm.ShowDescription;

        // Font sizes
        TitleFontSizeSlider.Value = vm.TitleFontSize;
        PointsFontSizeSlider.Value = vm.PointsFontSize;
        DescriptionFontSizeSlider.Value = vm.DescriptionFontSize;
        MasteryFontSizeSlider.Value = vm.MasteryInfoFontSize;

        // Line settings
        LineHeightSlider.Value = vm.LineHeight;
        LineMarginSlider.Value = vm.LineMargin;

        _isInitializingFocusSettings = false;
    }

    /// <summary>
    /// Handles slider value changes for Focus layout settings.
    /// </summary>
    private void FocusLayoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializingFocusSettings || _focusOverlay == null) return;

        var vm = _focusOverlay.ViewModel;

        // Update the appropriate property based on which slider changed
        if (sender == FocusWidthSlider)
            vm.WindowWidth = e.NewValue;
        else if (sender == FocusHeightSlider)
            vm.WindowHeight = e.NewValue;
        else if (sender == BadgeSizeSlider)
            vm.BadgeSize = e.NewValue;
        else if (sender == BadgeCornerSlider)
            vm.BadgeCornerRadius = e.NewValue;
        else if (sender == ContainerCornerSlider)
            vm.ContainerCornerRadius = e.NewValue;
        else if (sender == ContainerMarginSlider)
            vm.ContainerMargin = e.NewValue;
        else if (sender == ContentSpacingSlider)
            vm.ContentSpacing = e.NewValue;
        else if (sender == TitleFontSizeSlider)
            vm.TitleFontSize = e.NewValue;
        else if (sender == PointsFontSizeSlider)
            vm.PointsFontSize = e.NewValue;
        else if (sender == DescriptionFontSizeSlider)
            vm.DescriptionFontSize = e.NewValue;
        else if (sender == MasteryFontSizeSlider)
            vm.MasteryInfoFontSize = e.NewValue;
        else if (sender == LineHeightSlider)
            vm.LineHeight = e.NewValue;
        else if (sender == LineMarginSlider)
            vm.LineMargin = e.NewValue;
    }

    /// <summary>
    /// Handles checkbox changes for Focus element visibility.
    /// </summary>
    private void FocusVisibilityCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializingFocusSettings || _focusOverlay == null) return;

        var vm = _focusOverlay.ViewModel;

        if (sender == ShowBadgeCheckBox)
            vm.ShowBadge = ShowBadgeCheckBox.IsChecked ?? true;
        else if (sender == ShowTitleCheckBox)
            vm.ShowTitle = ShowTitleCheckBox.IsChecked ?? true;
        else if (sender == ShowLineCheckBox)
            vm.ShowLine = ShowLineCheckBox.IsChecked ?? true;
        else if (sender == ShowPointsCheckBox)
            vm.ShowPoints = ShowPointsCheckBox.IsChecked ?? true;
        else if (sender == ShowDescriptionCheckBox)
            vm.ShowDescription = ShowDescriptionCheckBox.IsChecked ?? true;
    }

    /// <summary>
    /// Resets all Focus layout settings to defaults.
    /// </summary>
    private void ResetFocusLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        EnsureFocusOverlayExists();
        _focusOverlay!.ViewModel.ResetLayoutToDefaults();
        InitializeFocusSettingsFromOverlay();
    }

    #endregion

    #region User Info Layout Settings

    private bool _isInitializingUserInfoSettings;

    private void InitializeUserInfoSettingsFromOverlay()
    {
        _isInitializingUserInfoSettings = true;

        EnsureUserInfoOverlayExists();
        var vm = _userInfoOverlay!.ViewModel;

        UserInfoWidthSlider.Value = vm.WindowWidth;
        UserInfoHeightSlider.Value = vm.WindowHeight;
        UserInfoLabelFontSizeSlider.Value = vm.LabelFontSize;
        UserInfoValueFontSizeSlider.Value = vm.ValueFontSize;
        UserInfoShowRankCheckBox.IsChecked = vm.ShowRank;
        UserInfoShowPointsCheckBox.IsChecked = vm.ShowPoints;
        UserInfoShowTruePointsCheckBox.IsChecked = vm.ShowTruePoints;
        UserInfoShowRatioCheckBox.IsChecked = vm.ShowRatio;

        _isInitializingUserInfoSettings = false;
    }

    private void UserInfoLayoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializingUserInfoSettings || _userInfoOverlay == null) return;

        var vm = _userInfoOverlay.ViewModel;

        if (sender == UserInfoWidthSlider)
            vm.WindowWidth = e.NewValue;
        else if (sender == UserInfoHeightSlider)
            vm.WindowHeight = e.NewValue;
        else if (sender == UserInfoLabelFontSizeSlider)
            vm.LabelFontSize = e.NewValue;
        else if (sender == UserInfoValueFontSizeSlider)
            vm.ValueFontSize = e.NewValue;
    }

    private void UserInfoVisibilityCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializingUserInfoSettings || _userInfoOverlay == null) return;

        var vm = _userInfoOverlay.ViewModel;

        if (sender == UserInfoShowRankCheckBox)
            vm.ShowRank = UserInfoShowRankCheckBox.IsChecked ?? true;
        else if (sender == UserInfoShowPointsCheckBox)
            vm.ShowPoints = UserInfoShowPointsCheckBox.IsChecked ?? true;
        else if (sender == UserInfoShowTruePointsCheckBox)
            vm.ShowTruePoints = UserInfoShowTruePointsCheckBox.IsChecked ?? true;
        else if (sender == UserInfoShowRatioCheckBox)
            vm.ShowRatio = UserInfoShowRatioCheckBox.IsChecked ?? true;
    }

    private void ResetUserInfoLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        EnsureUserInfoOverlayExists();
        // Reset to default values
        var vm = _userInfoOverlay!.ViewModel;
        vm.WindowWidth = 400;
        vm.WindowHeight = 200;
        vm.LabelFontSize = 18;
        vm.ValueFontSize = 24;
        vm.ShowRank = true;
        vm.ShowPoints = true;
        vm.ShowTruePoints = true;
        vm.ShowRatio = true;
        InitializeUserInfoSettingsFromOverlay();
    }

    #endregion

    #region Game Info Layout Settings

    private bool _isInitializingGameInfoSettings;

    private void InitializeGameInfoSettingsFromOverlay()
    {
        _isInitializingGameInfoSettings = true;

        EnsureGameInfoOverlayExists();
        var vm = _gameInfoOverlay!.ViewModel;

        GameInfoWidthSlider.Value = vm.WindowWidth;
        GameInfoHeightSlider.Value = vm.WindowHeight;
        GameInfoBadgeSizeSlider.Value = vm.BadgeSize;
        GameInfoLabelFontSizeSlider.Value = vm.LabelFontSize;
        GameInfoValueFontSizeSlider.Value = vm.ValueFontSize;
        GameInfoTitleFontSizeSlider.Value = vm.TitleValueFontSize;
        GameInfoShowBadgeCheckBox.IsChecked = vm.ShowBadge;
        GameInfoShowTitleCheckBox.IsChecked = vm.ShowTitle;
        GameInfoShowConsoleCheckBox.IsChecked = vm.ShowConsole;
        GameInfoShowDeveloperCheckBox.IsChecked = vm.ShowDeveloper;
        GameInfoShowPublisherCheckBox.IsChecked = vm.ShowPublisher;
        GameInfoShowGenreCheckBox.IsChecked = vm.ShowGenre;
        GameInfoShowReleasedCheckBox.IsChecked = vm.ShowReleased;

        _isInitializingGameInfoSettings = false;
    }

    private void GameInfoLayoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializingGameInfoSettings || _gameInfoOverlay == null) return;

        var vm = _gameInfoOverlay.ViewModel;

        if (sender == GameInfoWidthSlider)
            vm.WindowWidth = e.NewValue;
        else if (sender == GameInfoHeightSlider)
            vm.WindowHeight = e.NewValue;
        else if (sender == GameInfoBadgeSizeSlider)
            vm.BadgeSize = e.NewValue;
        else if (sender == GameInfoLabelFontSizeSlider)
            vm.LabelFontSize = e.NewValue;
        else if (sender == GameInfoValueFontSizeSlider)
            vm.ValueFontSize = e.NewValue;
        else if (sender == GameInfoTitleFontSizeSlider)
            vm.TitleValueFontSize = e.NewValue;
    }

    private void GameInfoVisibilityCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializingGameInfoSettings || _gameInfoOverlay == null) return;

        var vm = _gameInfoOverlay.ViewModel;

        if (sender == GameInfoShowBadgeCheckBox)
            vm.ShowBadge = GameInfoShowBadgeCheckBox.IsChecked ?? true;
        else if (sender == GameInfoShowTitleCheckBox)
            vm.ShowTitle = GameInfoShowTitleCheckBox.IsChecked ?? true;
        else if (sender == GameInfoShowConsoleCheckBox)
            vm.ShowConsole = GameInfoShowConsoleCheckBox.IsChecked ?? true;
        else if (sender == GameInfoShowDeveloperCheckBox)
            vm.ShowDeveloper = GameInfoShowDeveloperCheckBox.IsChecked ?? true;
        else if (sender == GameInfoShowPublisherCheckBox)
            vm.ShowPublisher = GameInfoShowPublisherCheckBox.IsChecked ?? true;
        else if (sender == GameInfoShowGenreCheckBox)
            vm.ShowGenre = GameInfoShowGenreCheckBox.IsChecked ?? true;
        else if (sender == GameInfoShowReleasedCheckBox)
            vm.ShowReleased = GameInfoShowReleasedCheckBox.IsChecked ?? true;
    }

    private void ResetGameInfoLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        EnsureGameInfoOverlayExists();
        var vm = _gameInfoOverlay!.ViewModel;
        vm.WindowWidth = 500;
        vm.WindowHeight = 320;
        vm.BadgeSize = 96;
        vm.LabelFontSize = 16;
        vm.ValueFontSize = 18;
        vm.TitleValueFontSize = 22;
        vm.ShowBadge = true;
        vm.ShowTitle = true;
        vm.ShowConsole = true;
        vm.ShowDeveloper = true;
        vm.ShowPublisher = true;
        vm.ShowGenre = true;
        vm.ShowReleased = true;
        InitializeGameInfoSettingsFromOverlay();
    }

    #endregion

    #region Game Progress Layout Settings

    private bool _isInitializingGameProgressSettings;

    private void InitializeGameProgressSettingsFromOverlay()
    {
        _isInitializingGameProgressSettings = true;

        EnsureGameProgressOverlayExists();
        var vm = _gameProgressOverlay!.ViewModel;

        GameProgressWidthSlider.Value = vm.WindowWidth;
        GameProgressHeightSlider.Value = vm.WindowHeight;
        GameProgressLabelFontSizeSlider.Value = vm.LabelFontSize;
        GameProgressValueFontSizeSlider.Value = vm.ValueFontSize;
        GameProgressShowAchievementsCheckBox.IsChecked = vm.ShowAchievements;
        GameProgressShowPointsCheckBox.IsChecked = vm.ShowPoints;
        GameProgressShowTruePointsCheckBox.IsChecked = vm.ShowTruePoints;
        GameProgressShowRatioCheckBox.IsChecked = vm.ShowRatio;
        GameProgressShowCompletedCheckBox.IsChecked = vm.ShowCompleted;

        _isInitializingGameProgressSettings = false;
    }

    private void GameProgressLayoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializingGameProgressSettings || _gameProgressOverlay == null) return;

        var vm = _gameProgressOverlay.ViewModel;

        if (sender == GameProgressWidthSlider)
            vm.WindowWidth = e.NewValue;
        else if (sender == GameProgressHeightSlider)
            vm.WindowHeight = e.NewValue;
        else if (sender == GameProgressLabelFontSizeSlider)
            vm.LabelFontSize = e.NewValue;
        else if (sender == GameProgressValueFontSizeSlider)
            vm.ValueFontSize = e.NewValue;
    }

    private void GameProgressVisibilityCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializingGameProgressSettings || _gameProgressOverlay == null) return;

        var vm = _gameProgressOverlay.ViewModel;

        if (sender == GameProgressShowAchievementsCheckBox)
            vm.ShowAchievements = GameProgressShowAchievementsCheckBox.IsChecked ?? true;
        else if (sender == GameProgressShowPointsCheckBox)
            vm.ShowPoints = GameProgressShowPointsCheckBox.IsChecked ?? true;
        else if (sender == GameProgressShowTruePointsCheckBox)
            vm.ShowTruePoints = GameProgressShowTruePointsCheckBox.IsChecked ?? true;
        else if (sender == GameProgressShowRatioCheckBox)
            vm.ShowRatio = GameProgressShowRatioCheckBox.IsChecked ?? true;
        else if (sender == GameProgressShowCompletedCheckBox)
            vm.ShowCompleted = GameProgressShowCompletedCheckBox.IsChecked ?? true;
    }

    private void ResetGameProgressLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        EnsureGameProgressOverlayExists();
        var vm = _gameProgressOverlay!.ViewModel;
        vm.WindowWidth = 400;
        vm.WindowHeight = 260;
        vm.LabelFontSize = 18;
        vm.ValueFontSize = 24;
        vm.ShowAchievements = true;
        vm.ShowPoints = true;
        vm.ShowTruePoints = true;
        vm.ShowRatio = true;
        vm.ShowCompleted = true;
        InitializeGameProgressSettingsFromOverlay();
    }

    #endregion

    #region Recent Unlocks Layout Settings

    private bool _isInitializingRecentUnlocksSettings;

    private void InitializeRecentUnlocksSettingsFromOverlay()
    {
        _isInitializingRecentUnlocksSettings = true;

        EnsureRecentUnlocksOverlayExists();
        var vm = _recentUnlocksOverlay!.ViewModel;

        RecentUnlocksWidthSlider.Value = vm.WindowWidth;
        RecentUnlocksHeightSlider.Value = vm.WindowHeight;
        RecentUnlocksBadgeSizeSlider.Value = vm.BadgeSize;
        RecentUnlocksItemSpacingSlider.Value = vm.ItemSpacing;
        RecentUnlocksTitleFontSizeSlider.Value = vm.TitleFontSize;
        RecentUnlocksDateFontSizeSlider.Value = vm.DateFontSize;
        RecentUnlocksPointsFontSizeSlider.Value = vm.PointsFontSize;

        _isInitializingRecentUnlocksSettings = false;
    }

    private void RecentUnlocksLayoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializingRecentUnlocksSettings || _recentUnlocksOverlay == null) return;

        var vm = _recentUnlocksOverlay.ViewModel;

        if (sender == RecentUnlocksWidthSlider)
            vm.WindowWidth = e.NewValue;
        else if (sender == RecentUnlocksHeightSlider)
            vm.WindowHeight = e.NewValue;
        else if (sender == RecentUnlocksBadgeSizeSlider)
            vm.BadgeSize = e.NewValue;
        else if (sender == RecentUnlocksItemSpacingSlider)
            vm.ItemSpacing = e.NewValue;
        else if (sender == RecentUnlocksTitleFontSizeSlider)
            vm.TitleFontSize = e.NewValue;
        else if (sender == RecentUnlocksDateFontSizeSlider)
            vm.DateFontSize = e.NewValue;
        else if (sender == RecentUnlocksPointsFontSizeSlider)
            vm.PointsFontSize = e.NewValue;
    }

    private void ResetRecentUnlocksLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        EnsureRecentUnlocksOverlayExists();
        var vm = _recentUnlocksOverlay!.ViewModel;
        vm.WindowWidth = 450;
        vm.WindowHeight = 500;
        vm.BadgeSize = 64;
        vm.ItemSpacing = 10;
        vm.TitleFontSize = 16;
        vm.DateFontSize = 12;
        vm.PointsFontSize = 18;
        InitializeRecentUnlocksSettingsFromOverlay();
    }

    #endregion

    #region Achievement List Layout Settings

    private bool _isInitializingAchievementListSettings;

    private void InitializeAchievementListSettingsFromOverlay()
    {
        _isInitializingAchievementListSettings = true;

        EnsureAchievementListOverlayExists();
        var vm = _achievementListOverlay!.ViewModel;

        AchievementListWidthSlider.Value = vm.WindowWidth;
        AchievementListHeightSlider.Value = vm.WindowHeight;
        AchievementListBadgeSizeSlider.Value = vm.BadgeSize;
        AchievementListBadgeSpacingSlider.Value = vm.BadgeSpacing;
        AchievementListShowUnlockedFirstCheckBox.IsChecked = vm.ShowUnlockedFirst;

        _isInitializingAchievementListSettings = false;
    }

    private void AchievementListLayoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializingAchievementListSettings || _achievementListOverlay == null) return;

        var vm = _achievementListOverlay.ViewModel;

        if (sender == AchievementListWidthSlider)
            vm.WindowWidth = e.NewValue;
        else if (sender == AchievementListHeightSlider)
            vm.WindowHeight = e.NewValue;
        else if (sender == AchievementListBadgeSizeSlider)
            vm.BadgeSize = e.NewValue;
        else if (sender == AchievementListBadgeSpacingSlider)
            vm.BadgeSpacing = e.NewValue;
    }

    private void AchievementListOptionsCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializingAchievementListSettings || _achievementListOverlay == null) return;

        var vm = _achievementListOverlay.ViewModel;

        if (sender == AchievementListShowUnlockedFirstCheckBox)
            vm.ShowUnlockedFirst = AchievementListShowUnlockedFirstCheckBox.IsChecked ?? true;
    }

    private void ResetAchievementListLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        EnsureAchievementListOverlayExists();
        var vm = _achievementListOverlay!.ViewModel;
        vm.WindowWidth = 680;
        vm.WindowHeight = 500;
        vm.BadgeSize = 64;
        vm.BadgeSpacing = 4;
        vm.ShowUnlockedFirst = true;
        InitializeAchievementListSettingsFromOverlay();
    }

    #endregion

    #region Related Media Layout Settings

    private bool _isInitializingRelatedMediaSettings;

    private void InitializeRelatedMediaSettingsFromOverlay()
    {
        _isInitializingRelatedMediaSettings = true;

        EnsureRelatedMediaOverlayExists();
        var vm = _relatedMediaOverlay!.ViewModel;

        RelatedMediaWidthSlider.Value = vm.WindowWidth;
        RelatedMediaHeightSlider.Value = vm.WindowHeight;
        RelatedMediaContainerCornerSlider.Value = vm.ContainerCornerRadius;
        RelatedMediaImageCornerSlider.Value = vm.ImageCornerRadius;

        _isInitializingRelatedMediaSettings = false;
    }

    private void RelatedMediaLayoutSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInitializingRelatedMediaSettings || _relatedMediaOverlay == null) return;

        var vm = _relatedMediaOverlay.ViewModel;

        if (sender == RelatedMediaWidthSlider)
            vm.WindowWidth = e.NewValue;
        else if (sender == RelatedMediaHeightSlider)
            vm.WindowHeight = e.NewValue;
        else if (sender == RelatedMediaContainerCornerSlider)
            vm.ContainerCornerRadius = e.NewValue;
        else if (sender == RelatedMediaImageCornerSlider)
            vm.ImageCornerRadius = e.NewValue;
    }

    private void ResetRelatedMediaLayoutButton_Click(object sender, RoutedEventArgs e)
    {
        EnsureRelatedMediaOverlayExists();
        var vm = _relatedMediaOverlay!.ViewModel;
        vm.WindowWidth = 640;
        vm.WindowHeight = 480;
        vm.ContainerCornerRadius = 8;
        vm.ImageCornerRadius = 5;
        InitializeRelatedMediaSettingsFromOverlay();
    }

    #endregion

    #region Overlay Management

    private void EnsureFocusOverlayExists()
    {
        if (_focusOverlay == null || !_focusOverlay.IsLoaded)
        {
            _focusOverlay = new FocusOverlay();
            _focusOverlay.Closed += (s, e) => _focusOverlay = null;
        }
    }

    private void OnFocusOverlayRequested(object? sender, EventArgs e)
    {
        EnsureFocusOverlayExists();

        if (!_focusOverlay!.IsVisible)
        {
            _focusOverlay.Show();
            _focusOverlay.ShowFocusImmediate();

            // Update with current focus if available
            if (_viewModel.CurrentFocusAchievement != null)
            {
                var setName = _viewModel.HasMultipleSets ? _viewModel.SelectedSetName : null;
                _focusOverlay.ViewModel.SetAchievement(_viewModel.CurrentFocusAchievement, setName);
            }
        }
        else
        {
            _focusOverlay.Activate();
        }
    }

    private void EnsureAlertsOverlayExists()
    {
        if (_alertsOverlay == null || !_alertsOverlay.IsLoaded)
        {
            _alertsOverlay = new AlertsOverlay();
            _alertsOverlay.Closed += (s, e) => _alertsOverlay = null;
        }
    }

    private void OnAlertsOverlayRequested(object? sender, EventArgs e)
    {
        EnsureAlertsOverlayExists();

        if (!_alertsOverlay!.IsVisible)
        {
            _alertsOverlay.Show();
        }
        else
        {
            _alertsOverlay.Activate();
        }
    }

    private void OnAchievementUnlocked(object? sender, RATracker.Models.Achievement achievement)
    {
        // Ensure Alerts overlay exists and is visible for notifications
        EnsureAlertsOverlayExists();
        
        if (!_alertsOverlay!.IsVisible)
        {
            _alertsOverlay.Show();
        }
        
        _alertsOverlay.QueueAchievementNotification(achievement);

        // Also update the Focus overlay if visible
        var setName = _viewModel.HasMultipleSets ? _viewModel.SelectedSetName : null;

        if (_focusOverlay?.IsVisible == true)
        {
            _ = _focusOverlay.TransitionToAchievement(achievement, setName);
        }
    }

    private void OnGameMastered(object? sender, RATracker.Models.GameInfo gameInfo)
    {
        // Ensure Alerts overlay exists and is visible for notifications
        EnsureAlertsOverlayExists();
        
        if (!_alertsOverlay!.IsVisible)
        {
            _alertsOverlay.Show();
        }
        
        _alertsOverlay.QueueMasteryNotification(gameInfo);

        // Also update the Focus overlay if visible
        var setName = _viewModel.HasMultipleSets ? _viewModel.SelectedSetName : null;

        if (_focusOverlay?.IsVisible == true)
        {
            _ = _focusOverlay.TransitionToMastery(gameInfo, setName);
        }
    }

    private void OnFocusChanged(object? sender, RATracker.Models.Achievement achievement)
    {
        if (_focusOverlay?.IsVisible == true)
        {
            var setName = _viewModel.HasMultipleSets ? _viewModel.SelectedSetName : null;
            _ = _focusOverlay.TransitionToAchievement(achievement, setName);
        }
    }

    #region Additional Overlay Management

    private void EnsureUserInfoOverlayExists()
    {
        if (_userInfoOverlay == null || !_userInfoOverlay.IsLoaded)
        {
            _userInfoOverlay = new UserInfoOverlay();
            _userInfoOverlay.Closed += (s, e) => _userInfoOverlay = null;
        }
    }

    private void OnUserInfoOverlayRequested(object? sender, EventArgs e)
    {
        EnsureUserInfoOverlayExists();

        if (!_userInfoOverlay!.IsVisible)
        {
            _userInfoOverlay.Show();
            _userInfoOverlay.ShowContentImmediate();

            // Update with current user info if available
            if (_viewModel.UserSummary != null)
            {
                _userInfoOverlay.ViewModel.SetUserInfo(_viewModel.UserSummary);
            }
        }
        else
        {
            _userInfoOverlay.Activate();
        }
    }

    private void EnsureGameInfoOverlayExists()
    {
        if (_gameInfoOverlay == null || !_gameInfoOverlay.IsLoaded)
        {
            _gameInfoOverlay = new GameInfoOverlay();
            _gameInfoOverlay.Closed += (s, e) => _gameInfoOverlay = null;
        }
    }

    private void OnGameInfoOverlayRequested(object? sender, EventArgs e)
    {
        EnsureGameInfoOverlayExists();

        if (!_gameInfoOverlay!.IsVisible)
        {
            _gameInfoOverlay.Show();
            _gameInfoOverlay.ShowContentImmediate();

            // Update with current game info if available
            if (_viewModel.CurrentGame != null)
            {
                _gameInfoOverlay.ViewModel.SetGameInfo(_viewModel.CurrentGame);
            }
        }
        else
        {
            _gameInfoOverlay.Activate();
        }
    }

    private void EnsureGameProgressOverlayExists()
    {
        if (_gameProgressOverlay == null || !_gameProgressOverlay.IsLoaded)
        {
            _gameProgressOverlay = new GameProgressOverlay();
            _gameProgressOverlay.Closed += (s, e) => _gameProgressOverlay = null;
        }
    }

    private void OnGameProgressOverlayRequested(object? sender, EventArgs e)
    {
        EnsureGameProgressOverlayExists();

        if (!_gameProgressOverlay!.IsVisible)
        {
            _gameProgressOverlay.Show();
            _gameProgressOverlay.ShowContentImmediate();

            // Update with current game info if available
            if (_viewModel.CurrentGame != null)
            {
                _gameProgressOverlay.ViewModel.SetGameProgress(_viewModel.CurrentGame);
            }
        }
        else
        {
            _gameProgressOverlay.Activate();
        }
    }

    private void EnsureRecentUnlocksOverlayExists()
    {
        if (_recentUnlocksOverlay == null || !_recentUnlocksOverlay.IsLoaded)
        {
            _recentUnlocksOverlay = new RecentUnlocksOverlay();
            _recentUnlocksOverlay.Closed += (s, e) => _recentUnlocksOverlay = null;
        }
    }

    private void OnRecentUnlocksOverlayRequested(object? sender, EventArgs e)
    {
        EnsureRecentUnlocksOverlayExists();

        if (!_recentUnlocksOverlay!.IsVisible)
        {
            _recentUnlocksOverlay.Show();
            _recentUnlocksOverlay.ShowContentImmediate();

            // Set the timezone from settings
            _recentUnlocksOverlay.SetTimezone(_viewModel.GetEffectiveTimezone());

            // Update with current unlocked achievements if available
            if (_viewModel.CurrentGame?.Achievements != null)
            {
                var unlockedAchievements = _viewModel.CurrentGame.Achievements
                    .Where(a => a.DateEarned.HasValue)
                    .ToList();
                _recentUnlocksOverlay.SetAchievements(unlockedAchievements);
            }
        }
        else
        {
            _recentUnlocksOverlay.Activate();
        }
    }

    /// <summary>
    /// Handles timezone changes from the settings.
    /// Updates the Recent Unlocks overlay if it's open.
    /// </summary>
    private void OnTimezoneChanged(object? sender, TimeZoneInfo timezone)
    {
        if (_recentUnlocksOverlay?.IsVisible == true)
        {
            _recentUnlocksOverlay.SetTimezone(timezone);
        }
    }

    /// <summary>
    /// Handles Position Mode changes from the settings.
    /// Updates all open overlay windows to enable/disable position mode.
    /// </summary>
    private void OnPositionModeChanged(object? sender, bool enabled)
    {
        // Update all open overlay windows
        if (_focusOverlay != null)
            _focusOverlay.IsPositionMode = enabled;
        
        if (_alertsOverlay != null)
            _alertsOverlay.IsPositionMode = enabled;
        
        if (_userInfoOverlay != null)
            _userInfoOverlay.IsPositionMode = enabled;
        
        if (_gameInfoOverlay != null)
            _gameInfoOverlay.IsPositionMode = enabled;
        
        if (_gameProgressOverlay != null)
            _gameProgressOverlay.IsPositionMode = enabled;
        
        if (_recentUnlocksOverlay != null)
            _recentUnlocksOverlay.IsPositionMode = enabled;
        
        if (_relatedMediaOverlay != null)
            _relatedMediaOverlay.IsPositionMode = enabled;
        
        if (_achievementListOverlay != null)
            _achievementListOverlay.IsPositionMode = enabled;
    }

    private void EnsureRelatedMediaOverlayExists()
    {
        if (_relatedMediaOverlay == null || !_relatedMediaOverlay.IsLoaded)
        {
            _relatedMediaOverlay = new RelatedMediaOverlay();
            _relatedMediaOverlay.Closed += (s, e) => _relatedMediaOverlay = null;
        }
    }

    private void OnRelatedMediaOverlayRequested(object? sender, EventArgs e)
    {
        EnsureRelatedMediaOverlayExists();

        if (!_relatedMediaOverlay!.IsVisible)
        {
            _relatedMediaOverlay.Show();
            _relatedMediaOverlay.ShowContentImmediate();

            // Update with current game box art if available
            if (_viewModel.CurrentGame != null)
            {
                _relatedMediaOverlay.ViewModel.SetGameImage(_viewModel.CurrentGame);
            }
        }
        else
        {
            _relatedMediaOverlay.Activate();
        }
    }

    private void EnsureAchievementListOverlayExists()
    {
        if (_achievementListOverlay == null || !_achievementListOverlay.IsLoaded)
        {
            _achievementListOverlay = new AchievementListOverlay();
            _achievementListOverlay.Closed += (s, e) => _achievementListOverlay = null;
        }
    }

    private void OnAchievementListOverlayRequested(object? sender, EventArgs e)
    {
        EnsureAchievementListOverlayExists();

        if (!_achievementListOverlay!.IsVisible)
        {
            _achievementListOverlay.Show();
            _achievementListOverlay.ShowContentImmediate();

            // Update with current game achievements if available
            if (_viewModel.CurrentGame?.Achievements != null)
            {
                _achievementListOverlay.SetAchievements(_viewModel.CurrentGame.Achievements);
            }
        }
        else
        {
            _achievementListOverlay.Activate();
        }
    }

    #endregion

    #endregion

    #region Window Lifecycle

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.Cleanup();
        _focusOverlay?.Close();
        _alertsOverlay?.Close();
        _userInfoOverlay?.Close();
        _gameInfoOverlay?.Close();
        _gameProgressOverlay?.Close();
        _recentUnlocksOverlay?.Close();
        _relatedMediaOverlay?.Close();
        _achievementListOverlay?.Close();
        base.OnClosed(e);
    }

    #endregion

    #region Stream Labels

    /// <summary>
        /// Opens the stream-labels folder in Windows Explorer.
        /// Creates the folder if it doesn't exist.
        /// </summary>
        private void OpenStreamLabelsFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var streamLabelsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stream-labels");

                // Create the directory if it doesn't exist
                if (!Directory.Exists(streamLabelsPath))
                {
                    Directory.CreateDirectory(streamLabelsPath);
                }

                // Open in Windows Explorer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = streamLabelsPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open stream-labels folder:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region Page Navigation

        /// <summary>
        /// Navigates to a settings page with a sliding animation.
        /// </summary>
        private async void NavigateToPage(Grid targetPage)
        {
            if (_isNavigating) return;
            _isNavigating = true;

            try
            {
                // Get the current page (Dashboard by default)
                var currentPage = _currentPage ?? DashboardPage;

                // Slide out the current page to the left
                var slideOutStoryboard = (Storyboard)FindResource("SlideOutLeft");
                slideOutStoryboard.Begin(currentPage);

                await Task.Delay(250); // Wait for animation to complete

                // Hide the current page and show the target page
                currentPage.Visibility = Visibility.Collapsed;
                targetPage.Visibility = Visibility.Visible;

                // Slide in the target page from the right
                var slideInStoryboard = (Storyboard)FindResource("SlideInRight");
                slideInStoryboard.Begin(targetPage);

                _currentPage = targetPage;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        /// <summary>
        /// Navigates back to the Dashboard with a sliding animation.
        /// </summary>
        private async void NavigateBackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (_isNavigating || _currentPage == null || _currentPage == DashboardPage) return;
            _isNavigating = true;

            try
            {
                // Slide out the current page to the right
                var slideOutStoryboard = (Storyboard)FindResource("SlideOutRight");
                slideOutStoryboard.Begin(_currentPage);

                await Task.Delay(250); // Wait for animation to complete

                // Hide the current page and show the dashboard
                _currentPage.Visibility = Visibility.Collapsed;
                DashboardPage.Visibility = Visibility.Visible;

                // Slide in the dashboard from the left
                var slideInStoryboard = (Storyboard)FindResource("SlideInLeft");
                slideInStoryboard.Begin(DashboardPage);

                _currentPage = DashboardPage;
            }
            finally
            {
                _isNavigating = false;
            }
        }

        // Navigation button click handlers
        private void NavigateToFocusSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage(FocusSettingsPage);
        private void NavigateToAlertsSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage(AlertsSettingsPage);
        private void NavigateToUserInfoSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage(UserInfoSettingsPage);
        private void NavigateToGameInfoSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage(GameInfoSettingsPage);
        private void NavigateToGameProgressSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage(GameProgressSettingsPage);
        private void NavigateToRecentUnlocksSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage(RecentUnlocksSettingsPage);
        private void NavigateToAchievementListSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage(AchievementListSettingsPage);
        private void NavigateToRelatedMediaSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage(RelatedMediaSettingsPage);
        private void NavigateToGeneralSettings_Click(object sender, RoutedEventArgs e) => NavigateToPage(GeneralSettingsPage);

        #endregion
    }


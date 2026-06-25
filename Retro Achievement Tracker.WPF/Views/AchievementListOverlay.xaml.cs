using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using RATracker.Models;
using RATracker.WPF.Services;
using RATracker.WPF.ViewModels;

namespace RATracker.WPF.Views;

/// <summary>
/// Achievement List Overlay Window - Displays all achievements for a game in a grid layout.
/// Shows unlocked achievements with gold borders, locked with gray borders.
/// Includes tooltips with achievement details on hover.
/// </summary>
public partial class AchievementListOverlay : Window
{
    private readonly Storyboard _showAnimation;
    private readonly Storyboard _hideAnimation;
    private readonly SettingsService _settingsService;
    private bool _isUpdatingSize;
    private bool _isPositionMode;

    /// <summary>
    /// Gets or sets whether position mode is active for dragging.
    /// </summary>
    public bool IsPositionMode
    {
        get => _isPositionMode;
        set => _isPositionMode = value;
    }

    public AchievementListViewModel ViewModel { get; }

    public AchievementListOverlay()
    {
        InitializeComponent();

        ViewModel = new AchievementListViewModel();
        DataContext = ViewModel;
        Width = ViewModel.WindowWidth;
        Height = ViewModel.WindowHeight;
        _settingsService = SettingsService.Instance;

        _showAnimation = (Storyboard)FindResource("ShowAnimation");
        _hideAnimation = (Storyboard)FindResource("HideAnimation");

        // Allow window to be dragged
        MouseLeftButtonDown += (s, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };

        Loaded += (s, e) => RestoreWindowPosition();
        LocationChanged += (s, e) => SaveWindowPosition();

        // Set sample data for demo
        ViewModel.SetSampleData();
    }

    private void RestoreWindowPosition()
    {
        var settings = _settingsService.Settings;
        if (settings.AchievementListOverlayX.HasValue && settings.AchievementListOverlayY.HasValue)
        {
            if (IsPositionOnScreen(settings.AchievementListOverlayX.Value, settings.AchievementListOverlayY.Value))
            {
                Left = settings.AchievementListOverlayX.Value;
                Top = settings.AchievementListOverlayY.Value;
            }
        }
        IsPositionMode = settings.PositionModeEnabled;
    }

    private void SaveWindowPosition()
    {
        if (!IsLoaded) return;
        _settingsService.Settings.AchievementListOverlayX = Left;
        _settingsService.Settings.AchievementListOverlayY = Top;
        _settingsService.ScheduleSave();
    }

    private static bool IsPositionOnScreen(double x, double y)
    {
        return x >= SystemParameters.VirtualScreenLeft &&
               x < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth &&
               y >= SystemParameters.VirtualScreenTop &&
               y < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isUpdatingSize) return;
        if (!IsLoaded) return;

        _isUpdatingSize = true;
        ViewModel.WindowWidth = e.NewSize.Width;
        ViewModel.WindowHeight = e.NewSize.Height;
        _isUpdatingSize = false;
    }

    /// <summary>
    /// Plays the show animation.
    /// </summary>
    public void ShowContent()
    {
        _showAnimation.Begin(this);
    }

    /// <summary>
    /// Plays the hide animation.
    /// </summary>
    public void HideContent()
    {
        _hideAnimation.Begin(this);
    }

    /// <summary>
    /// Immediately shows the content without animation.
    /// </summary>
    public void ShowContentImmediate()
    {
        MainContainer.Opacity = 1;
        ContainerScale.ScaleX = 1;
        ContainerScale.ScaleY = 1;
    }

    /// <summary>
    /// Immediately hides the content without animation.
    /// </summary>
    public void HideContentImmediate()
    {
        MainContainer.Opacity = 0;
    }

    /// <summary>
    /// Sets all achievements for the current game.
    /// </summary>
    /// <param name="achievements">All achievements (locked and unlocked)</param>
    public void SetAchievements(IEnumerable<Achievement> achievements)
    {
        ViewModel.SetAchievements(achievements);
    }

    /// <summary>
    /// Updates the achievement list with new data, with transition animation.
    /// </summary>
    public async Task UpdateAchievements(IEnumerable<Achievement> achievements)
    {
        HideContent();
        await Task.Delay(400);
        ViewModel.SetAchievements(achievements);
        ShowContent();
    }

    /// <summary>
    /// Marks an achievement as unlocked with animation.
    /// </summary>
    public void UnlockAchievement(int achievementId)
    {
        ViewModel.UnlockAchievement(achievementId);
    }

    /// <summary>
    /// Clears all achievements (used when switching games).
    /// </summary>
    public async Task ClearAchievements()
    {
        HideContent();
        await Task.Delay(400);
        ViewModel.ClearAchievements();
    }
}

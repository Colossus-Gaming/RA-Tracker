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

    // Fixed layout chrome between the window edge and the badge area, matching the XAML:
    // MainContainer BorderThickness (3, when the border is enabled) and the badge ScrollViewer Padding (8).
    // ContainerMargin is read from the ViewModel. Used to snap the window to a clean badge-tile multiple.
    private const double ContainerBorderThickness = 3;
    private const double ScrollViewerPadding = 8;

    /// <summary>
    /// Gets or sets whether position mode is active for dragging.
    /// </summary>
    public bool IsPositionMode
    {
        get => _isPositionMode;
        set
        {
            _isPositionMode = value;
            UpdatePositioningGuideVisibility();
        }
    }

    private void UpdatePositioningGuideVisibility()
    {
        if (PositioningGuide == null) return;
        PositioningGuide.Visibility = _isPositionMode ? Visibility.Visible : Visibility.Collapsed;
        PositioningGuide.Background = _isPositionMode
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0)) // hit-testable
            : System.Windows.Media.Brushes.Transparent;
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

        Loaded += (s, e) => { RestoreWindowPosition(); SnapSize(); };
        LocationChanged += (s, e) => SaveWindowPosition();

        // Keep the window a clean badge-tile multiple when the badge/border/spacing size changes.
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName is nameof(AchievementListViewModel.BadgeSize)
                or nameof(AchievementListViewModel.BadgeSpacing)
                or nameof(AchievementListViewModel.UnlockedBorderThickness)
                or nameof(AchievementListViewModel.LockedBorderThickness)
                or nameof(AchievementListViewModel.ContainerMargin)
                or nameof(AchievementListViewModel.BorderEnabled))
            {
                if (IsLoaded) SnapSize();
            }
        };

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
        var (w, h) = SnapToBadgeGrid(e.NewSize.Width, e.NewSize.Height);
        ViewModel.WindowWidth = w;
        ViewModel.WindowHeight = h;
        _isUpdatingSize = false;
    }

    /// <summary>
    /// Snaps a window size to a clean multiple of the badge tile so the grid fills edge-to-edge with no
    /// partial column/row. tile = BadgeSize + spacing + border; chrome = container margin + container
    /// border + scroll padding (all doubled for both sides).
    /// </summary>
    private (double width, double height) SnapToBadgeGrid(double width, double height)
    {
        double tile = ViewModel.TotalBadgeSize;
        if (tile <= 0) return (width, height);

        double border = ViewModel.BorderEnabled ? ContainerBorderThickness : 0;
        double chrome = (2 * ViewModel.ContainerMargin) + (2 * border) + (2 * ScrollViewerPadding);

        int cols = Math.Max(1, (int)Math.Round((width - chrome) / tile));
        int rows = Math.Max(1, (int)Math.Round((height - chrome) / tile));
        return ((cols * tile) + chrome, (rows * tile) + chrome);
    }

    /// <summary>Re-snaps the current window size to the badge grid (e.g. after a badge-size change).</summary>
    private void SnapSize()
    {
        if (_isUpdatingSize) return;
        _isUpdatingSize = true;
        var (w, h) = SnapToBadgeGrid(
            ActualWidth > 0 ? ActualWidth : ViewModel.WindowWidth,
            ActualHeight > 0 ? ActualHeight : ViewModel.WindowHeight);
        ViewModel.WindowWidth = w;
        ViewModel.WindowHeight = h;
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

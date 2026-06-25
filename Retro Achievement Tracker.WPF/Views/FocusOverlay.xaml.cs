using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using RATracker.WPF.Services;
using RATracker.WPF.ViewModels;

namespace RATracker.WPF.Views;

/// <summary>
/// Focus Overlay Window - Displays the currently focused achievement or game mastery.
/// Features native WPF animations instead of WebView2/JavaScript.
/// </summary>
public partial class FocusOverlay : Window
{
    private readonly Storyboard _showAnimation;
    private readonly Storyboard _hideAnimation;
    private readonly Storyboard? _masteryGlowAnimation;
    private readonly SettingsService _settingsService;
    private bool _isUpdatingSize;
    private bool _isPositionMode;

    /// <summary>
    /// Gets or sets whether position mode is active. When active, the window shows a
    /// visible guide and can be dragged. When inactive, the window is fully transparent
    /// for OBS capture.
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

    public FocusViewModel ViewModel { get; }

    public FocusOverlay()
    {
        InitializeComponent();

        ViewModel = new FocusViewModel();
        DataContext = ViewModel;
        Width = ViewModel.WindowWidth;
        Height = ViewModel.WindowHeight;
        _settingsService = SettingsService.Instance;

        // Get animation storyboards from resources
        _showAnimation = (Storyboard)FindResource("ShowFocusAnimation");
        _hideAnimation = (Storyboard)FindResource("HideFocusAnimation");
        _masteryGlowAnimation = FindResource("MasteryGlowAnimation") as Storyboard;

        // Allow window to be dragged when in position mode
        MouseLeftButtonDown += (s, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };

        // Toggle position mode with 'P' key
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.P)
                IsPositionMode = !IsPositionMode;
        };

        // Restore saved position
        Loaded += (s, e) => RestoreWindowPosition();

        // Save position when window is moved
        LocationChanged += (s, e) => SaveWindowPosition();

        // Start with sample data
        ViewModel.SetSampleAchievement();
    }

    /// <summary>
    /// Restores the window position from saved settings.
    /// </summary>
    private void RestoreWindowPosition()
    {
        var settings = _settingsService.Settings;
        if (settings.FocusOverlayX.HasValue && settings.FocusOverlayY.HasValue)
        {
            if (IsPositionOnScreen(settings.FocusOverlayX.Value, settings.FocusOverlayY.Value))
            {
                Left = settings.FocusOverlayX.Value;
                Top = settings.FocusOverlayY.Value;
            }
        }
    }

    /// <summary>
    /// Saves the current window position to settings.
    /// </summary>
    private void SaveWindowPosition()
    {
        if (!IsLoaded) return;

        _settingsService.Settings.FocusOverlayX = Left;
        _settingsService.Settings.FocusOverlayY = Top;
        _settingsService.ScheduleSave();
    }

    /// <summary>
    /// Checks if a position is within the virtual screen bounds (all monitors combined).
    /// </summary>
    private static bool IsPositionOnScreen(double x, double y)
    {
        return x >= SystemParameters.VirtualScreenLeft &&
               x < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth &&
               y >= SystemParameters.VirtualScreenTop &&
               y < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;
    }

    /// <summary>
    /// Updates the positioning guide visibility based solely on this window's position mode.
    /// </summary>
    private void UpdatePositioningGuideVisibility()
    {
        if (PositioningGuide == null) return;
        PositioningGuide.Visibility = _isPositionMode ? Visibility.Visible : Visibility.Collapsed;
        PositioningGuide.Background = _isPositionMode
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0)) // hit-testable
            : System.Windows.Media.Brushes.Transparent;
    }

    /// <summary>
    /// Handles window size changed to update the ViewModel.
    /// </summary>
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
    /// Plays the show animation to reveal the focus content.
    /// </summary>
    public void ShowFocus()
    {
        _showAnimation.Begin(this);

        // Start mastery glow if in mastery mode
        if (ViewModel.IsMasteryMode && _masteryGlowAnimation != null)
        {
            _masteryGlowAnimation.Begin(this);
        }
    }

    /// <summary>
    /// Plays the hide animation to conceal the focus content.
    /// </summary>
    public void HideFocus()
    {
        _masteryGlowAnimation?.Stop(this);
        _hideAnimation.Begin(this);
    }

    /// <summary>
    /// Immediately shows the focus without animation.
    /// </summary>
    public void ShowFocusImmediate()
    {
        FocusContainer.Opacity = 1;
        BadgeBorder.Opacity = 1;
        TitleContainer.Opacity = 1;
        Line.Opacity = 1;
        PointsContainer.Opacity = 1;
        DescriptionContainer.Opacity = 1;

        BadgeTransform.X = 0;
        TitleTransform.X = 0;
        LineTransform.X = 0;
        PointsTransform.X = 0;
        DescriptionTransform.X = 0;

        if (ViewModel.IsMasteryMode && _masteryGlowAnimation != null)
        {
            _masteryGlowAnimation.Begin(this);
        }
    }

    /// <summary>
    /// Immediately hides the focus without animation.
    /// </summary>
    public void HideFocusImmediate()
    {
        _masteryGlowAnimation?.Stop(this);

        FocusContainer.Opacity = 0;
        BadgeBorder.Opacity = 0;
        TitleContainer.Opacity = 0;
        Line.Opacity = 0;
        PointsContainer.Opacity = 0;
        DescriptionContainer.Opacity = 0;
    }

    /// <summary>
    /// Updates the focus with a new achievement, playing the transition animation.
    /// </summary>
    /// <param name="achievement">The achievement to display.</param>
    /// <param name="setName">Optional name of the achievement set (for multi-set games).</param>
    public async Task TransitionToAchievement(RATracker.Models.Achievement achievement, string? setName = null)
    {
        // Hide current content
        HideFocus();

        // Wait for hide animation
        await Task.Delay(500);

        // Update content
        ViewModel.SetAchievement(achievement, setName);

        // Show new content
        ShowFocus();
    }

    /// <summary>
    /// Updates the focus with game mastery info, playing the transition animation.
    /// </summary>
    /// <param name="gameInfo">The game info to display.</param>
    /// <param name="setName">Optional name of the achievement set (for multi-set mastery).</param>
    public async Task TransitionToMastery(RATracker.Models.GameInfo gameInfo, string? setName = null)
    {
        // Hide current content
        HideFocus();

        // Wait for hide animation
        await Task.Delay(500);

        // Update content
        ViewModel.SetGameMastery(gameInfo, setName);

        // Show new content
        ShowFocus();
    }

    protected override void OnClosed(EventArgs e)
    {
        _masteryGlowAnimation?.Stop(this);
        base.OnClosed(e);
    }
}

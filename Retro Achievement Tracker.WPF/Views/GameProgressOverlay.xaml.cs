using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using RATracker.WPF.Services;
using RATracker.WPF.ViewModels;

namespace RATracker.WPF.Views;

/// <summary>
/// Game Progress Overlay Window - Displays achievements, points, true points, ratio, and completion percentage.
/// </summary>
public partial class GameProgressOverlay : Window
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

    public GameProgressViewModel ViewModel { get; }

    public GameProgressOverlay()
    {
        InitializeComponent();

        ViewModel = new GameProgressViewModel();
        DataContext = ViewModel;
        _settingsService = SettingsService.Instance;

        _showAnimation = (Storyboard)FindResource("ShowAnimation");
        _hideAnimation = (Storyboard)FindResource("HideAnimation");

        MouseLeftButtonDown += (s, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };

        Loaded += (s, e) => RestoreWindowPosition();
        LocationChanged += (s, e) => SaveWindowPosition();

        ViewModel.SetSampleData();
    }

    private void RestoreWindowPosition()
    {
        var settings = _settingsService.Settings;
        if (settings.GameProgressOverlayX.HasValue && settings.GameProgressOverlayY.HasValue)
        {
            if (IsPositionOnScreen(settings.GameProgressOverlayX.Value, settings.GameProgressOverlayY.Value))
            {
                Left = settings.GameProgressOverlayX.Value;
                Top = settings.GameProgressOverlayY.Value;
            }
        }
        IsPositionMode = settings.PositionModeEnabled;
    }

    private void SaveWindowPosition()
    {
        if (!IsLoaded) return;
        _settingsService.Settings.GameProgressOverlayX = Left;
        _settingsService.Settings.GameProgressOverlayY = Top;
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
    /// Updates the display with new game progress, with transition animation.
    /// </summary>
    public async Task UpdateGameProgress(RATracker.Models.GameInfo gameInfo)
    {
        HideContent();
        await Task.Delay(400);
        ViewModel.SetGameProgress(gameInfo);
        ShowContent();
    }
}

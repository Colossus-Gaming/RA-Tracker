using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using RATracker.WPF.Services;
using RATracker.WPF.ViewModels;

namespace RATracker.WPF.Views;

/// <summary>
/// Related Media Overlay Window - Displays game box art, screenshots, and title screens.
/// </summary>
public partial class RelatedMediaOverlay : Window
{
    private readonly Storyboard _showAnimation;
    private readonly Storyboard _hideAnimation;
    private readonly Storyboard _imageTransitionAnimation;
    private readonly SettingsService _settingsService;
    private bool _isUpdatingSize;
    private bool _isPositionMode;

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

    public RelatedMediaViewModel ViewModel { get; }

    public RelatedMediaOverlay()
    {
        InitializeComponent();

        ViewModel = new RelatedMediaViewModel();
        DataContext = ViewModel;
        Width = ViewModel.WindowWidth;
        Height = ViewModel.WindowHeight;
        _settingsService = SettingsService.Instance;

        _showAnimation = (Storyboard)FindResource("ShowAnimation");
        _hideAnimation = (Storyboard)FindResource("HideAnimation");
        _imageTransitionAnimation = (Storyboard)FindResource("ImageTransitionAnimation");

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
        if (settings.RelatedMediaOverlayX.HasValue && settings.RelatedMediaOverlayY.HasValue)
        {
            if (IsPositionOnScreen(settings.RelatedMediaOverlayX.Value, settings.RelatedMediaOverlayY.Value))
            {
                Left = settings.RelatedMediaOverlayX.Value;
                Top = settings.RelatedMediaOverlayY.Value;
            }
        }
    }

    private void SaveWindowPosition()
    {
        if (!IsLoaded) return;
        _settingsService.Settings.RelatedMediaOverlayX = Left;
        _settingsService.Settings.RelatedMediaOverlayY = Top;
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
    /// Sets the image with a transition animation.
    /// </summary>
    public void SetImage(string imageUri)
    {
        ViewModel.SetImage(imageUri);
        _imageTransitionAnimation.Begin(this);
    }

    /// <summary>
    /// Updates the display with game info, with transition animation.
    /// </summary>
    public async Task UpdateGameMedia(RATracker.Models.GameInfo gameInfo)
    {
        HideContent();
        await Task.Delay(400);
        ViewModel.SetGameImage(gameInfo);
        ShowContent();
    }
}

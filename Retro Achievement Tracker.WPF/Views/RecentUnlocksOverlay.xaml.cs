using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using RATracker.WPF.Services;
using RATracker.WPF.ViewModels;

namespace RATracker.WPF.Views;

/// <summary>
/// Converter for formatting DateEarned values in the recent unlocks list.
/// Uses the timezone from the ViewModel when available.
/// </summary>
public class DateEarnedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is DateTime dateEarned && values[1] is TimeZoneInfo timezone)
        {
            return RecentUnlocksViewModel.FormatDateEarned(dateEarned, timezone);
        }
        
        // Fallback for single value binding
        if (values.Length >= 1 && values[0] is DateTime date)
        {
            return RecentUnlocksViewModel.FormatDateEarned(date);
        }
        
        return string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Simple single-value converter for backward compatibility.
/// </summary>
public class SimpleDateEarnedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime dateEarned)
        {
            return RecentUnlocksViewModel.FormatDateEarned(dateEarned);
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Recent Unlocks Overlay Window - Displays a scrollable list of recently unlocked achievements.
/// </summary>
public partial class RecentUnlocksOverlay : Window
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

    public RecentUnlocksViewModel ViewModel { get; }

    public RecentUnlocksOverlay()
    {
        InitializeComponent();

        ViewModel = new RecentUnlocksViewModel();
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
        if (settings.RecentUnlocksOverlayX.HasValue && settings.RecentUnlocksOverlayY.HasValue)
        {
            if (IsPositionOnScreen(settings.RecentUnlocksOverlayX.Value, settings.RecentUnlocksOverlayY.Value))
            {
                Left = settings.RecentUnlocksOverlayX.Value;
                Top = settings.RecentUnlocksOverlayY.Value;
            }
        }
        IsPositionMode = settings.PositionModeEnabled;
    }

    private void SaveWindowPosition()
    {
        if (!IsLoaded) return;
        _settingsService.Settings.RecentUnlocksOverlayX = Left;
        _settingsService.Settings.RecentUnlocksOverlayY = Top;
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
    }

    /// <summary>
    /// Immediately hides the content without animation.
    /// </summary>
    public void HideContentImmediate()
    {
        MainContainer.Opacity = 0;
    }

    /// <summary>
    /// Updates the list with new achievements.
    /// </summary>
    public void SetAchievements(IEnumerable<RATracker.Models.Achievement> achievements)
    {
        ViewModel.SetAchievements(achievements);
    }

    /// <summary>
    /// Adds a new achievement to the list with animation.
    /// </summary>
    public void AddAchievement(RATracker.Models.Achievement achievement)
    {
        ViewModel.AddAchievement(achievement);
    }

    /// <summary>
    /// Clears all achievements from the list.
    /// </summary>
    public void ClearAchievements()
    {
        ViewModel.ClearAchievements();
    }

    /// <summary>
    /// Sets the timezone for displaying achievement unlock times.
    /// </summary>
    /// <param name="timezone">The timezone to use for display.</param>
    public void SetTimezone(TimeZoneInfo timezone)
    {
        ViewModel.Timezone = timezone;
    }
}

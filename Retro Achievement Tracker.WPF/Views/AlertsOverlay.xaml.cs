using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using RATracker.Models;
using RATracker.WPF.Services;
using RATracker.WPF.ViewModels;

namespace RATracker.WPF.Views;

/// <summary>
/// Alerts Overlay Window - Displays achievement unlock and mastery notifications with animations.
/// </summary>
public partial class AlertsOverlay : Window
{
    private readonly ConcurrentQueue<NotificationItem> _notificationQueue = new();
    private readonly SettingsService _settingsService;
    private bool _isPlaying;
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

    private void UpdatePositioningGuideVisibility()
    {
        if (PositioningGuide == null) return;
        PositioningGuide.Visibility = _isPositionMode ? Visibility.Visible : Visibility.Collapsed;
        PositioningGuide.Background = _isPositionMode
            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(1, 0, 0, 0)) // hit-testable
            : System.Windows.Media.Brushes.Transparent;
    }

    public AlertsViewModel ViewModel { get; }

    public AlertsOverlay()
    {
        InitializeComponent();

        ViewModel = new AlertsViewModel();
        DataContext = ViewModel;
        Width = ViewModel.WindowWidth;
        Height = ViewModel.WindowHeight;
        _settingsService = SettingsService.Instance;

        // Only allow dragging when in position mode
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
    }

    /// <summary>
    /// Restores the window position from saved settings.
    /// </summary>
    private void RestoreWindowPosition()
    {
        var settings = _settingsService.Settings;
        if (settings.AlertsOverlayX.HasValue && settings.AlertsOverlayY.HasValue)
        {
            if (IsPositionOnScreen(settings.AlertsOverlayX.Value, settings.AlertsOverlayY.Value))
            {
                Left = settings.AlertsOverlayX.Value;
                Top = settings.AlertsOverlayY.Value;
            }
        }
    }

    /// <summary>
    /// Saves the current window position to settings.
    /// </summary>
    private void SaveWindowPosition()
    {
        if (!IsLoaded) return;

        _settingsService.Settings.AlertsOverlayX = Left;
        _settingsService.Settings.AlertsOverlayY = Top;
        _settingsService.ScheduleSave();
    }

    /// <summary>
    /// Checks if a position is within the virtual screen bounds (all monitors combined).
    /// </summary>
    private static bool IsPositionOnScreen(double x, double y)
    {
        // Use WPF's SystemParameters to check if position is within virtual screen
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
    /// Queues an achievement notification.
    /// </summary>
    public void QueueAchievementNotification(Achievement achievement)
    {
        _notificationQueue.Enqueue(new NotificationItem
        {
            Type = NotificationType.Achievement,
            Achievement = achievement
        });

        ProcessQueue();
    }

    /// <summary>
    /// Queues a mastery notification.
    /// </summary>
    public void QueueMasteryNotification(GameInfo gameInfo)
    {
        _notificationQueue.Enqueue(new NotificationItem
        {
            Type = NotificationType.Mastery,
            GameInfo = gameInfo
        });

        ProcessQueue();
    }

    /// <summary>
    /// Shows a test achievement notification.
    /// </summary>
    public async Task ShowTestAchievementNotification()
    {
        ViewModel.SetSampleAchievementNotification();
        await PlayNotificationAnimation(false);
    }

    /// <summary>
    /// Shows a test mastery notification.
    /// </summary>
    public async Task ShowTestMasteryNotification()
    {
        ViewModel.SetSampleMasteryNotification();
        await PlayNotificationAnimation(true);
    }

    private async void ProcessQueue()
    {
        if (_isPlaying) return;

        while (_notificationQueue.TryDequeue(out var notification))
        {
            _isPlaying = true;

            if (notification.Type == NotificationType.Achievement && notification.Achievement != null)
            {
                ViewModel.SetAchievementNotification(notification.Achievement);
                await PlayNotificationAnimation(false);
            }
            else if (notification.Type == NotificationType.Mastery && notification.GameInfo != null)
            {
                ViewModel.SetMasteryNotification(notification.GameInfo);
                await PlayNotificationAnimation(true);
            }

            _isPlaying = false;
        }
    }

    private async Task PlayNotificationAnimation(bool isMastery)
    {
        var inDirection = isMastery ? ViewModel.MasteryInDirection : ViewModel.AchievementInDirection;
        var outDirection = isMastery ? ViewModel.MasteryOutDirection : ViewModel.AchievementOutDirection;
        var duration = TimeSpan.FromSeconds(ViewModel.AnimationDuration);
        var inTime = TimeSpan.FromSeconds(ViewModel.InAnimationTime);
        var outTime = TimeSpan.FromSeconds(ViewModel.OutAnimationTime);

        // Animate in
        await AnimateIn(inDirection, inTime);

        // Wait for display duration
        await Task.Delay(duration);

        // Animate out
        await AnimateOut(outDirection, outTime);
    }

    private async Task AnimateIn(AnimationDirection direction, TimeSpan duration)
    {
        var storyboard = new Storyboard();
        var easing = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 };

        // Opacity animation
        var opacityAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(opacityAnimation, AchievementContainer);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(opacityAnimation);

        // Position animation based on direction
        var (fromX, fromY) = GetStartPosition(direction);
        
        if (fromX != 0)
        {
            var xAnimation = new DoubleAnimation
            {
                From = fromX,
                To = 0,
                Duration = duration,
                EasingFunction = easing
            };
            Storyboard.SetTarget(xAnimation, AchievementContainer);
            Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            storyboard.Children.Add(xAnimation);
        }

        if (fromY != 0)
        {
            var yAnimation = new DoubleAnimation
            {
                From = fromY,
                To = 0,
                Duration = duration,
                EasingFunction = easing
            };
            Storyboard.SetTarget(yAnimation, AchievementContainer);
            Storyboard.SetTargetProperty(yAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(yAnimation);
        }

        var tcs = new TaskCompletionSource<bool>();
        storyboard.Completed += (s, e) => tcs.SetResult(true);
        storyboard.Begin(this);
        await tcs.Task;
    }

    private async Task AnimateOut(AnimationDirection direction, TimeSpan duration)
    {
        var storyboard = new Storyboard();
        var easing = new CubicEase { EasingMode = EasingMode.EaseIn };

        // Opacity animation
        var opacityAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = duration,
            EasingFunction = easing
        };
        Storyboard.SetTarget(opacityAnimation, AchievementContainer);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(opacityAnimation);

        // Position animation based on direction
        var (toX, toY) = GetEndPosition(direction);

        if (toX != 0)
        {
            var xAnimation = new DoubleAnimation
            {
                From = 0,
                To = toX,
                Duration = duration,
                EasingFunction = easing
            };
            Storyboard.SetTarget(xAnimation, AchievementContainer);
            Storyboard.SetTargetProperty(xAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            storyboard.Children.Add(xAnimation);
        }

        if (toY != 0)
        {
            var yAnimation = new DoubleAnimation
            {
                From = 0,
                To = toY,
                Duration = duration,
                EasingFunction = easing
            };
            Storyboard.SetTarget(yAnimation, AchievementContainer);
            Storyboard.SetTargetProperty(yAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(yAnimation);
        }

        var tcs = new TaskCompletionSource<bool>();
        storyboard.Completed += (s, e) => tcs.SetResult(true);
        storyboard.Begin(this);
        await tcs.Task;
    }

    private static (double x, double y) GetStartPosition(AnimationDirection direction)
    {
        return direction switch
        {
            AnimationDirection.Up => (0, 200),
            AnimationDirection.Down => (0, -200),
            AnimationDirection.Left => (200, 0),
            AnimationDirection.Right => (-200, 0),
            _ => (0, 0)
        };
    }

    private static (double x, double y) GetEndPosition(AnimationDirection direction)
    {
        return direction switch
        {
            AnimationDirection.Up => (0, -200),
            AnimationDirection.Down => (0, 200),
            AnimationDirection.Left => (-200, 0),
            AnimationDirection.Right => (200, 0),
            _ => (0, 0)
        };
    }

    /// <summary>
    /// Immediately hides any visible notification.
    /// </summary>
    public void HideNotifications()
    {
        AchievementContainer.Opacity = 0;
        AchievementTransform.X = 0;
        AchievementTransform.Y = 0;
    }

    /// <summary>
    /// Clears the notification queue.
    /// </summary>
    public void ClearQueue()
    {
        while (_notificationQueue.TryDequeue(out _)) { }
    }

    /// <summary>
    /// Enters position mode, showing the positioning guide and enabling dragging.
    /// </summary>
    public void EnterPositionMode()
    {
        IsPositionMode = true;
        Activate(); // Bring window to front and give it focus for keyboard input
    }

    /// <summary>
    /// Exits position mode, hiding the positioning guide for clean OBS capture.
    /// </summary>
    public void ExitPositionMode()
    {
        IsPositionMode = false;
    }

    private enum NotificationType
    {
        Achievement,
        Mastery
    }

    private class NotificationItem
    {
        public NotificationType Type { get; set; }
        public Achievement? Achievement { get; set; }
        public GameInfo? GameInfo { get; set; }
    }
}

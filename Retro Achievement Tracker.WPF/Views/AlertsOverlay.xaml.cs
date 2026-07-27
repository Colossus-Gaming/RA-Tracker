using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Web.WebView2.Core;
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
            // In edit mode a click belongs to whatever is being dragged inside the window (the panel
            // or the video). Those handlers mark the event handled, but guard here too so a press on
            // empty canvas doesn't yank the whole window mid-edit.
            if (_isEditMode) return;

            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };

        // Toggle position mode with 'P' key
        KeyDown += (s, e) =>
        {
            if (e.Key == Key.P)
                IsPositionMode = !IsPositionMode;
        };

        // Restore saved position, then warm the video surface. WebView2 takes a second or two to
        // spin up; doing it here rather than on the first alert means the first unlock of a session
        // plays its custom video on time instead of arriving late.
        Loaded += async (s, e) =>
        {
            RestoreWindowPosition();
            await PrewarmVideoSurfaceAsync();
        };

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

    #region Custom alert video

    /// <summary>
    /// Host page for the custom-alert video. Alert .webm files are VP8/VP9 with an alpha channel,
    /// which WPF's MediaElement cannot decode, so playback runs in WebView2 (Chromium) — the same
    /// reason the legacy WinForms app used a WebView. Unlike that version, which painted an opaque
    /// chroma-key background for OBS to key out, this page is genuinely transparent and composites
    /// straight onto the desktop.
    /// <para>The page reports playback position back to the host, because the alert panel's in/out
    /// cues are scheduled against the VIDEO's timeline rather than a wall clock — that is what lets
    /// the text land on a specific frame of the animation.</para>
    /// </summary>
    private const string VideoPageHtml = """
        <html><head><style>
        html,body{margin:0;padding:0;background:transparent;overflow:hidden}
        #v{position:absolute;display:none}
        </style></head><body>
        <video id="v" muted playsinline></video>
        <script>
        const v = document.getElementById('v');
        let sx = 1, sleft = 0, stop = 0, raf = 0, last = -1;
        const post = m => window.chrome.webview.postMessage(m);
        function layout() {
          if (!v.videoWidth) return;
          v.style.width = (v.videoWidth * sx) + 'px';
          v.style.left = sleft + 'px';
          v.style.top = stop + 'px';
        }
        function tick() {
          const ms = v.currentTime * 1000;
          if (last < 0 || ms - last >= 40) { last = ms; post('t:' + ms.toFixed(0)); }
          raf = requestAnimationFrame(tick);
        }
        v.addEventListener('loadedmetadata', () => {
          layout();
          post('meta:' + v.videoWidth + 'x' + v.videoHeight + '@' + (v.duration * 1000).toFixed(0));
        });
        v.addEventListener('ended', () => { cancelAnimationFrame(raf); post('ended'); });
        v.addEventListener('error', () => post('error'));
        function playAlert(src, x, y, s) {
          sleft = x; stop = y; sx = s; last = -1;
          v.style.display = 'block';
          const start = () => {
            layout();
            try { v.currentTime = 0; } catch (e) {}
            v.play().catch(() => post('error'));
            cancelAnimationFrame(raf); raf = requestAnimationFrame(tick);
          };
          if (v.getAttribute('data-src') === src && v.readyState >= 1) { start(); }
          else {
            v.setAttribute('data-src', src);
            v.src = src;
            v.addEventListener('loadedmetadata', start, { once: true });
          }
        }
        function hideAlert() {
          cancelAnimationFrame(raf);
          try { v.pause(); } catch (e) {}
          v.loop = false;
          v.style.display = 'none';
        }
        // Edit mode: loop the video so the composition can be judged while dragging, and skip the
        // position reporting entirely (there are no cues to fire while editing).
        function showPreview(src, x, y, s) {
          sleft = x; stop = y; sx = s;
          v.loop = true;
          v.style.display = 'block';
          const start = () => { layout(); v.play().catch(() => {}); };
          if (v.getAttribute('data-src') === src && v.readyState >= 1) { start(); }
          else {
            v.setAttribute('data-src', src);
            v.src = src;
            v.addEventListener('loadedmetadata', start, { once: true });
          }
        }
        // Live geometry update while dragging — no reload, no restart.
        function updateGeometry(x, y, s) { sleft = x; stop = y; sx = s; layout(); }
        // Edit mode input bridge. The page's native window takes all mouse input over the overlay,
        // so it forwards pointer events to the host, which owns the drag maths (it knows where the
        // alert panel and the video are). Only active while editing.
        let editing = false, down = false;
        function setEditMode(on) {
          editing = on; down = false;
          document.body.style.cursor = on ? 'move' : 'default';
        }
        document.addEventListener('mousedown', e => {
          if (!editing) return;
          down = true; post('down:' + e.clientX + ',' + e.clientY);
        });
        document.addEventListener('mousemove', e => {
          if (editing && down) post('move:' + e.clientX + ',' + e.clientY);
        });
        document.addEventListener('mouseup', e => {
          if (!editing) return;
          down = false; post('up:' + e.clientX + ',' + e.clientY);
        });
        document.addEventListener('wheel', e => {
          if (editing) { e.preventDefault(); post('wheel:' + (e.deltaY < 0 ? 1 : -1)); }
        }, { passive: false });
        </script></body></html>
        """;

    private bool _videoSurfaceReady;
    private double _videoNativeWidth, _videoNativeHeight;

    private TaskCompletionSource<bool>? _playbackComplete;
    private Task _outAnimationTask = Task.CompletedTask;
    private int _cueInMs, _cueOutMs, _inSpeedMs, _outSpeedMs;
    private AnimationDirection _cueInDirection, _cueOutDirection;
    private bool _inCueFired, _outCueFired;

    private readonly Dictionary<string, string> _mappedHosts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Brings up the WebView2 surface and exposes the folder holding <paramref name="videoPath"/>
    /// under <paramref name="host"/>, so the video can be referenced by URL without copying it into
    /// the app directory.
    /// <para>The mapping must be registered BEFORE the page is navigated — a document loaded by
    /// <c>NavigateToString</c> will not pick up a virtual host added afterwards, and every media
    /// request 404s. So a mapping change re-navigates the page, exactly as the legacy app did.</para>
    /// </summary>
    private async Task EnsureVideoSurfaceAsync(string host, string videoPath)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(videoPath)) ?? string.Empty;
        var needsMapping = !_mappedHosts.TryGetValue(host, out var mapped)
                           || !string.Equals(mapped, dir, StringComparison.OrdinalIgnoreCase);

        if (_videoSurfaceReady && !needsMapping) return;

        if (!_videoSurfaceReady)
        {
            VideoView.DefaultBackgroundColor = System.Drawing.Color.Transparent;
            await VideoView.EnsureCoreWebView2Async(null);

            var settings = VideoView.CoreWebView2.Settings;
            settings.AreDefaultContextMenusEnabled = false;
            settings.AreDevToolsEnabled = false;
            settings.IsStatusBarEnabled = false;
            settings.IsZoomControlEnabled = false;

            VideoView.CoreWebView2.WebMessageReceived += OnVideoMessage;
            _videoSurfaceReady = true;
        }

        if (needsMapping && Directory.Exists(dir))
        {
            VideoView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                host, dir, CoreWebView2HostResourceAccessKind.Allow);
            _mappedHosts[host] = dir;
            Log($"mapped {host} -> {dir}");
        }

        var navigated = new TaskCompletionSource<bool>();
        void OnNavigationCompleted(object? s, CoreWebView2NavigationCompletedEventArgs e)
            => navigated.TrySetResult(true);

        VideoView.NavigationCompleted += OnNavigationCompleted;
        VideoView.NavigateToString(VideoPageHtml);
        await navigated.Task;
        VideoView.NavigationCompleted -= OnNavigationCompleted;
    }

    /// <summary>
    /// Spins up WebView2 ahead of the first alert when a custom video is configured, so its cold
    /// start doesn't eat into the alert. No-op when custom alerts are off.
    /// </summary>
    private async Task PrewarmVideoSurfaceAsync()
    {
        var (host, path) = ViewModel.CustomAchievementEnabled && File.Exists(ViewModel.CustomAchievementVideoPath)
            ? ("appassets.customachievement", ViewModel.CustomAchievementVideoPath)
            : ViewModel.CustomMasteryEnabled && File.Exists(ViewModel.CustomMasteryVideoPath)
                ? ("appassets.custommastery", ViewModel.CustomMasteryVideoPath)
                : (string.Empty, string.Empty);

        if (host.Length == 0) return;

        try
        {
            await EnsureVideoSurfaceAsync(host, path);
            Log("video surface pre-warmed");
        }
        catch (Exception ex)
        {
            Log($"pre-warm failed ({ex.GetType().Name}: {ex.Message}); will retry on first alert.");
        }
    }

    private async Task PlayCustomNotificationAsync(bool isMastery, string videoPath)
    {
        var host = isMastery ? "appassets.custommastery" : "appassets.customachievement";

        _cueInDirection = isMastery ? ViewModel.MasteryInDirection : ViewModel.AchievementInDirection;
        _cueOutDirection = isMastery ? ViewModel.MasteryOutDirection : ViewModel.AchievementOutDirection;
        _cueInMs = isMastery ? ViewModel.CustomMasteryInTime : ViewModel.CustomAchievementInTime;
        _cueOutMs = isMastery ? ViewModel.CustomMasteryOutTime : ViewModel.CustomAchievementOutTime;
        _inSpeedMs = isMastery ? ViewModel.CustomMasteryInSpeed : ViewModel.CustomAchievementInSpeed;
        _outSpeedMs = isMastery ? ViewModel.CustomMasteryOutSpeed : ViewModel.CustomAchievementOutSpeed;

        var x = isMastery ? ViewModel.CustomMasteryX : ViewModel.CustomAchievementX;
        var y = isMastery ? ViewModel.CustomMasteryY : ViewModel.CustomAchievementY;
        var scale = isMastery ? ViewModel.CustomMasteryScale : ViewModel.CustomAchievementScale;

        await EnsureVideoSurfaceAsync(host, videoPath);

        _inCueFired = false;
        _outCueFired = false;
        _outAnimationTask = Task.CompletedTask;
        _playbackComplete = new TaskCompletionSource<bool>();

        // The panel stays hidden until its in-cue; the video carries the opening beat.
        HideNotifications();

        VideoView.Visibility = Visibility.Visible;

        var src = $"https://{host}/{Uri.EscapeDataString(Path.GetFileName(videoPath))}";
        var script = string.Format(CultureInfo.InvariantCulture,
            "playAlert('{0}',{1},{2},{3})", src, x, y, scale);
        Log($"exec {script}");
        var scriptResult = await VideoView.ExecuteScriptAsync(script);
        Log($"exec result: {scriptResult}");

        await _playbackComplete.Task;
        await _outAnimationTask;

        await HideVideoAsync();
        HideNotifications();
    }

    private async Task HideVideoAsync()
    {
        if (!_videoSurfaceReady) return;
        await VideoView.ExecuteScriptAsync("hideAlert()");
        VideoView.Visibility = Visibility.Collapsed;
    }

    private void OnVideoMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        string message;
        try { message = e.TryGetWebMessageAsString(); }
        catch { return; }

        if (!message.StartsWith("t:", StringComparison.Ordinal)) Log($"page -> {message}");

        if (message is "ended" or "error")
        {
            FinishPlayback();
            return;
        }

        if (message.StartsWith("meta:", StringComparison.Ordinal))
        {
            Log($"video loaded {message[5..]} (native px @ duration ms)");

            // "meta:<w>x<h>@<durationMs>" — the native size is what the scale multiplies, so it's
            // needed to draw the edit-mode drag handle over the video's actual on-screen rect.
            var spec = message[5..];
            var at = spec.IndexOf('@');
            var by = spec.IndexOf('x');
            if (by > 0 && at > by
                && double.TryParse(spec[..by], NumberStyles.Float, CultureInfo.InvariantCulture, out var nw)
                && double.TryParse(spec[(by + 1)..at], NumberStyles.Float, CultureInfo.InvariantCulture, out var nh))
            {
                _videoNativeWidth = nw;
                _videoNativeHeight = nh;
                if (_isEditMode) UpdateVideoEditHandle();
            }
            return;
        }

        if (_isEditMode && TryHandleEditPointerMessage(message)) return;

        if (!message.StartsWith("t:", StringComparison.Ordinal)) return;
        if (double.TryParse(message.AsSpan(2), NumberStyles.Float, CultureInfo.InvariantCulture, out var ms))
        {
            OnVideoPosition(ms);
        }
    }

    /// <summary>
    /// Fires the in/out cues as the video crosses them. Each flag is set before awaiting so a
    /// later position report can't re-trigger the same cue.
    /// </summary>
    private async void OnVideoPosition(double positionMs)
    {
        if (!_inCueFired && positionMs >= _cueInMs)
        {
            _inCueFired = true;
            Log($"IN cue at video {positionMs:F0}ms (cue {_cueInMs}ms, {_cueInDirection}, {_inSpeedMs}ms)");
            await AnimateIn(_cueInDirection, TimeSpan.FromMilliseconds(_inSpeedMs));
            return;
        }

        if (_inCueFired && !_outCueFired && positionMs >= _cueOutMs)
        {
            _outCueFired = true;
            Log($"OUT cue at video {positionMs:F0}ms (cue {_cueOutMs}ms, {_cueOutDirection}, {_outSpeedMs}ms)");
            _outAnimationTask = AnimateOut(_cueOutDirection, TimeSpan.FromMilliseconds(_outSpeedMs));
            await _outAnimationTask;
        }
    }

    /// <summary>
    /// Ends playback. If the video ran shorter than the out-cue, the panel is still on screen, so
    /// the out animation runs here rather than being skipped.
    /// </summary>
    private static void Log(string message)
        => System.Diagnostics.Debug.WriteLine($"[AlertsOverlay] {message}");

    private void FinishPlayback()
    {
        Log($"playback ended (inFired={_inCueFired}, outFired={_outCueFired})");
        if (_inCueFired && !_outCueFired)
        {
            _outCueFired = true;
            _outAnimationTask = AnimateOut(_cueOutDirection, TimeSpan.FromMilliseconds(_outSpeedMs));
        }

        _playbackComplete?.TrySetResult(true);
    }

    #endregion

    #region Edit mode

    private bool _isEditMode;
    private bool _draggingPanel, _draggingVideo;
    private Point _dragStart;
    private double _dragOriginX, _dragOriginY;

    /// <summary>Whether the overlay is in layout-editing mode.</summary>
    public bool IsEditMode => _isEditMode;

    /// <summary>
    /// Enters edit mode: the alert panel is pinned on screen with sample content (no animation, no
    /// auto-hide) and the custom video loops behind it, so both can be dragged into place against
    /// the real composition. Changes are written to settings as they happen.
    /// </summary>
    public async Task EnterEditModeAsync()
    {
        if (_isEditMode) return;
        _isEditMode = true;

        ClearQueue();
        ViewModel.SetSampleAchievementNotification();

        // Pin the panel: cancel any in-flight animation state and show it outright.
        AchievementContainer.BeginAnimation(OpacityProperty, null);
        AchievementTransform.BeginAnimation(TranslateTransform.XProperty, null);
        AchievementTransform.BeginAnimation(TranslateTransform.YProperty, null);
        AchievementTransform.X = 0;
        AchievementTransform.Y = 0;
        AchievementContainer.Opacity = 1;

        var path = ViewModel.CustomAchievementVideoPath;
        if (ViewModel.CustomAchievementEnabled && !string.IsNullOrWhiteSpace(path) && File.Exists(path))
        {
            try
            {
                await EnsureVideoSurfaceAsync("appassets.customachievement", path);
                VideoView.Visibility = Visibility.Visible;

                var src = $"https://appassets.customachievement/{Uri.EscapeDataString(Path.GetFileName(path))}";
                await VideoView.ExecuteScriptAsync(string.Format(CultureInfo.InvariantCulture,
                    "showPreview('{0}',{1},{2},{3})",
                    src, ViewModel.CustomAchievementX, ViewModel.CustomAchievementY, ViewModel.CustomAchievementScale));

                VideoEditHandle.Visibility = Visibility.Visible;
                UpdateVideoEditHandle();
            }
            catch (Exception ex)
            {
                Log($"edit-mode video preview failed: {ex.Message}");
            }
        }

        if (_videoSurfaceReady)
            await VideoView.ExecuteScriptAsync("setEditMode(true)");

        EditInfoChip.Visibility = Visibility.Visible;
        UpdateEditInfo();
        Log("edit mode ON");
    }

    /// <summary>
    /// Leaves edit mode and returns the overlay to its clean capture state.
    /// </summary>
    public async Task ExitEditModeAsync()
    {
        if (!_isEditMode) return;
        _isEditMode = false;

        _draggingPanel = _draggingVideo = false;

        if (_videoSurfaceReady)
            await VideoView.ExecuteScriptAsync("setEditMode(false)");

        VideoEditHandle.Visibility = Visibility.Collapsed;
        EditInfoChip.Visibility = Visibility.Collapsed;
        VideoEditHandle.ReleaseMouseCapture();
        AchievementContainer.ReleaseMouseCapture();

        await HideVideoAsync();
        HideNotifications();

        ViewModel.SaveAlertLayoutSettings();
        ViewModel.SaveCustomAlertSettings();
        Log("edit mode OFF");
    }

    /// <summary>
    /// Positions the video drag handle over the video's on-screen rect (native size x scale, at the
    /// configured offset). Falls back to a nominal box until the video reports its native size.
    /// </summary>
    private void UpdateVideoEditHandle()
    {
        var width = _videoNativeWidth > 0 ? _videoNativeWidth * ViewModel.CustomAchievementScale : 320;
        var height = _videoNativeHeight > 0 ? _videoNativeHeight * ViewModel.CustomAchievementScale : 180;

        Canvas.SetLeft(VideoEditHandle, ViewModel.CustomAchievementX);
        Canvas.SetTop(VideoEditHandle, ViewModel.CustomAchievementY);
        VideoEditHandle.Width = width;
        VideoEditHandle.Height = height;

        UpdateEditInfo();
    }

    /// <summary>
    /// Refreshes the on-screen readout. This is the only reliable cue when the video is scaled past
    /// the window, since the handle's own borders are then off-screen.
    /// </summary>
    private void UpdateEditInfo()
    {
        if (EditInfoText == null) return;

        var videoLine = _videoNativeWidth > 0
            ? string.Format(CultureInfo.InvariantCulture,
                "video  {0,6:F0},{1,6:F0}  x{2:F2}  ({3:F0}x{4:F0})",
                ViewModel.CustomAchievementX, ViewModel.CustomAchievementY, ViewModel.CustomAchievementScale,
                _videoNativeWidth * ViewModel.CustomAchievementScale,
                _videoNativeHeight * ViewModel.CustomAchievementScale)
            : "video  (no custom video configured)";

        var panelLine = string.Format(CultureInfo.InvariantCulture,
            "panel  {0,6:F0},{1,6:F0}  ({2:F0}x{3:F0})",
            ViewModel.AchievementLeft, ViewModel.AchievementTop,
            AchievementContainer.ActualWidth, AchievementContainer.ActualHeight);

        EditInfoText.Text = videoLine + Environment.NewLine + panelLine;
    }

    private async Task PushVideoGeometryAsync()
    {
        if (!_videoSurfaceReady) return;

        await VideoView.ExecuteScriptAsync(string.Format(CultureInfo.InvariantCulture,
            "updateGeometry({0},{1},{2})",
            ViewModel.CustomAchievementX, ViewModel.CustomAchievementY, ViewModel.CustomAchievementScale));
    }

    /// <summary>
    /// Handles the pointer events the video page forwards while editing. Coordinates arrive in CSS
    /// pixels, which match WPF device-independent units, so they map straight onto the canvas.
    /// </summary>
    private bool TryHandleEditPointerMessage(string message)
    {
        var split = message.IndexOf(':');
        if (split < 0) return false;

        var verb = message[..split];
        var payload = message[(split + 1)..];

        if (verb == "wheel")
        {
            if (!int.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dir)) return true;
            ApplyVideoScaleStep(dir);
            return true;
        }

        if (verb is not ("down" or "move" or "up")) return false;

        var comma = payload.IndexOf(',');
        if (comma < 0) return true;
        if (!double.TryParse(payload[..comma], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
            !double.TryParse(payload[(comma + 1)..], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
        {
            return true;
        }

        switch (verb)
        {
            case "down":
                BeginEditDrag(new Point(x, y));
                break;
            case "move":
                UpdateEditDrag(new Point(x, y));
                break;
            case "up":
                EndEditDrag();
                break;
        }

        return true;
    }

    /// <summary>
    /// Starts a drag. A press inside the alert panel moves the panel; anywhere else moves the video,
    /// so the panel always wins where the two overlap.
    /// </summary>
    private void BeginEditDrag(Point point)
    {
        _dragStart = point;

        var panel = new Rect(
            ViewModel.AchievementLeft, ViewModel.AchievementTop,
            AchievementContainer.ActualWidth, AchievementContainer.ActualHeight);

        if (panel.Contains(point))
        {
            _draggingPanel = true;
            _dragOriginX = ViewModel.AchievementLeft;
            _dragOriginY = ViewModel.AchievementTop;
        }
        else
        {
            _draggingVideo = true;
            _dragOriginX = ViewModel.CustomAchievementX;
            _dragOriginY = ViewModel.CustomAchievementY;
        }
    }

    private async void UpdateEditDrag(Point point)
    {
        var dx = point.X - _dragStart.X;
        var dy = point.Y - _dragStart.Y;

        if (_draggingPanel)
        {
            ViewModel.AchievementLeft = _dragOriginX + dx;
            ViewModel.AchievementTop = _dragOriginY + dy;
            UpdateEditInfo();
        }
        else if (_draggingVideo)
        {
            ViewModel.CustomAchievementX = _dragOriginX + dx;
            ViewModel.CustomAchievementY = _dragOriginY + dy;
            UpdateVideoEditHandle();
            await PushVideoGeometryAsync();
        }
    }

    private void EndEditDrag()
    {
        if (_draggingPanel)
        {
            _draggingPanel = false;
            ViewModel.SaveAlertLayoutSettings();
            Log($"panel moved to {ViewModel.AchievementLeft:F0},{ViewModel.AchievementTop:F0}");
        }
        else if (_draggingVideo)
        {
            _draggingVideo = false;
            ViewModel.SaveCustomAlertSettings();
            Log($"video moved to {ViewModel.CustomAchievementX:F0},{ViewModel.CustomAchievementY:F0}");
        }
    }

    private async void ApplyVideoScaleStep(int direction)
    {
        var scale = ViewModel.CustomAchievementScale + (direction > 0 ? 0.05 : -0.05);
        ViewModel.CustomAchievementScale = Math.Clamp(Math.Round(scale, 2), 0.05, 10.0);

        UpdateVideoEditHandle();
        await PushVideoGeometryAsync();
        ViewModel.SaveCustomAlertSettings();
    }

    // The panel handlers below cover the no-custom-video case, where WebView2 is collapsed and WPF
    // receives the mouse normally. With a video showing, the page's forwarded pointer events drive
    // both drags instead (see TryHandleEditPointerMessage).
    private void AchievementContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!_isEditMode) return;

        _draggingPanel = true;
        _dragStart = e.GetPosition(OverlayCanvas);
        _dragOriginX = ViewModel.AchievementLeft;
        _dragOriginY = ViewModel.AchievementTop;
        AchievementContainer.CaptureMouse();
        e.Handled = true;
    }

    private void AchievementContainer_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_draggingPanel) return;

        var now = e.GetPosition(OverlayCanvas);
        ViewModel.AchievementLeft = _dragOriginX + (now.X - _dragStart.X);
        ViewModel.AchievementTop = _dragOriginY + (now.Y - _dragStart.Y);

        UpdateEditInfo();
    }

    private void AchievementContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_draggingPanel) return;

        _draggingPanel = false;
        AchievementContainer.ReleaseMouseCapture();
        ViewModel.SaveAlertLayoutSettings();
        Log($"panel moved to {ViewModel.AchievementLeft:F0},{ViewModel.AchievementTop:F0}");
        e.Handled = true;
    }

    #endregion

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isUpdatingSize) return;
        if (!IsLoaded) return;

        _isUpdatingSize = true;
        ViewModel.WindowWidth = e.NewSize.Width;
        ViewModel.WindowHeight = e.NewSize.Height;
        _isUpdatingSize = false;

        // Persist the new size (debounced by ScheduleSave) so a window sized to fit a scaled custom
        // video survives a restart. Guarded by the !IsLoaded check above so the transient sizes seen
        // while the window is still being laid out never overwrite the saved value.
        ViewModel.SaveAlertLayoutSettings();
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
        // Custom alert: a user-supplied video drives the whole presentation, and the panel's
        // in/out cues are scheduled against its playback position. Falls back to the built-in
        // animation below if the file is missing or playback fails.
        var useCustom = isMastery ? ViewModel.CustomMasteryEnabled : ViewModel.CustomAchievementEnabled;
        var videoPath = isMastery ? ViewModel.CustomMasteryVideoPath : ViewModel.CustomAchievementVideoPath;

        if (useCustom && !string.IsNullOrWhiteSpace(videoPath) && File.Exists(videoPath))
        {
            try
            {
                await PlayCustomNotificationAsync(isMastery, videoPath);
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AlertsOverlay] Custom alert failed ({ex.GetType().Name}: {ex.Message}); using built-in animation.");
                try { await HideVideoAsync(); } catch { /* best effort */ }
            }
        }

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
        // Zero duration (STATIC, or an explicit speed of 0) means snap into place. A zero-length
        // storyboard is unreliable here, so land the final state directly.
        if (duration <= TimeSpan.Zero)
        {
            AchievementTransform.X = 0;
            AchievementTransform.Y = 0;
            AchievementContainer.Opacity = 1;
            return;
        }

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
        if (duration <= TimeSpan.Zero)
        {
            var (snapX, snapY) = GetEndPosition(direction);
            AchievementTransform.X = snapX;
            AchievementTransform.Y = snapY;
            AchievementContainer.Opacity = 0;
            return;
        }

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

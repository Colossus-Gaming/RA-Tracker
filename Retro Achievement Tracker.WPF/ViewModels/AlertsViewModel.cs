using System.Windows;
using System.Windows.Media;
using RATracker.Models;
using RATracker.WPF.Converters;
using RATracker.WPF.Services;

namespace RATracker.WPF.ViewModels;

/// <summary>
/// Animation direction for notifications.
/// </summary>
public enum AnimationDirection
{
    Up,
    Down,
    Left,
    Right,
    Static
}

/// <summary>
/// Where the badge sits relative to the text inside the alert container. This is the lever that
/// changes the container's shape: the default <see cref="Left"/> gives the classic wide banner,
/// while <see cref="Top"/> or <see cref="Bottom"/> produce a tall card and <see cref="Hidden"/>
/// gives a text-only strip.
/// </summary>
public enum BadgePlacement
{
    Left,
    Right,
    Top,
    Bottom,
    Hidden
}

/// <summary>
/// ViewModel for the Alerts Overlay window.
/// Displays achievement unlock and mastery notifications.
/// </summary>
public class AlertsViewModel : ViewModelBase
{
    #region Private Fields

    // Current notification data
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _points = string.Empty;
    private string _badgeUri = string.Empty;
    private bool _isMasteryNotification;

    // Sub-set (Core/Bonus/Specialty/Exclusive/Challenge) visual differentiation
    private AchievementSetType _setType = AchievementSetType.Core;
    private string _setName = string.Empty;

    // Mastery-specific
    private string _masteryAchievements = string.Empty;
    private string _masteryPoints = string.Empty;

    // Animation settings
    private AnimationDirection _achievementInDirection = AnimationDirection.Up;
    private AnimationDirection _achievementOutDirection = AnimationDirection.Down;
    private AnimationDirection _masteryInDirection = AnimationDirection.Left;
    private AnimationDirection _masteryOutDirection = AnimationDirection.Right;
    private double _animationDuration = 5.0;
    private double _inAnimationTime = 0.5;
    private double _outAnimationTime = 0.5;

    // Layout settings
    private double _windowWidth = 550;
    private double _windowHeight = 250;
    private double _notificationWidth = 500;
    private double _notificationHeight;              // 0 = size to content (classic banner)
    private double _containerPadding = 15;
    private BadgePlacement _badgePlacement = BadgePlacement.Left;
    private double _containerCornerRadius = 10;
    private double _badgeSize = 96;
    private double _badgeCornerRadius = 5;
    private double _lineHeight = 4;
    private double _contentSpacing = 15;

    // Position settings
    private double _achievementLeft = 20;
    private double _achievementTop = 20;

    // Font sizes
    private double _titleFontSize = 24;
    private double _descriptionFontSize = 16;
    private double _pointsFontSize = 28;
    private double _masteryInfoFontSize = 18;

    // Simple mode settings
    private FontFamily _simpleFontFamily = new("Segoe UI");
    private Brush _simpleFontColor = Brushes.White;
    private Brush _simpleStrokeColor = Brushes.Black;
    private double _simpleStrokeSize = 2;
    private bool _simpleStrokeEnabled = true;

    // Advanced mode - Title
    private FontFamily _titleFontFamily = new("Segoe UI");
    private Brush _titleColor = Brushes.Gold;
    private Brush _titleStrokeColor = Brushes.Black;
    private double _titleStrokeSize = 2;
    private bool _titleStrokeEnabled = true;

    // Advanced mode - Description
    private FontFamily _descriptionFontFamily = new("Segoe UI");
    private Brush _descriptionColor = Brushes.White;
    private Brush _descriptionStrokeColor = Brushes.Black;
    private double _descriptionStrokeSize = 1;
    private bool _descriptionStrokeEnabled = true;

    // Advanced mode - Points
    private FontFamily _pointsFontFamily = new("Segoe UI");
    private Brush _pointsColor = Brushes.Yellow;
    private Brush _pointsStrokeColor = Brushes.Black;
    private double _pointsStrokeSize = 2;
    private bool _pointsStrokeEnabled = true;

    // Line settings
    private Brush _lineColor = Brushes.Gold;
    private Brush _lineStrokeColor = Brushes.Black;
    private double _lineStrokeSize = 1;
    private bool _lineStrokeEnabled;

    // Window/Container settings
    private Brush _backgroundColor = new SolidColorBrush(Color.FromArgb(220, 40, 40, 40));
    private Brush _borderColor = Brushes.Gold;
    private bool _borderEnabled = true;
    private bool _advancedSettingsEnabled;

    // Custom video paths
    private string _customAchievementVideoPath = string.Empty;
    private string _customMasteryVideoPath = string.Empty;
    private bool _customAchievementEnabled;
    private bool _customMasteryEnabled;

    // Custom-alert geometry + schedule. Mirrors the legacy AlertsController model: the video is
    // placed by an explicit offset (often negative, to bleed a large video off the overlay edges)
    // and scaled relative to its own native width. The in/out animations are then scheduled against
    // the VIDEO's playback position, not a wall clock, so the text lands on a specific frame.
    private double _customAchievementX;
    private double _customAchievementY;
    private double _customAchievementScale = 1.0;
    private int _customAchievementInTime;
    private int _customAchievementOutTime = 5200;
    private int _customAchievementInSpeed;
    private int _customAchievementOutSpeed = 700;

    private double _customMasteryX;
    private double _customMasteryY;
    private double _customMasteryScale = 1.0;
    private int _customMasteryInTime;
    private int _customMasteryOutTime = 5200;
    private int _customMasteryInSpeed;
    private int _customMasteryOutSpeed = 700;

    #endregion

    #region Constructor

    public AlertsViewModel()
    {
        LoadAlertLayoutSettings();
        LoadCustomAlertSettings();
    }

    /// <summary>
    /// Loads the alert container's size/shape from persisted settings.
    /// </summary>
    public void LoadAlertLayoutSettings()
    {
        var settings = SettingsService.Instance.Settings;

        _windowWidth = settings.AlertsWindowWidth;
        _windowHeight = settings.AlertsWindowHeight;
        _achievementLeft = settings.AlertAchievementLeft;
        _achievementTop = settings.AlertAchievementTop;
        _notificationWidth = settings.AlertNotificationWidth;
        _notificationHeight = settings.AlertNotificationHeight;
        _containerPadding = settings.AlertContainerPadding;
        _containerCornerRadius = settings.AlertContainerCornerRadius;
        _badgeSize = settings.AlertBadgeSize;
        _badgeCornerRadius = settings.AlertBadgeCornerRadius;
        _contentSpacing = settings.AlertContentSpacing;
        _badgePlacement = Enum.TryParse<BadgePlacement>(settings.AlertBadgePlacement, ignoreCase: true, out var placement)
            ? placement
            : BadgePlacement.Left;

        OnPropertiesChanged(
            nameof(WindowWidth), nameof(WindowHeight),
            nameof(AchievementLeft), nameof(AchievementTop),
            nameof(NotificationWidth), nameof(NotificationHeight), nameof(NotificationHeightValue),
            nameof(ContainerPadding), nameof(ContainerPaddingThickness),
            nameof(ContainerCornerRadius), nameof(ContainerCornerRadiusValue),
            nameof(BadgeSize), nameof(BadgeCornerRadius), nameof(BadgeCornerRadiusValue),
            nameof(ContentSpacing), nameof(ContentSpacingMargin),
            nameof(BadgePlacement), nameof(BadgeDock), nameof(BadgeVisible), nameof(BadgeMargin));
    }

    /// <summary>
    /// Writes the alert container's size/shape back to settings.
    /// </summary>
    public void SaveAlertLayoutSettings()
    {
        var settings = SettingsService.Instance.Settings;

        settings.AlertsWindowWidth = _windowWidth;
        settings.AlertsWindowHeight = _windowHeight;
        settings.AlertAchievementLeft = _achievementLeft;
        settings.AlertAchievementTop = _achievementTop;
        settings.AlertNotificationWidth = _notificationWidth;
        settings.AlertNotificationHeight = _notificationHeight;
        settings.AlertContainerPadding = _containerPadding;
        settings.AlertContainerCornerRadius = _containerCornerRadius;
        settings.AlertBadgeSize = _badgeSize;
        settings.AlertBadgeCornerRadius = _badgeCornerRadius;
        settings.AlertContentSpacing = _contentSpacing;
        settings.AlertBadgePlacement = _badgePlacement.ToString();

        SettingsService.Instance.ScheduleSave();
    }

    /// <summary>
    /// Restores the container to the stock banner shape.
    /// </summary>
    public void ResetAlertLayoutToDefaults()
    {
        var defaults = new Models.AppSettings();
        var settings = SettingsService.Instance.Settings;

        settings.AlertNotificationWidth = defaults.AlertNotificationWidth;
        settings.AlertNotificationHeight = defaults.AlertNotificationHeight;
        settings.AlertContainerPadding = defaults.AlertContainerPadding;
        settings.AlertContainerCornerRadius = defaults.AlertContainerCornerRadius;
        settings.AlertBadgeSize = defaults.AlertBadgeSize;
        settings.AlertBadgeCornerRadius = defaults.AlertBadgeCornerRadius;
        settings.AlertContentSpacing = defaults.AlertContentSpacing;
        settings.AlertBadgePlacement = defaults.AlertBadgePlacement;

        LoadAlertLayoutSettings();
        SettingsService.Instance.ScheduleSave();
    }

    /// <summary>
    /// Loads the custom-alert configuration from persisted settings. Only the custom-alert block is
    /// restored here; the rest of the alert styling is still session state.
    /// </summary>
    public void LoadCustomAlertSettings()
    {
        var settings = SettingsService.Instance.Settings;

        _customAchievementEnabled = settings.CustomAchievementEnabled;
        _customAchievementVideoPath = settings.CustomAchievementVideoPath;
        _customAchievementX = settings.CustomAchievementX;
        _customAchievementY = settings.CustomAchievementY;
        _customAchievementScale = settings.CustomAchievementScale;
        _customAchievementInTime = settings.CustomAchievementInTime;
        _customAchievementOutTime = settings.CustomAchievementOutTime;
        _customAchievementInSpeed = settings.CustomAchievementInSpeed;
        _customAchievementOutSpeed = settings.CustomAchievementOutSpeed;

        _customMasteryEnabled = settings.CustomMasteryEnabled;
        _customMasteryVideoPath = settings.CustomMasteryVideoPath;
        _customMasteryX = settings.CustomMasteryX;
        _customMasteryY = settings.CustomMasteryY;
        _customMasteryScale = settings.CustomMasteryScale;
        _customMasteryInTime = settings.CustomMasteryInTime;
        _customMasteryOutTime = settings.CustomMasteryOutTime;
        _customMasteryInSpeed = settings.CustomMasteryInSpeed;
        _customMasteryOutSpeed = settings.CustomMasteryOutSpeed;

        // Directions are shared with the built-in alert path; custom alerts simply drive them
        // from their own persisted values.
        _achievementInDirection = ParseDirection(settings.CustomAchievementInDirection, AnimationDirection.Static);
        _achievementOutDirection = ParseDirection(settings.CustomAchievementOutDirection, AnimationDirection.Up);
        _masteryInDirection = ParseDirection(settings.CustomMasteryInDirection, AnimationDirection.Static);
        _masteryOutDirection = ParseDirection(settings.CustomMasteryOutDirection, AnimationDirection.Up);
    }

    /// <summary>
    /// Writes the custom-alert configuration back to settings.
    /// </summary>
    public void SaveCustomAlertSettings()
    {
        var settings = SettingsService.Instance.Settings;

        settings.CustomAchievementEnabled = _customAchievementEnabled;
        settings.CustomAchievementVideoPath = _customAchievementVideoPath;
        settings.CustomAchievementX = _customAchievementX;
        settings.CustomAchievementY = _customAchievementY;
        settings.CustomAchievementScale = _customAchievementScale;
        settings.CustomAchievementInTime = _customAchievementInTime;
        settings.CustomAchievementOutTime = _customAchievementOutTime;
        settings.CustomAchievementInSpeed = _customAchievementInSpeed;
        settings.CustomAchievementOutSpeed = _customAchievementOutSpeed;

        settings.CustomMasteryEnabled = _customMasteryEnabled;
        settings.CustomMasteryVideoPath = _customMasteryVideoPath;
        settings.CustomMasteryX = _customMasteryX;
        settings.CustomMasteryY = _customMasteryY;
        settings.CustomMasteryScale = _customMasteryScale;
        settings.CustomMasteryInTime = _customMasteryInTime;
        settings.CustomMasteryOutTime = _customMasteryOutTime;
        settings.CustomMasteryInSpeed = _customMasteryInSpeed;
        settings.CustomMasteryOutSpeed = _customMasteryOutSpeed;

        settings.CustomAchievementInDirection = _achievementInDirection.ToString();
        settings.CustomAchievementOutDirection = _achievementOutDirection.ToString();
        settings.CustomMasteryInDirection = _masteryInDirection.ToString();
        settings.CustomMasteryOutDirection = _masteryOutDirection.ToString();

        SettingsService.Instance.ScheduleSave();
    }

    /// <summary>
    /// Parses a persisted direction name. Accepts the legacy all-caps spellings ("STATIC", "UP")
    /// used by the previous version's settings so old configurations keep working.
    /// </summary>
    private static AnimationDirection ParseDirection(string? value, AnimationDirection fallback)
        => Enum.TryParse<AnimationDirection>(value, ignoreCase: true, out var parsed) ? parsed : fallback;

    #endregion

    #region Content Properties

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Points
    {
        get => _points;
        set => SetProperty(ref _points, value);
    }

    public string BadgeUri
    {
        get => _badgeUri;
        set => SetProperty(ref _badgeUri, value);
    }

    public bool IsMasteryNotification
    {
        get => _isMasteryNotification;
        set { if (SetProperty(ref _isMasteryNotification, value)) OnPropertyChanged(nameof(SetBadgeVisible)); }
    }

    public string MasteryAchievements
    {
        get => _masteryAchievements;
        set => SetProperty(ref _masteryAchievements, value);
    }

    public string MasteryPoints
    {
        get => _masteryPoints;
        set => SetProperty(ref _masteryPoints, value);
    }

    #endregion

    #region Sub-Set Properties

    /// <summary>
    /// The achievement set type of the current notification. Core keeps the standard look;
    /// non-core sets get a distinct accent border and a corner badge.
    /// </summary>
    public AchievementSetType SetType
    {
        get => _setType;
        set { if (SetProperty(ref _setType, value)) NotifySetVisualChanged(); }
    }

    /// <summary>
    /// The display name of the current notification's achievement set (e.g. "Bonus", "Speedrun Showcase").
    /// </summary>
    public string SetName
    {
        get => _setName;
        set => SetProperty(ref _setName, value);
    }

    /// <summary>
    /// Whether the current notification is from a non-core, recognized subset
    /// (Bonus/Specialty/Exclusive/Challenge). Unknown is treated as core for safety.
    /// </summary>
    public bool IsSubSetNotification =>
        _setType != AchievementSetType.Core && _setType != AchievementSetType.Unknown;

    /// <summary>
    /// The accent color for the current set type. Core/Unknown fall back to the configured BorderColor.
    /// </summary>
    public Brush SetAccentColor => _setType is AchievementSetType.Core or AchievementSetType.Unknown
        ? BorderColor
        : AchievementSetVisuals.AccentBrush(_setType);

    /// <summary>
    /// The border brush actually rendered: the per-set accent for subsets, otherwise the user's BorderColor.
    /// </summary>
    public Brush EffectiveBorderColor => IsSubSetNotification ? SetAccentColor : BorderColor;

    /// <summary>
    /// Whether to show the corner set-type badge (only for non-core achievement notifications).
    /// </summary>
    public bool SetBadgeVisible => IsSubSetNotification && !IsMasteryNotification;

    /// <summary>
    /// The short set-type label shown in the corner badge (e.g. "BONUS", "CHALLENGE").
    /// </summary>
    public string SetBadgeText => IsSubSetNotification ? _setType.ToString().ToUpperInvariant() : string.Empty;

    /// <summary>
    /// The background brush for the corner set-type badge (matches the accent color).
    /// </summary>
    public Brush SetBadgeBackground => SetAccentColor;

    #endregion

    #region Animation Properties

    public AnimationDirection AchievementInDirection
    {
        get => _achievementInDirection;
        set => SetProperty(ref _achievementInDirection, value);
    }

    public AnimationDirection AchievementOutDirection
    {
        get => _achievementOutDirection;
        set => SetProperty(ref _achievementOutDirection, value);
    }

    public AnimationDirection MasteryInDirection
    {
        get => _masteryInDirection;
        set => SetProperty(ref _masteryInDirection, value);
    }

    public AnimationDirection MasteryOutDirection
    {
        get => _masteryOutDirection;
        set => SetProperty(ref _masteryOutDirection, value);
    }

    public double AnimationDuration
    {
        get => _animationDuration;
        set => SetProperty(ref _animationDuration, value);
    }

    public double InAnimationTime
    {
        get => _inAnimationTime;
        set => SetProperty(ref _inAnimationTime, value);
    }

    public double OutAnimationTime
    {
        get => _outAnimationTime;
        set => SetProperty(ref _outAnimationTime, value);
    }

    #endregion

    #region Layout Properties

    public double WindowWidth
    {
        get => _windowWidth;
        set => SetProperty(ref _windowWidth, value);
    }

    public double WindowHeight
    {
        get => _windowHeight;
        set => SetProperty(ref _windowHeight, value);
    }

    public double NotificationWidth
    {
        get => _notificationWidth;
        set => SetProperty(ref _notificationWidth, value);
    }

    /// <summary>
    /// Explicit container height. 0 (the default) means size to content, which is the original
    /// banner behaviour — the container is only as tall as the badge and text need.
    /// </summary>
    public double NotificationHeight
    {
        get => _notificationHeight;
        set { if (SetProperty(ref _notificationHeight, value)) OnPropertyChanged(nameof(NotificationHeightValue)); }
    }

    /// <summary>
    /// Height for binding. WPF reads <see cref="double.NaN"/> as "Auto", so a height of 0 keeps the
    /// container content-sized rather than collapsing it.
    /// </summary>
    public double NotificationHeightValue => _notificationHeight > 0 ? _notificationHeight : double.NaN;

    /// <summary>Inner padding between the container edge and its contents.</summary>
    public double ContainerPadding
    {
        get => _containerPadding;
        set { if (SetProperty(ref _containerPadding, value)) OnPropertyChanged(nameof(ContainerPaddingThickness)); }
    }

    public Thickness ContainerPaddingThickness => new(_containerPadding);

    /// <summary>
    /// Badge position within the container. Drives <see cref="BadgeDock"/>, <see cref="BadgeVisible"/>
    /// and <see cref="BadgeMargin"/>, which between them reshape the layout.
    /// </summary>
    public BadgePlacement BadgePlacement
    {
        get => _badgePlacement;
        set
        {
            if (SetProperty(ref _badgePlacement, value))
            {
                OnPropertiesChanged(nameof(BadgeDock), nameof(BadgeVisible), nameof(BadgeMargin));
            }
        }
    }

    public System.Windows.Controls.Dock BadgeDock => _badgePlacement switch
    {
        BadgePlacement.Right => System.Windows.Controls.Dock.Right,
        BadgePlacement.Top => System.Windows.Controls.Dock.Top,
        BadgePlacement.Bottom => System.Windows.Controls.Dock.Bottom,
        _ => System.Windows.Controls.Dock.Left
    };

    public bool BadgeVisible => _badgePlacement != BadgePlacement.Hidden;

    /// <summary>
    /// Gap between the badge and the text, applied to whichever badge edge faces the text so the
    /// spacing stays correct for every placement.
    /// </summary>
    public Thickness BadgeMargin => _badgePlacement switch
    {
        BadgePlacement.Right => new Thickness(_contentSpacing, 0, 0, 0),
        BadgePlacement.Top => new Thickness(0, 0, 0, _contentSpacing),
        BadgePlacement.Bottom => new Thickness(0, _contentSpacing, 0, 0),
        BadgePlacement.Hidden => default,
        _ => new Thickness(0, 0, _contentSpacing, 0)
    };

    public double ContainerCornerRadius
    {
        get => _containerCornerRadius;
        set { if (SetProperty(ref _containerCornerRadius, value)) OnPropertyChanged(nameof(ContainerCornerRadiusValue)); }
    }

    public CornerRadius ContainerCornerRadiusValue => new(_containerCornerRadius);

    public double BadgeSize
    {
        get => _badgeSize;
        set => SetProperty(ref _badgeSize, value);
    }

    public double BadgeCornerRadius
    {
        get => _badgeCornerRadius;
        set { if (SetProperty(ref _badgeCornerRadius, value)) OnPropertyChanged(nameof(BadgeCornerRadiusValue)); }
    }

    public CornerRadius BadgeCornerRadiusValue => new(_badgeCornerRadius);

    public double LineHeight
    {
        get => _lineHeight;
        set => SetProperty(ref _lineHeight, value);
    }

    public double ContentSpacing
    {
        get => _contentSpacing;
        set
        {
            // The gap now lives on the badge (see BadgeMargin) so it follows the badge around
            // as the placement changes; ContentSpacingMargin is kept for the left-badge case.
            if (SetProperty(ref _contentSpacing, value))
            {
                OnPropertiesChanged(nameof(ContentSpacingMargin), nameof(BadgeMargin));
            }
        }
    }

    public Thickness ContentSpacingMargin => new(_contentSpacing, 0, 0, 0);

    public double AchievementLeft
    {
        get => _achievementLeft;
        set => SetProperty(ref _achievementLeft, value);
    }

    public double AchievementTop
    {
        get => _achievementTop;
        set => SetProperty(ref _achievementTop, value);
    }

    // Mastery reuses the same container as achievements (the XAML binds AchievementLeft/Top for
    // both), so there are deliberately no separate MasteryLeft/MasteryTop properties.

    public double TitleFontSize
    {
        get => _titleFontSize;
        set => SetProperty(ref _titleFontSize, value);
    }

    public double DescriptionFontSize
    {
        get => _descriptionFontSize;
        set => SetProperty(ref _descriptionFontSize, value);
    }

    public double PointsFontSize
    {
        get => _pointsFontSize;
        set => SetProperty(ref _pointsFontSize, value);
    }

    public double MasteryInfoFontSize
    {
        get => _masteryInfoFontSize;
        set => SetProperty(ref _masteryInfoFontSize, value);
    }

    #endregion

    #region Simple Mode Properties

    public FontFamily SimpleFontFamily
    {
        get => _simpleFontFamily;
        set { if (SetProperty(ref _simpleFontFamily, value)) NotifyFontPropertiesChanged(); }
    }

    public Brush SimpleFontColor
    {
        get => _simpleFontColor;
        set { if (SetProperty(ref _simpleFontColor, value)) NotifyColorPropertiesChanged(); }
    }

    public Brush SimpleStrokeColor
    {
        get => _simpleStrokeColor;
        set { if (SetProperty(ref _simpleStrokeColor, value)) NotifyColorPropertiesChanged(); }
    }

    public double SimpleStrokeSize
    {
        get => _simpleStrokeSize;
        set { if (SetProperty(ref _simpleStrokeSize, value)) NotifyColorPropertiesChanged(); }
    }

    public bool SimpleStrokeEnabled
    {
        get => _simpleStrokeEnabled;
        set { if (SetProperty(ref _simpleStrokeEnabled, value)) NotifyColorPropertiesChanged(); }
    }

    #endregion

    #region Advanced Mode Properties

    public FontFamily TitleFontFamily
    {
        get => _advancedSettingsEnabled ? _titleFontFamily : _simpleFontFamily;
        set => SetProperty(ref _titleFontFamily, value);
    }

    public Brush TitleColor
    {
        get => _advancedSettingsEnabled ? _titleColor : _simpleFontColor;
        set => SetProperty(ref _titleColor, value);
    }

    public Brush TitleStrokeColor
    {
        get => _advancedSettingsEnabled ? _titleStrokeColor : _simpleStrokeColor;
        set => SetProperty(ref _titleStrokeColor, value);
    }

    public double TitleStrokeSize
    {
        get => _advancedSettingsEnabled ? _titleStrokeSize : _simpleStrokeSize;
        set => SetProperty(ref _titleStrokeSize, value);
    }

    public bool TitleStrokeEnabled
    {
        get => _advancedSettingsEnabled ? _titleStrokeEnabled : _simpleStrokeEnabled;
        set => SetProperty(ref _titleStrokeEnabled, value);
    }

    public FontFamily DescriptionFontFamily
    {
        get => _advancedSettingsEnabled ? _descriptionFontFamily : _simpleFontFamily;
        set => SetProperty(ref _descriptionFontFamily, value);
    }

    public Brush DescriptionColor
    {
        get => _advancedSettingsEnabled ? _descriptionColor : _simpleFontColor;
        set => SetProperty(ref _descriptionColor, value);
    }

    public Brush DescriptionStrokeColor
    {
        get => _advancedSettingsEnabled ? _descriptionStrokeColor : _simpleStrokeColor;
        set => SetProperty(ref _descriptionStrokeColor, value);
    }

    public double DescriptionStrokeSize
    {
        get => _advancedSettingsEnabled ? _descriptionStrokeSize : _simpleStrokeSize;
        set => SetProperty(ref _descriptionStrokeSize, value);
    }

    public bool DescriptionStrokeEnabled
    {
        get => _advancedSettingsEnabled ? _descriptionStrokeEnabled : _simpleStrokeEnabled;
        set => SetProperty(ref _descriptionStrokeEnabled, value);
    }

    public FontFamily PointsFontFamily
    {
        get => _advancedSettingsEnabled ? _pointsFontFamily : _simpleFontFamily;
        set => SetProperty(ref _pointsFontFamily, value);
    }

    public Brush PointsColor
    {
        get => _advancedSettingsEnabled ? _pointsColor : _simpleFontColor;
        set => SetProperty(ref _pointsColor, value);
    }

    public Brush PointsStrokeColor
    {
        get => _advancedSettingsEnabled ? _pointsStrokeColor : _simpleStrokeColor;
        set => SetProperty(ref _pointsStrokeColor, value);
    }

    public double PointsStrokeSize
    {
        get => _advancedSettingsEnabled ? _pointsStrokeSize : _simpleStrokeSize;
        set => SetProperty(ref _pointsStrokeSize, value);
    }

    public bool PointsStrokeEnabled
    {
        get => _advancedSettingsEnabled ? _pointsStrokeEnabled : _simpleStrokeEnabled;
        set => SetProperty(ref _pointsStrokeEnabled, value);
    }

    #endregion

    #region Line Properties

    public Brush LineColor
    {
        get => _lineColor;
        set => SetProperty(ref _lineColor, value);
    }

    public Brush LineStrokeColor
    {
        get => _lineStrokeColor;
        set => SetProperty(ref _lineStrokeColor, value);
    }

    public double LineStrokeSize
    {
        get => _lineStrokeSize;
        set => SetProperty(ref _lineStrokeSize, value);
    }

    public bool LineStrokeEnabled
    {
        get => _lineStrokeEnabled;
        set => SetProperty(ref _lineStrokeEnabled, value);
    }

    #endregion

    #region Window Properties

    public Brush BackgroundColor
    {
        get => _backgroundColor;
        set => SetProperty(ref _backgroundColor, value);
    }

    public Brush BorderColor
    {
        get => _borderColor;
        set
        {
            if (SetProperty(ref _borderColor, value))
                OnPropertiesChanged(nameof(SetAccentColor), nameof(EffectiveBorderColor), nameof(SetBadgeBackground));
        }
    }

    public bool BorderEnabled
    {
        get => _borderEnabled;
        set => SetProperty(ref _borderEnabled, value);
    }

    public bool AdvancedSettingsEnabled
    {
        get => _advancedSettingsEnabled;
        set
        {
            if (SetProperty(ref _advancedSettingsEnabled, value))
            {
                NotifyFontPropertiesChanged();
                NotifyColorPropertiesChanged();
            }
        }
    }

    public string CustomAchievementVideoPath
    {
        get => _customAchievementVideoPath;
        set => SetProperty(ref _customAchievementVideoPath, value);
    }

    public string CustomMasteryVideoPath
    {
        get => _customMasteryVideoPath;
        set => SetProperty(ref _customMasteryVideoPath, value);
    }

    public bool CustomAchievementEnabled
    {
        get => _customAchievementEnabled;
        set => SetProperty(ref _customAchievementEnabled, value);
    }

    /// <summary>Horizontal offset of the custom achievement video, in px. May be negative.</summary>
    public double CustomAchievementX
    {
        get => _customAchievementX;
        set => SetProperty(ref _customAchievementX, value);
    }

    /// <summary>Vertical offset of the custom achievement video, in px. May be negative.</summary>
    public double CustomAchievementY
    {
        get => _customAchievementY;
        set => SetProperty(ref _customAchievementY, value);
    }

    /// <summary>Multiplier applied to the video's native width. 2.0 renders it at double size.</summary>
    public double CustomAchievementScale
    {
        get => _customAchievementScale;
        set => SetProperty(ref _customAchievementScale, value);
    }

    /// <summary>Video position (ms) at which the achievement panel animates in.</summary>
    public int CustomAchievementInTime
    {
        get => _customAchievementInTime;
        set => SetProperty(ref _customAchievementInTime, value);
    }

    /// <summary>Video position (ms) at which the achievement panel animates out.</summary>
    public int CustomAchievementOutTime
    {
        get => _customAchievementOutTime;
        set => SetProperty(ref _customAchievementOutTime, value);
    }

    /// <summary>Duration (ms) of the achievement in-animation. 0 = snap (used by STATIC).</summary>
    public int CustomAchievementInSpeed
    {
        get => _customAchievementInSpeed;
        set => SetProperty(ref _customAchievementInSpeed, value);
    }

    /// <summary>Duration (ms) of the achievement out-animation.</summary>
    public int CustomAchievementOutSpeed
    {
        get => _customAchievementOutSpeed;
        set => SetProperty(ref _customAchievementOutSpeed, value);
    }

    /// <summary>Horizontal offset of the custom mastery video, in px. May be negative.</summary>
    public double CustomMasteryX
    {
        get => _customMasteryX;
        set => SetProperty(ref _customMasteryX, value);
    }

    /// <summary>Vertical offset of the custom mastery video, in px. May be negative.</summary>
    public double CustomMasteryY
    {
        get => _customMasteryY;
        set => SetProperty(ref _customMasteryY, value);
    }

    /// <summary>Multiplier applied to the mastery video's native width.</summary>
    public double CustomMasteryScale
    {
        get => _customMasteryScale;
        set => SetProperty(ref _customMasteryScale, value);
    }

    /// <summary>Video position (ms) at which the mastery panel animates in.</summary>
    public int CustomMasteryInTime
    {
        get => _customMasteryInTime;
        set => SetProperty(ref _customMasteryInTime, value);
    }

    /// <summary>Video position (ms) at which the mastery panel animates out.</summary>
    public int CustomMasteryOutTime
    {
        get => _customMasteryOutTime;
        set => SetProperty(ref _customMasteryOutTime, value);
    }

    /// <summary>Duration (ms) of the mastery in-animation. 0 = snap (used by STATIC).</summary>
    public int CustomMasteryInSpeed
    {
        get => _customMasteryInSpeed;
        set => SetProperty(ref _customMasteryInSpeed, value);
    }

    /// <summary>Duration (ms) of the mastery out-animation.</summary>
    public int CustomMasteryOutSpeed
    {
        get => _customMasteryOutSpeed;
        set => SetProperty(ref _customMasteryOutSpeed, value);
    }

    public bool CustomMasteryEnabled
    {
        get => _customMasteryEnabled;
        set => SetProperty(ref _customMasteryEnabled, value);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets up an achievement notification.
    /// </summary>
    public void SetAchievementNotification(Achievement achievement)
    {
        IsMasteryNotification = false;
        SetType = achievement.SetType;
        SetName = string.IsNullOrWhiteSpace(achievement.SetName)
            ? achievement.SetType.ToString()
            : achievement.SetName!;
        Title = achievement.Title;
        Description = achievement.Description;
        Points = achievement.Points.ToString();
        BadgeUri = achievement.BadgeUri;
    }

    /// <summary>
    /// Sets up a mastery notification.
    /// </summary>
    public void SetMasteryNotification(GameInfo gameInfo)
    {
        IsMasteryNotification = true;
        // Mastery uses the standard treatment; clear any stale subset accent from a prior alert.
        SetType = AchievementSetType.Core;
        SetName = string.Empty;
        Title = gameInfo.Title;
        MasteryAchievements = $"{gameInfo.AchievementsEarned}/{gameInfo.AchievementsPossible}";
        MasteryPoints = $"{gameInfo.GamePointsEarned:N0} pts";
        BadgeUri = gameInfo.BadgeUri;
    }

    /// <summary>
    /// Sets sample achievement notification for demo/design.
    /// </summary>
    public void SetSampleAchievementNotification()
    {
        IsMasteryNotification = false;
        SetType = AchievementSetType.Core;
        SetName = "Core";
        Title = "Achievement Unlocked!";
        Description = "Complete the first level and begin your adventure!";
        Points = "10";
        BadgeUri = "https://media.retroachievements.org/Badge/00000.png";
    }

    /// <summary>
    /// Sets sample mastery notification for demo/design.
    /// </summary>
    public void SetSampleMasteryNotification()
    {
        IsMasteryNotification = true;
        SetType = AchievementSetType.Core;
        SetName = string.Empty;
        Title = "MASTERED!";
        MasteryAchievements = "24/24";
        MasteryPoints = "400 pts";
        BadgeUri = "https://media.retroachievements.org/Images/000001.png";
    }

    private void NotifyFontPropertiesChanged()
    {
        OnPropertiesChanged(
            nameof(TitleFontFamily),
            nameof(DescriptionFontFamily),
            nameof(PointsFontFamily));
    }

    private void NotifyColorPropertiesChanged()
    {
        OnPropertiesChanged(
            nameof(TitleColor), nameof(TitleStrokeColor), nameof(TitleStrokeSize), nameof(TitleStrokeEnabled),
            nameof(DescriptionColor), nameof(DescriptionStrokeColor), nameof(DescriptionStrokeSize), nameof(DescriptionStrokeEnabled),
            nameof(PointsColor), nameof(PointsStrokeColor), nameof(PointsStrokeSize), nameof(PointsStrokeEnabled));
    }

    private void NotifySetVisualChanged()
    {
        OnPropertiesChanged(
            nameof(IsSubSetNotification),
            nameof(SetAccentColor),
            nameof(EffectiveBorderColor),
            nameof(SetBadgeVisible),
            nameof(SetBadgeText),
            nameof(SetBadgeBackground));
    }

    #endregion
}

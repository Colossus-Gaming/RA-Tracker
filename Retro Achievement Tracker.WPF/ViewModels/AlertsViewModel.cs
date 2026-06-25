using System.Windows;
using System.Windows.Media;
using RATracker.Models;
using RATracker.WPF.Converters;

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
    private double _containerCornerRadius = 10;
    private double _badgeSize = 96;
    private double _badgeCornerRadius = 5;
    private double _lineHeight = 4;
    private double _contentSpacing = 15;

    // Position settings
    private double _achievementLeft = 20;
    private double _achievementTop = 20;
    private double _masteryLeft = 20;
    private double _masteryTop = 20;

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
        set { if (SetProperty(ref _contentSpacing, value)) OnPropertyChanged(nameof(ContentSpacingMargin)); }
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

    public double MasteryLeft
    {
        get => _masteryLeft;
        set => SetProperty(ref _masteryLeft, value);
    }

    public double MasteryTop
    {
        get => _masteryTop;
        set => SetProperty(ref _masteryTop, value);
    }

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
        BadgeUri = "https://media.retroachievements.org/Badge/00001.png";
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

using System.Windows;
using System.Windows.Media;
using RATracker.Models;
using RATracker.WPF.Services;

namespace RATracker.WPF.ViewModels;

/// <summary>
/// ViewModel for the Focus Overlay window.
/// Displays either an achievement or game mastery information.
/// </summary>
public class FocusViewModel : ViewModelBase
{
    #region Private Fields

    private readonly SettingsService _settingsService;
    private bool _isLoadingSettings;

    private string _title = "Achievement Title";
    private string _description = "Complete this challenge to earn the achievement!";
    private string _points = "10";
    private string _badgeUri = string.Empty;
    private bool _isMasteryMode;

    // Layout settings (defaults, will be overwritten by settings)
    private double _windowWidth = 700;
    private double _windowHeight = 175;
    private double _badgeSize = 140;
    private double _badgeCornerRadius = 5;
    private double _containerCornerRadius = 8;
    private double _containerMargin = 5;
    private double _contentSpacing = 15;
    private double _lineHeight = 4;
    private double _lineMargin = 8;

    // Font sizes
    private double _titleFontSize = 26;
    private double _pointsFontSize = 36;
    private double _descriptionFontSize = 16;
    private double _masteryInfoFontSize = 20;

    // Element visibility
    private bool _showBadge = true;
    private bool _showTitle = true;
    private bool _showLine = true;
    private bool _showPoints = true;
    private bool _showDescription = true;

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

    // Window settings
    private Brush _backgroundColor = new SolidColorBrush(Color.FromArgb(200, 50, 50, 50));
    private Brush _borderColor = Brushes.Gold;
    private bool _borderEnabled = true;
    private bool _advancedSettingsEnabled;

    // Mastery-specific fields
    private string _masteryAchievements = string.Empty;
    private string _masteryPoints = string.Empty;

    #endregion

    #region Constructor

    public FocusViewModel()
    {
        _settingsService = SettingsService.Instance;
        LoadSettingsFromService();
    }

    /// <summary>
    /// Loads all Focus layout settings from the SettingsService.
    /// </summary>
    private void LoadSettingsFromService()
    {
        _isLoadingSettings = true;

        try
        {
            var settings = _settingsService.Settings;

            // Window size
            _windowWidth = settings.FocusWindowWidth;
            _windowHeight = settings.FocusWindowHeight;

            // Badge settings
            _badgeSize = settings.FocusBadgeSize;
            _badgeCornerRadius = settings.FocusBadgeCornerRadius;

            // Container settings
            _containerCornerRadius = settings.FocusContainerCornerRadius;
            _containerMargin = settings.FocusContainerMargin;
            _contentSpacing = settings.FocusContentSpacing;

            // Element visibility
            _showBadge = settings.FocusShowBadge;
            _showTitle = settings.FocusShowTitle;
            _showLine = settings.FocusShowLine;
            _showPoints = settings.FocusShowPoints;
            _showDescription = settings.FocusShowDescription;

            // Font sizes
            _titleFontSize = settings.FocusTitleFontSize;
            _pointsFontSize = settings.FocusPointsFontSize;
            _descriptionFontSize = settings.FocusDescriptionFontSize;
            _masteryInfoFontSize = settings.FocusMasteryFontSize;

            // Line settings
            _lineHeight = settings.FocusLineHeight;
            _lineMargin = settings.FocusLineMargin;

            System.Diagnostics.Debug.WriteLine($"[FocusViewModel] Settings loaded - WindowWidth: {_windowWidth}, BadgeSize: {_badgeSize}");
        }
        finally
        {
            _isLoadingSettings = false;
        }
    }

    /// <summary>
    /// Helper method to save settings only when not in the loading phase.
    /// </summary>
    private void SaveSettingIfNotLoading(Action saveAction)
    {
        if (!_isLoadingSettings)
        {
            saveAction();
        }
    }

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

    public bool IsMasteryMode
    {
        get => _isMasteryMode;
        set => SetProperty(ref _isMasteryMode, value);
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

    private string? _currentSetName;
    /// <summary>
    /// Gets or sets the name of the current achievement set (for multi-set games).
    /// Null or empty if the game has only one set.
    /// </summary>
    public string? CurrentSetName
    {
        get => _currentSetName;
        set
        {
            if (SetProperty(ref _currentSetName, value))
            {
                OnPropertyChanged(nameof(HasSetName));
            }
        }
    }

    /// <summary>
    /// Gets whether there is a set name to display.
    /// </summary>
    public bool HasSetName => !string.IsNullOrEmpty(CurrentSetName);

    #endregion

    #region Layout Properties

    public double WindowWidth
    {
        get => _windowWidth;
        set
        {
            if (SetProperty(ref _windowWidth, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusWindowWidth = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public double WindowHeight
    {
        get => _windowHeight;
        set
        {
            if (SetProperty(ref _windowHeight, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusWindowHeight = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public double BadgeSize
    {
        get => _badgeSize;
        set
        {
            if (SetProperty(ref _badgeSize, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusBadgeSize = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public double BadgeCornerRadius
    {
        get => _badgeCornerRadius;
        set
        {
            if (SetProperty(ref _badgeCornerRadius, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusBadgeCornerRadius = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public CornerRadius BadgeCornerRadiusValue => new CornerRadius(BadgeCornerRadius);

    public double ContainerCornerRadius
    {
        get => _containerCornerRadius;
        set
        {
            if (SetProperty(ref _containerCornerRadius, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusContainerCornerRadius = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public CornerRadius ContainerCornerRadiusValue => new CornerRadius(ContainerCornerRadius);

    public double ContainerMargin
    {
        get => _containerMargin;
        set
        {
            if (SetProperty(ref _containerMargin, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusContainerMargin = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public Thickness ContainerMarginValue => new Thickness(ContainerMargin);

    public double ContentSpacing
    {
        get => _contentSpacing;
        set
        {
            if (SetProperty(ref _contentSpacing, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusContentSpacing = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public Thickness ContentSpacingMargin => new Thickness(ContentSpacing, 0, 0, 0);

    public double LineHeight
    {
        get => _lineHeight;
        set
        {
            if (SetProperty(ref _lineHeight, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusLineHeight = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public double LineMargin
    {
        get => _lineMargin;
        set
        {
            if (SetProperty(ref _lineMargin, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusLineMargin = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public Thickness LineMarginValue => new Thickness(0, LineMargin, 0, LineMargin);

    public double TitleFontSize
    {
        get => _titleFontSize;
        set
        {
            if (SetProperty(ref _titleFontSize, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusTitleFontSize = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public double PointsFontSize
    {
        get => _pointsFontSize;
        set
        {
            if (SetProperty(ref _pointsFontSize, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusPointsFontSize = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public double DescriptionFontSize
    {
        get => _descriptionFontSize;
        set
        {
            if (SetProperty(ref _descriptionFontSize, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusDescriptionFontSize = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public double MasteryInfoFontSize
    {
        get => _masteryInfoFontSize;
        set
        {
            if (SetProperty(ref _masteryInfoFontSize, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusMasteryFontSize = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool ShowBadge
    {
        get => _showBadge;
        set
        {
            if (SetProperty(ref _showBadge, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusShowBadge = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool ShowTitle
    {
        get => _showTitle;
        set
        {
            if (SetProperty(ref _showTitle, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusShowTitle = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool ShowLine
    {
        get => _showLine;
        set
        {
            if (SetProperty(ref _showLine, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusShowLine = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool ShowPoints
    {
        get => _showPoints;
        set
        {
            if (SetProperty(ref _showPoints, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusShowPoints = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
    }

    public bool ShowDescription
    {
        get => _showDescription;
        set
        {
            if (SetProperty(ref _showDescription, value))
            {
                SaveSettingIfNotLoading(() =>
                {
                    _settingsService.Settings.FocusShowDescription = value;
                    _settingsService.ScheduleSave();
                });
            }
        }
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

    #region Advanced Mode - Title Properties

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

    #endregion

    #region Advanced Mode - Description Properties

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

    #endregion

    #region Advanced Mode - Points Properties

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
        set => SetProperty(ref _borderColor, value);
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

    #endregion

    #region Methods

    /// <summary>
    /// Sets the focus to display an achievement.
    /// </summary>
    /// <param name="achievement">The achievement to display.</param>
    /// <param name="setName">Optional name of the achievement set (for multi-set games).</param>
    public void SetAchievement(Achievement achievement, string? setName = null)
    {
        IsMasteryMode = false;
        Title = achievement.Title;
        Description = achievement.Description;
        Points = achievement.Points.ToString();
        BadgeUri = achievement.BadgeUri;
        CurrentSetName = setName;
    }

    /// <summary>
    /// Sets the focus to display game mastery information.
    /// </summary>
    /// <param name="gameInfo">The game info to display.</param>
    /// <param name="setName">Optional name of the achievement set (for multi-set mastery).</param>
    public void SetGameMastery(GameInfo gameInfo, string? setName = null)
    {
        IsMasteryMode = true;
        Title = gameInfo.Title;
        MasteryAchievements = $"Cheevos:\n{gameInfo.AchievementsEarned}";
        MasteryPoints = $"Points:\n{gameInfo.GamePointsPossible}";
        BadgeUri = gameInfo.BadgeUri;
        CurrentSetName = setName;
    }

    /// <summary>
    /// Clears the focus display to an empty/idle state (no achievement). Used when the player has
    /// no focusable achievement (e.g. game has 0 locked achievements, or polling has just started).
    /// </summary>
    public void ClearAchievement()
    {
        IsMasteryMode = false;
        Title = string.Empty;
        Description = string.Empty;
        Points = string.Empty;
        BadgeUri = string.Empty;
        CurrentSetName = null;
    }

    /// <summary>
    /// Sets a sample achievement for demo purposes.
    /// </summary>
    public void SetSampleAchievement()
    {
        IsMasteryMode = false;
        Title = "Welcome to the Club";
        Description = "Complete the first level and join the ranks of true retro gamers!";
        Points = "10";
        BadgeUri = "https://media.retroachievements.org/Badge/00001.png";
        CurrentSetName = null;
    }

    /// <summary>
    /// Sets sample mastery data for demo purposes.
    /// </summary>
    public void SetSampleMastery()
    {
        IsMasteryMode = true;
        Title = "Super Mario Bros.";
        MasteryAchievements = "Cheevos:\n24/24";
        MasteryPoints = "Points:\n400";
        BadgeUri = "https://media.retroachievements.org/Images/000001.png";
        CurrentSetName = null;
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

    /// <summary>
    /// Resets all layout properties to their default values and saves to settings.
    /// </summary>
    public void ResetLayoutToDefaults()
    {
        // Reset all values - each setter will save to settings
        WindowWidth = 700;
        WindowHeight = 175;
        BadgeSize = 140;
        BadgeCornerRadius = 5;
        ContainerCornerRadius = 8;
        ContainerMargin = 5;
        ContentSpacing = 15;
        LineHeight = 4;
        LineMargin = 8;
        TitleFontSize = 26;
        PointsFontSize = 36;
        DescriptionFontSize = 16;
        MasteryInfoFontSize = 20;
        ShowBadge = true;
        ShowTitle = true;
        ShowLine = true;
        ShowPoints = true;
        ShowDescription = true;

        NotifyLayoutPropertiesChanged();

        System.Diagnostics.Debug.WriteLine("[FocusViewModel] Layout reset to defaults and saved");
    }

    private void NotifyLayoutPropertiesChanged()
    {
        OnPropertiesChanged(
            nameof(WindowWidth), nameof(WindowHeight),
            nameof(BadgeSize), nameof(BadgeCornerRadius), nameof(BadgeCornerRadiusValue),
            nameof(ContainerCornerRadius), nameof(ContainerCornerRadiusValue),
            nameof(ContainerMargin), nameof(ContainerMarginValue),
            nameof(ContentSpacing), nameof(ContentSpacingMargin),
            nameof(LineHeight), nameof(LineMargin), nameof(LineMarginValue),
            nameof(TitleFontSize), nameof(PointsFontSize), 
            nameof(DescriptionFontSize), nameof(MasteryInfoFontSize),
            nameof(ShowBadge), nameof(ShowTitle), nameof(ShowLine),
            nameof(ShowPoints), nameof(ShowDescription));
    }

    #endregion

    #region Static Helpers

    public static Brush HexToBrush(string hex)
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(hex);
            return new SolidColorBrush(color);
        }
        catch
        {
            return Brushes.White;
        }
    }

    public static string BrushToHex(Brush brush)
    {
        if (brush is SolidColorBrush solidBrush)
        {
            var color = solidBrush.Color;
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        return "#FFFFFF";
    }

    #endregion
}

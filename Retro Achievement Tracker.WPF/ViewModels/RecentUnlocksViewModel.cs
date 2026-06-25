using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using RATracker.Models;

namespace RATracker.WPF.ViewModels;

/// <summary>
/// ViewModel for the Recent Unlocks Overlay window.
/// Displays a list of recently unlocked achievements.
/// </summary>
public class RecentUnlocksViewModel : ViewModelBase
{
    #region Private Fields

    private ObservableCollection<Achievement> _achievements = new();
    private TimeZoneInfo _timezone = TimeZoneInfo.Local;

    // Layout settings
    private double _windowWidth = 450;
    private double _windowHeight = 500;
    private double _containerCornerRadius = 8;
    private double _containerMargin = 5;
    private double _itemSpacing = 10;
    private double _badgeSize = 64;
    private double _badgeCornerRadius = 5;

    // Font sizes
    private double _titleFontSize = 16;
    private double _dateFontSize = 12;
    private double _pointsFontSize = 18;

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

    // Advanced mode - Date
    private FontFamily _dateFontFamily = new("Segoe UI");
    private Brush _dateColor = Brushes.LightGray;
    private Brush _dateStrokeColor = Brushes.Black;
    private double _dateStrokeSize = 1;
    private bool _dateStrokeEnabled = true;

    // Advanced mode - Points
    private FontFamily _pointsFontFamily = new("Segoe UI");
    private Brush _pointsColor = Brushes.Yellow;
    private Brush _pointsStrokeColor = Brushes.Black;
    private double _pointsStrokeSize = 2;
    private bool _pointsStrokeEnabled = true;

    // Line settings
    private Brush _lineColor = Brushes.Gold;
    private double _lineHeight = 2;

    // Window settings
    private Brush _backgroundColor = new SolidColorBrush(Color.FromArgb(200, 50, 50, 50));
    private Brush _borderColor = Brushes.Gold;
    private bool _borderEnabled = true;
    private bool _advancedSettingsEnabled;

    #endregion

    #region Content Properties

    public ObservableCollection<Achievement> Achievements
    {
        get => _achievements;
        set => SetProperty(ref _achievements, value);
    }

    /// <summary>
    /// Gets or sets the timezone used for displaying achievement unlock times.
    /// </summary>
    public TimeZoneInfo Timezone
    {
        get => _timezone;
        set
        {
            if (SetProperty(ref _timezone, value))
            {
                // Force refresh of all achievement date displays
                OnPropertyChanged(nameof(Achievements));
            }
        }
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

    public double ContainerCornerRadius
    {
        get => _containerCornerRadius;
        set { if (SetProperty(ref _containerCornerRadius, value)) OnPropertyChanged(nameof(ContainerCornerRadiusValue)); }
    }

    public CornerRadius ContainerCornerRadiusValue => new(_containerCornerRadius);

    public double ContainerMargin
    {
        get => _containerMargin;
        set { if (SetProperty(ref _containerMargin, value)) OnPropertyChanged(nameof(ContainerMarginValue)); }
    }

    public Thickness ContainerMarginValue => new(_containerMargin);

    public double ItemSpacing
    {
        get => _itemSpacing;
        set { if (SetProperty(ref _itemSpacing, value)) OnPropertyChanged(nameof(ItemSpacingValue)); }
    }

    public Thickness ItemSpacingValue => new(0, 0, 0, _itemSpacing);

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

    public double TitleFontSize
    {
        get => _titleFontSize;
        set => SetProperty(ref _titleFontSize, value);
    }

    public double DateFontSize
    {
        get => _dateFontSize;
        set => SetProperty(ref _dateFontSize, value);
    }

    public double PointsFontSize
    {
        get => _pointsFontSize;
        set => SetProperty(ref _pointsFontSize, value);
    }

    public Brush LineColor
    {
        get => _lineColor;
        set => SetProperty(ref _lineColor, value);
    }

    public double LineHeight
    {
        get => _lineHeight;
        set => SetProperty(ref _lineHeight, value);
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

    #region Advanced Mode - Date Properties

    public FontFamily DateFontFamily
    {
        get => _advancedSettingsEnabled ? _dateFontFamily : _simpleFontFamily;
        set => SetProperty(ref _dateFontFamily, value);
    }

    public Brush DateColor
    {
        get => _advancedSettingsEnabled ? _dateColor : _simpleFontColor;
        set => SetProperty(ref _dateColor, value);
    }

    public Brush DateStrokeColor
    {
        get => _advancedSettingsEnabled ? _dateStrokeColor : _simpleStrokeColor;
        set => SetProperty(ref _dateStrokeColor, value);
    }

    public double DateStrokeSize
    {
        get => _advancedSettingsEnabled ? _dateStrokeSize : _simpleStrokeSize;
        set => SetProperty(ref _dateStrokeSize, value);
    }

    public bool DateStrokeEnabled
    {
        get => _advancedSettingsEnabled ? _dateStrokeEnabled : _simpleStrokeEnabled;
        set => SetProperty(ref _dateStrokeEnabled, value);
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
    /// Sets the list of recent achievements.
    /// </summary>
    public void SetAchievements(IEnumerable<Achievement> achievements)
    {
        Achievements.Clear();
        foreach (var achievement in achievements.OrderByDescending(a => a.DateEarned))
        {
            Achievements.Add(achievement);
        }
    }

    /// <summary>
    /// Adds a new achievement to the top of the list.
    /// </summary>
    public void AddAchievement(Achievement achievement)
    {
        Achievements.Insert(0, achievement);
    }

    /// <summary>
    /// Clears all achievements from the list.
    /// </summary>
    public void ClearAchievements()
    {
        Achievements.Clear();
    }

    /// <summary>
    /// Sets sample data for demo/design purposes.
    /// </summary>
    public void SetSampleData()
    {
        Achievements.Clear();

        var sampleAchievements = new[]
        {
            new Achievement
            {
                Title = "Welcome to the Club",
                Description = "Complete the first level",
                Points = 10,
                BadgeUri = "https://media.retroachievements.org/Badge/00001.png",
                DateEarned = DateTime.Now.AddMinutes(-5)
            },
            new Achievement
            {
                Title = "Speed Demon",
                Description = "Complete a level in under 60 seconds",
                Points = 25,
                BadgeUri = "https://media.retroachievements.org/Badge/00002.png",
                DateEarned = DateTime.Now.AddMinutes(-15)
            },
            new Achievement
            {
                Title = "Coin Collector",
                Description = "Collect 100 coins",
                Points = 5,
                BadgeUri = "https://media.retroachievements.org/Badge/00003.png",
                DateEarned = DateTime.Now.AddMinutes(-30)
            },
            new Achievement
            {
                Title = "Boss Slayer",
                Description = "Defeat the first boss",
                Points = 50,
                BadgeUri = "https://media.retroachievements.org/Badge/00004.png",
                DateEarned = DateTime.Now.AddHours(-1)
            },
            new Achievement
            {
                Title = "Explorer",
                Description = "Find all hidden areas in World 1",
                Points = 15,
                BadgeUri = "https://media.retroachievements.org/Badge/00005.png",
                DateEarned = DateTime.Now.AddHours(-2)
            }
        };

        foreach (var achievement in sampleAchievements)
        {
            Achievements.Add(achievement);
        }
    }

    private void NotifyFontPropertiesChanged()
    {
        OnPropertiesChanged(
            nameof(TitleFontFamily),
            nameof(DateFontFamily),
            nameof(PointsFontFamily));
    }

    private void NotifyColorPropertiesChanged()
    {
        OnPropertiesChanged(
            nameof(TitleColor), nameof(TitleStrokeColor), nameof(TitleStrokeSize), nameof(TitleStrokeEnabled),
            nameof(DateColor), nameof(DateStrokeColor), nameof(DateStrokeSize), nameof(DateStrokeEnabled),
            nameof(PointsColor), nameof(PointsStrokeColor), nameof(PointsStrokeSize), nameof(PointsStrokeEnabled));
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Formats the date earned for display using the specified timezone.
    /// </summary>
    /// <param name="dateEarned">The UTC date/time when the achievement was earned.</param>
    /// <param name="timezone">The timezone to convert to for display.</param>
    /// <returns>A human-readable string representing when the achievement was earned.</returns>
    public static string FormatDateEarned(DateTime? dateEarned, TimeZoneInfo timezone)
    {
        if (!dateEarned.HasValue)
            return string.Empty;

        // Convert UTC time to the target timezone
        DateTime localTime;
        DateTime nowInTimezone;
        
        try
        {
            // Assume dateEarned is in UTC (as it comes from the RA API)
            var utcTime = DateTime.SpecifyKind(dateEarned.Value, DateTimeKind.Utc);
            localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timezone);
            nowInTimezone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timezone);
        }
        catch
        {
            // Fallback to treating as local time if conversion fails
            localTime = dateEarned.Value;
            nowInTimezone = DateTime.Now;
        }

        var timeAgo = nowInTimezone - localTime;

        if (timeAgo.TotalMinutes < 1)
            return "Just now";
        if (timeAgo.TotalMinutes < 60)
            return $"{(int)timeAgo.TotalMinutes}m ago";
        if (timeAgo.TotalHours < 24)
            return $"{(int)timeAgo.TotalHours}h ago";
        if (timeAgo.TotalDays < 7)
            return $"{(int)timeAgo.TotalDays}d ago";

        return localTime.ToString("MMM d, yyyy");
    }

    /// <summary>
    /// Formats the date earned for display using the local timezone.
    /// </summary>
    public static string FormatDateEarned(DateTime? dateEarned)
    {
        return FormatDateEarned(dateEarned, TimeZoneInfo.Local);
    }

    #endregion
}

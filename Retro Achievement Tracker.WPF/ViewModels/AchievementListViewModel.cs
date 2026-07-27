using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using RATracker.Models;

namespace RATracker.WPF.ViewModels;

/// <summary>
/// Represents an achievement item in the achievement list with additional display properties.
/// </summary>
public class AchievementListItem : ViewModelBase
{
    private Achievement _achievement = null!;
    private bool _isUnlocked;
    private bool _justUnlocked;
    private double _opacity = 1.0;

    public Achievement Achievement
    {
        get => _achievement;
        set => SetProperty(ref _achievement, value);
    }

    public bool IsUnlocked
    {
        get => _isUnlocked;
        set { if (SetProperty(ref _isUnlocked, value)) OnPropertyChanged(nameof(BadgeUri)); }
    }

    /// <summary>
    /// Set once when this achievement unlocks live during the session; drives the badge flip +
    /// gold-glow animation. Stays false for achievements already unlocked when the list loaded.
    /// </summary>
    public bool JustUnlocked
    {
        get => _justUnlocked;
        set => SetProperty(ref _justUnlocked, value);
    }

    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }

    public string BadgeUri => IsUnlocked ? Achievement.BadgeUri : GetLockedBadgeUri();
    public string Title => Achievement.Title;
    public string Description => Achievement.Description;
    public int Points => Achievement.Points;
    public int Id => Achievement.Id;

    private string GetLockedBadgeUri()
    {
        // RetroAchievements uses _lock suffix for locked badges
        if (string.IsNullOrEmpty(Achievement.BadgeUri))
            return string.Empty;

        return Achievement.BadgeUri.Replace(".png", "_lock.png");
    }

    /// <summary>
    /// Creates an AchievementListItem from an Achievement.
    /// </summary>
    public static AchievementListItem FromAchievement(Achievement achievement)
    {
        return new AchievementListItem
        {
            Achievement = achievement,
            IsUnlocked = achievement.DateEarned.HasValue
        };
    }
}

/// <summary>
/// ViewModel for the Achievement List Overlay window.
/// Displays all achievements for a game in a grid layout.
/// </summary>
public class AchievementListViewModel : ViewModelBase
{
    #region Private Fields

    private ObservableCollection<AchievementListItem> _achievements = new();

    // Layout settings. Defaults are a clean multiple of the 76px badge tile (BadgeSize 64 + spacing 8 +
    // border 4) plus 32px window chrome: 8 columns -> 8*76+32 = 640, 6 rows -> 6*76+32 = 488. The
    // overlay snaps back to clean multiples on resize (see AchievementListOverlay.Window_SizeChanged).
    private double _windowWidth = 640;
    private double _windowHeight = 488;
    private double _containerCornerRadius = 8;
    private double _containerMargin = 5;
    private double _badgeSize = 64;
    private double _badgeSpacing = 4;
    private double _badgeCornerRadius = 3;

    // Border settings
    private double _unlockedBorderThickness = 2;
    private double _lockedBorderThickness = 2;
    private Brush _unlockedBorderColor = Brushes.Gold;
    private Brush _lockedBorderColor = new SolidColorBrush(Color.FromRgb(39, 39, 39));

    // Window settings
    private Brush _backgroundColor = new SolidColorBrush(Color.FromArgb(200, 30, 30, 30));
    private Brush _borderColor = Brushes.Gold;
    private bool _borderEnabled = true;

    // Tooltip settings
    private FontFamily _tooltipFontFamily = new("Segoe UI");
    private double _tooltipTitleFontSize = 14;
    private double _tooltipDescriptionFontSize = 12;
    private double _tooltipPointsFontSize = 12;

    // Display options
    private bool _autoScroll;
    private bool _showUnlockedFirst = true;

    #endregion

    #region Content Properties

    public ObservableCollection<AchievementListItem> Achievements
    {
        get => _achievements;
        set => SetProperty(ref _achievements, value);
    }

    public int TotalCount => Achievements.Count;
    public int UnlockedCount => Achievements.Count(a => a.IsUnlocked);
    public int LockedCount => Achievements.Count(a => !a.IsUnlocked);

    public string ProgressText => $"{UnlockedCount} / {TotalCount}";

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

    public double BadgeSize
    {
        get => _badgeSize;
        set => SetProperty(ref _badgeSize, value);
    }

    public double BadgeSpacing
    {
        get => _badgeSpacing;
        set { if (SetProperty(ref _badgeSpacing, value)) OnPropertyChanged(nameof(BadgeSpacingValue)); }
    }

    public Thickness BadgeSpacingValue => new(_badgeSpacing);

    public double BadgeCornerRadius
    {
        get => _badgeCornerRadius;
        set { if (SetProperty(ref _badgeCornerRadius, value)) OnPropertyChanged(nameof(BadgeCornerRadiusValue)); }
    }

    public CornerRadius BadgeCornerRadiusValue => new(_badgeCornerRadius);

    public double TotalBadgeSize => BadgeSize + (BadgeSpacing * 2) + (UnlockedBorderThickness * 2);

    #endregion

    #region Border Properties

    public double UnlockedBorderThickness
    {
        get => _unlockedBorderThickness;
        set => SetProperty(ref _unlockedBorderThickness, value);
    }

    public double LockedBorderThickness
    {
        get => _lockedBorderThickness;
        set => SetProperty(ref _lockedBorderThickness, value);
    }

    public Brush UnlockedBorderColor
    {
        get => _unlockedBorderColor;
        set => SetProperty(ref _unlockedBorderColor, value);
    }

    public Brush LockedBorderColor
    {
        get => _lockedBorderColor;
        set => SetProperty(ref _lockedBorderColor, value);
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

    #endregion

    #region Tooltip Properties

    public FontFamily TooltipFontFamily
    {
        get => _tooltipFontFamily;
        set => SetProperty(ref _tooltipFontFamily, value);
    }

    public double TooltipTitleFontSize
    {
        get => _tooltipTitleFontSize;
        set => SetProperty(ref _tooltipTitleFontSize, value);
    }

    public double TooltipDescriptionFontSize
    {
        get => _tooltipDescriptionFontSize;
        set => SetProperty(ref _tooltipDescriptionFontSize, value);
    }

    public double TooltipPointsFontSize
    {
        get => _tooltipPointsFontSize;
        set => SetProperty(ref _tooltipPointsFontSize, value);
    }

    #endregion

    #region Display Options

    public bool AutoScroll
    {
        get => _autoScroll;
        set => SetProperty(ref _autoScroll, value);
    }

    public bool ShowUnlockedFirst
    {
        get => _showUnlockedFirst;
        set
        {
            if (SetProperty(ref _showUnlockedFirst, value))
            {
                SortAchievements();
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets all achievements for the game.
    /// </summary>
    public void SetAchievements(IEnumerable<Achievement> achievements)
    {
        Achievements.Clear();

        foreach (var achievement in achievements)
        {
            Achievements.Add(AchievementListItem.FromAchievement(achievement));
        }

        SortAchievements();
        NotifyProgressChanged();
    }

    /// <summary>
    /// Re-evaluates the unlocked/locked/total counts. Call after an item's IsUnlocked changes live
    /// (a live unlock updates the badge in place and does not re-sort, so the animation isn't disrupted).
    /// </summary>
    public void RefreshProgress() => NotifyProgressChanged();

    /// <summary>
    /// Moves a just-unlocked item to its slot in the current sort order using an in-place Move, so the
    /// item container is preserved and FluidMoveBehavior animates the badges sliding to their new grid
    /// positions. No-op when unlocked-first ordering is off.
    /// </summary>
    public void MoveToSortedPosition(AchievementListItem item)
    {
        if (!ShowUnlockedFirst) return;

        int oldIndex = Achievements.IndexOf(item);
        if (oldIndex < 0) return;

        var sorted = Achievements
            .OrderByDescending(a => a.IsUnlocked)
            .ThenBy(a => a.Achievement.DisplayOrder)
            .ToList();
        int newIndex = sorted.IndexOf(item);
        if (newIndex >= 0 && newIndex != oldIndex)
            Achievements.Move(oldIndex, newIndex);
    }

    /// <summary>
    /// Clears all achievements from the list.
    /// </summary>
    public void ClearAchievements()
    {
        Achievements.Clear();
        NotifyProgressChanged();
    }

    /// <summary>
    /// Sorts achievements based on the ShowUnlockedFirst setting.
    /// Unlocked achievements are sorted by display order, locked by display order.
    /// </summary>
    private void SortAchievements()
    {
        var sorted = ShowUnlockedFirst
            ? Achievements.OrderByDescending(a => a.IsUnlocked)
                         .ThenBy(a => a.Achievement.DisplayOrder)
                         .ToList()
            : Achievements.OrderBy(a => a.Achievement.DisplayOrder)
                         .ToList();

        Achievements.Clear();
        foreach (var item in sorted)
        {
            Achievements.Add(item);
        }
    }

    private void NotifyProgressChanged()
    {
        OnPropertiesChanged(
            nameof(TotalCount),
            nameof(UnlockedCount),
            nameof(LockedCount),
            nameof(ProgressText));
    }

    /// <summary>
    /// Sets sample data for demo/design purposes.
    /// </summary>
    public void SetSampleData()
    {
        Achievements.Clear();

        // Create sample achievements
        for (int i = 1; i <= 20; i++)
        {
            var isUnlocked = i <= 8; // First 8 are unlocked
            var achievement = new Achievement
            {
                Id = i,
                Title = $"Achievement {i}",
                Description = $"This is the description for achievement {i}. Complete the required task to unlock it.",
                Points = (i % 4 + 1) * 5, // 5, 10, 15, 20 points
                BadgeUri = $"https://media.retroachievements.org/Badge/0000{i:D2}.png",
                DateEarned = isUnlocked ? DateTime.Now.AddHours(-i) : null,
                DisplayOrder = i
            };

            Achievements.Add(new AchievementListItem
            {
                Achievement = achievement,
                IsUnlocked = isUnlocked
            });
        }

        SortAchievements();
        NotifyProgressChanged();
    }

    #endregion
}

namespace RATracker.Models;

/// <summary>
/// Represents game information from RetroAchievements.
/// </summary>
public class GameInfo : IComparable<GameInfo>
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;

    private long _consoleId;
    public long ConsoleId
    {
        get => _consoleId;
        set
        {
            _consoleId = value;
            ConsoleName = value switch
            {
                1 => "Sega Genesis",
                2 => "Nintendo 64",
                3 => "Super Nintendo Entertainment System",
                4 => "Nintendo Game Boy",
                5 => "Nintendo Game Boy Advance",
                6 => "Nintendo Game Boy Color",
                7 => "Nintendo Entertainment System",
                8 => "NEC TurboGrafx-16",
                9 => "Sega CD",
                10 => "Sega 32X",
                11 => "Sega Master System",
                12 => "Sony Playstation",
                13 => "Atari Lynx",
                14 => "SNK Neo Geo Pocket",
                15 => "Sega Game Gear",
                16 => "Nintendo GameCube",
                17 => "Atari Jaguar",
                18 => "Nintendo DS",
                19 => "Nintendo Wii",
                20 => "Nintendo Wii U",
                21 => "Sony Playstation 2",
                22 => "Microsoft Xbox",
                23 => "Magnavox Odyssey 2",
                24 => "Nintendo Pokemon Mini",
                25 => "Atari 2600",
                26 => "MS-DOS",
                27 => "Arcade",
                28 => "Nintendo Virtual Boy",
                29 => "Microsoft MSX",
                30 => "Commodore 64",
                31 => "Spectrum ZX81",
                32 => "Tangerine Oric",
                33 => "Sega SG-1000",
                37 => "Amstrad CPC",
                38 => "Apple II",
                39 => "Sega Saturn",
                40 => "Sega Dreamcast",
                41 => "Sony PSP",
                43 => "3DO Interactive Multiplayer",
                44 => "ColecoVision",
                45 => "Mattel Intellivision",
                46 => "GCE Vectrex",
                47 => "PC-8000/8800",
                49 => "NEC PC-FX",
                51 => "Atari 7800",
                53 => "WonderSwan",
                57 => "Fairchild Channel F",
                63 => "Watara Supervision",
                69 => "Mega Duck",
                71 => "Arduboy",
                72 => "WASM-4",
                76 => "NEC TurboGrafx-CD",
                _ => "Unknown Console"
            };
        }
    }

    public string BadgeUri { get; set; } = string.Empty;
    public string ImageTitle { get; set; } = string.Empty;
    public string ImageIngame { get; set; } = string.Empty;
    public string ImageBoxArt { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string Developer { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Released { get; set; } = string.Empty;
    public string ConsoleName { get; set; } = string.Empty;

    /// <summary>
    /// The achievement sets for this game (V2 API).
    /// When populated, contains all achievement sets (core, bonus, etc.).
    /// </summary>
    public List<AchievementSet> AchievementSets { get; set; } = new();

    /// <summary>
    /// Whether this game has multiple achievement sets.
    /// </summary>
    public bool HasMultipleSets => AchievementSets?.Count > 1;

    /// <summary>
    /// The core achievement set (primary set required for mastery).
    /// </summary>
    public AchievementSet? CoreSet => AchievementSets?.FirstOrDefault(s => s.IsCore);

    /// <summary>
    /// The currently selected achievement set for display/tracking.
    /// Defaults to core set if available, otherwise first set.
    /// </summary>
    public AchievementSet? SelectedSet { get; set; }

    /// <summary>
    /// Gets the currently active achievement set (selected or core or first).
    /// </summary>
    public AchievementSet? ActiveSet => SelectedSet ?? CoreSet ?? AchievementSets?.FirstOrDefault();

    /// <summary>
    /// The achievements for this game.
    /// When AchievementSets is populated, returns achievements from the active set.
    /// Otherwise, returns the direct Achievements list (V1 API compatibility).
    /// </summary>
    public List<Achievement> Achievements
    {
        get
        {
            // If we have achievement sets, return from the active set
            if (AchievementSets?.Count > 0)
            {
                return ActiveSet?.Achievements ?? new List<Achievement>();
            }
            // Otherwise return the legacy direct list
            return _achievements;
        }
        set => _achievements = value;
    }
    private List<Achievement> _achievements = new();

    /// <summary>
    /// Gets all achievements across all sets (for total game progress).
    /// </summary>
    public List<Achievement> AllAchievements
    {
        get
        {
            if (AchievementSets?.Count > 0)
            {
                return AchievementSets.SelectMany(s => s.Achievements).ToList();
            }
            return _achievements;
        }
    }

    public int AchievementsEarned => Achievements?.Count(x => x.DateEarned.HasValue) ?? 0;

    public int AchievementsPossible => Achievements?.Count ?? 0;

    public int GameTruePointsPossible => Achievements?.Sum(x => x.TrueRatio) ?? 0;

    public int GameTruePointsEarned => Achievements?.Where(x => x.DateEarned.HasValue).Sum(x => x.TrueRatio) ?? 0;

    public int GamePointsPossible => Achievements?.Sum(x => x.Points) ?? 0;

    public int GamePointsEarned => Achievements?.Where(x => x.DateEarned.HasValue).Sum(x => x.Points) ?? 0;

    public string PercentComplete
    {
        get
        {
            if (AchievementsPossible == 0) return "0.00";
            return (AchievementsEarned / (float)AchievementsPossible * 100f).ToString("0.00");
        }
    }

    /// <summary>
    /// Total achievements across all sets (for overall game completion tracking).
    /// </summary>
    public int TotalAchievementsAllSets => AllAchievements?.Count ?? 0;

    /// <summary>
    /// Total achievements earned across all sets.
    /// </summary>
    public int TotalAchievementsEarnedAllSets => AllAchievements?.Count(x => x.DateEarned.HasValue) ?? 0;

    public DateTime? LastPlayed { get; set; }

    public int CompareTo(GameInfo? other)
    {
        if (other == null || !other.LastPlayed.HasValue)
        {
            return 1;
        }
        else if (!LastPlayed.HasValue)
        {
            return -1;
        }
        return other.LastPlayed.Value.CompareTo(LastPlayed.Value);
    }
}

/// <summary>
/// Represents a claim on a game's achievement set.
/// </summary>
public class Claim
{
    public string User { get; set; } = string.Empty;
    public int SetType { get; set; }
    public int ClaimType { get; set; }
    public string Created { get; set; } = string.Empty;
    public string Expiration { get; set; } = string.Empty;
}

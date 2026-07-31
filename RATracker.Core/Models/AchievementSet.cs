namespace RATracker.Models;

/// <summary>
/// Represents an achievement set type.
/// </summary>
public enum AchievementSetType
{
    /// <summary>
    /// Core/Base achievement set - required for mastery.
    /// </summary>
    Core = 0,

    /// <summary>
    /// Bonus achievement set - optional achievements.
    /// </summary>
    Bonus = 1,

    /// <summary>
    /// Specialty achievement set - special challenges.
    /// </summary>
    Specialty = 2,

    /// <summary>
    /// Exclusive achievement set - loads in isolation, incompatible with other sets.
    /// </summary>
    Exclusive = 3,

    /// <summary>
    /// Challenge achievement set - no patched ROM, opt-out by default
    /// (e.g. "A Button Challenge", "Speedrun Showcase").
    /// </summary>
    Challenge = 4,

    /// <summary>
    /// Unknown or unrecognized set type.
    /// </summary>
    Unknown = -1
}

/// <summary>
/// Represents an achievement set from RetroAchievements V2 API.
/// A game can have multiple achievement sets (core, bonus, specialty, etc.).
/// </summary>
public class AchievementSet
{
    /// <summary>
    /// The unique identifier for this achievement set.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The ID of the game this achievement set belongs to.
    /// </summary>
    public long GameId { get; set; }

    /// <summary>
    /// The name/title of this achievement set.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The type of achievement set (core, bonus, specialty, etc.).
    /// </summary>
    public AchievementSetType SetType { get; set; } = AchievementSetType.Core;

    /// <summary>
    /// A user-facing label for the set: its name when present, otherwise the set type
    /// (the API leaves the core set's title null, so fall back to "Core").
    /// </summary>
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? SetType.ToString() : Name;

    /// <summary>
    /// A short, uppercase set-type label for badges (e.g. "CORE", "BONUS", "CHALLENGE").
    /// </summary>
    public string TypeLabel => SetType.ToString().ToUpperInvariant();

    /// <summary>
    /// The badge/icon URL for this set. A subset has its own badge, distinct from the game badge.
    /// </summary>
    public string BadgeUrl { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the core/base achievement set.
    /// </summary>
    public bool IsCore => SetType == AchievementSetType.Core;

    /// <summary>
    /// Whether this is a bonus achievement set.
    /// </summary>
    public bool IsBonus => SetType == AchievementSetType.Bonus;

    /// <summary>
    /// The achievements in this set.
    /// </summary>
    public List<Achievement> Achievements { get; set; } = new();

    /// <summary>
    /// The total number of achievements in this set.
    /// </summary>
    public int AchievementCount => Achievements?.Count ?? 0;

    /// <summary>
    /// The number of achievements earned by the user in this set.
    /// </summary>
    public int AchievementsEarned => Achievements?.Count(a => a.DateEarned.HasValue) ?? 0;

    /// <summary>
    /// The total points available in this set.
    /// </summary>
    public int PointsTotal => Achievements?.Sum(a => a.Points) ?? 0;

    /// <summary>
    /// The points earned by the user in this set.
    /// </summary>
    public int PointsEarned => Achievements?.Where(a => a.DateEarned.HasValue).Sum(a => a.Points) ?? 0;

    /// <summary>
    /// The total true points (weighted) available in this set.
    /// </summary>
    public int TruePointsTotal => Achievements?.Sum(a => a.TrueRatio) ?? 0;

    /// <summary>
    /// The true points (weighted) earned by the user in this set.
    /// </summary>
    public int TruePointsEarned => Achievements?.Where(a => a.DateEarned.HasValue).Sum(a => a.TrueRatio) ?? 0;

    /// <summary>
    /// The completion percentage for this set.
    /// </summary>
    public string PercentComplete
    {
        get
        {
            if (AchievementCount == 0) return "0.00";
            return (AchievementsEarned / (float)AchievementCount * 100f).ToString("0.00");
        }
    }

    /// <summary>
    /// Parses a set type from a string or integer value.
    /// </summary>
    public static AchievementSetType ParseSetType(object? value)
    {
        if (value == null) return AchievementSetType.Core;

        // Handle string values
        if (value is string strValue)
        {
            return strValue.ToLowerInvariant() switch
            {
                "core" or "base" => AchievementSetType.Core,
                "bonus" => AchievementSetType.Bonus,
                "specialty" or "special" => AchievementSetType.Specialty,
                "exclusive" => AchievementSetType.Exclusive,
                "challenge" => AchievementSetType.Challenge,
                _ => int.TryParse(strValue, out var intVal) ? ParseSetType(intVal) : AchievementSetType.Unknown
            };
        }

        // Handle integer values
        if (value is int intValue || (value is long longValue && (intValue = (int)longValue) == intValue))
        {
            return intValue switch
            {
                0 => AchievementSetType.Core,
                1 => AchievementSetType.Bonus,
                2 => AchievementSetType.Specialty,
                3 => AchievementSetType.Exclusive,
                4 => AchievementSetType.Challenge,
                _ => AchievementSetType.Unknown
            };
        }

        return AchievementSetType.Unknown;
    }
}

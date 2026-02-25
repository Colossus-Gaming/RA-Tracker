namespace RATracker.Models;

/// <summary>
/// Represents a user's progress on a specific game.
/// </summary>
public class UserGameProgress
{
    /// <summary>
    /// The user's identifier (ULID or username).
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The game ID.
    /// </summary>
    public long GameId { get; set; }

    /// <summary>
    /// The game title.
    /// </summary>
    public string GameTitle { get; set; } = string.Empty;

    /// <summary>
    /// The console/system name.
    /// </summary>
    public string ConsoleName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of achievements in the game (for the active set).
    /// </summary>
    public int TotalAchievements { get; set; }

    /// <summary>
    /// Number of achievements earned by the user.
    /// </summary>
    public int EarnedAchievements { get; set; }

    /// <summary>
    /// Total points available in the game.
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// Points earned by the user.
    /// </summary>
    public int EarnedPoints { get; set; }

    /// <summary>
    /// Total true points (weighted) available.
    /// </summary>
    public int TotalTruePoints { get; set; }

    /// <summary>
    /// True points earned by the user.
    /// </summary>
    public int EarnedTruePoints { get; set; }

    /// <summary>
    /// When the user last played this game.
    /// </summary>
    public DateTime? LastPlayed { get; set; }

    /// <summary>
    /// The achievements in this game with progress data.
    /// </summary>
    public List<Achievement> Achievements { get; set; } = new();

    /// <summary>
    /// Achievement sets with progress data (for multi-set games).
    /// </summary>
    public List<AchievementSetProgress> AchievementSets { get; set; } = new();

    /// <summary>
    /// Whether this game has multiple achievement sets.
    /// </summary>
    public bool HasMultipleSets => AchievementSets.Count > 1;

    /// <summary>
    /// The completion percentage for this game.
    /// </summary>
    public double CompletionPercentage
    {
        get
        {
            if (TotalAchievements == 0) return 0;
            return (double)EarnedAchievements / TotalAchievements * 100;
        }
    }

    /// <summary>
    /// Formatted completion percentage string.
    /// </summary>
    public string CompletionPercentageFormatted => CompletionPercentage.ToString("0.00");

    /// <summary>
    /// Whether the game is fully completed (mastered).
    /// </summary>
    public bool IsMastered => TotalAchievements > 0 && EarnedAchievements == TotalAchievements;

    /// <summary>
    /// The achievements that have been unlocked.
    /// </summary>
    public List<Achievement> UnlockedAchievements => Achievements.Where(a => a.DateEarned.HasValue).ToList();

    /// <summary>
    /// The achievements that are still locked.
    /// </summary>
    public List<Achievement> LockedAchievements => Achievements.Where(a => !a.DateEarned.HasValue).ToList();

    /// <summary>
    /// Creates a new UserGameProgress instance.
    /// </summary>
    public UserGameProgress() { }

    /// <summary>
    /// Creates a UserGameProgress from a GameInfo instance.
    /// </summary>
    public static UserGameProgress FromGameInfo(GameInfo gameInfo, string userId)
    {
        var progress = new UserGameProgress
        {
            UserId = userId,
            GameId = gameInfo.Id,
            GameTitle = gameInfo.Title,
            ConsoleName = gameInfo.ConsoleName,
            TotalAchievements = gameInfo.AchievementsPossible,
            EarnedAchievements = gameInfo.AchievementsEarned,
            TotalPoints = gameInfo.GamePointsPossible,
            EarnedPoints = gameInfo.GamePointsEarned,
            TotalTruePoints = gameInfo.GameTruePointsPossible,
            EarnedTruePoints = gameInfo.GameTruePointsEarned,
            LastPlayed = gameInfo.LastPlayed,
            Achievements = gameInfo.Achievements.ToList()
        };

        // Map achievement sets if present
        if (gameInfo.AchievementSets.Count > 0)
        {
            foreach (var set in gameInfo.AchievementSets)
            {
                progress.AchievementSets.Add(AchievementSetProgress.FromAchievementSet(set));
            }
        }

        return progress;
    }
}

/// <summary>
/// Represents progress within a specific achievement set.
/// </summary>
public class AchievementSetProgress
{
    /// <summary>
    /// The achievement set ID.
    /// </summary>
    public long SetId { get; set; }

    /// <summary>
    /// The achievement set name.
    /// </summary>
    public string SetName { get; set; } = string.Empty;

    /// <summary>
    /// The type of achievement set.
    /// </summary>
    public AchievementSetType SetType { get; set; }

    /// <summary>
    /// Whether this is the core set.
    /// </summary>
    public bool IsCore => SetType == AchievementSetType.Core;

    /// <summary>
    /// Total achievements in this set.
    /// </summary>
    public int TotalAchievements { get; set; }

    /// <summary>
    /// Achievements earned in this set.
    /// </summary>
    public int EarnedAchievements { get; set; }

    /// <summary>
    /// Total points in this set.
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// Points earned in this set.
    /// </summary>
    public int EarnedPoints { get; set; }

    /// <summary>
    /// Completion percentage for this set.
    /// </summary>
    public double CompletionPercentage
    {
        get
        {
            if (TotalAchievements == 0) return 0;
            return (double)EarnedAchievements / TotalAchievements * 100;
        }
    }

    /// <summary>
    /// Whether this set is fully completed.
    /// </summary>
    public bool IsCompleted => TotalAchievements > 0 && EarnedAchievements == TotalAchievements;

    /// <summary>
    /// Creates an AchievementSetProgress from an AchievementSet.
    /// </summary>
    public static AchievementSetProgress FromAchievementSet(AchievementSet set)
    {
        return new AchievementSetProgress
        {
            SetId = set.Id,
            SetName = set.Name,
            SetType = set.SetType,
            TotalAchievements = set.AchievementCount,
            EarnedAchievements = set.AchievementsEarned,
            TotalPoints = set.PointsTotal,
            EarnedPoints = set.PointsEarned
        };
    }
}

/// <summary>
/// Represents a recently played game with basic progress info.
/// </summary>
public class RecentlyPlayedGame
{
    /// <summary>
    /// The game ID.
    /// </summary>
    public long GameId { get; set; }

    /// <summary>
    /// The game title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The console/system name.
    /// </summary>
    public string ConsoleName { get; set; } = string.Empty;

    /// <summary>
    /// Badge/icon URL for the game.
    /// </summary>
    public string BadgeUrl { get; set; } = string.Empty;

    /// <summary>
    /// When the user last played this game.
    /// </summary>
    public DateTime? LastPlayed { get; set; }

    /// <summary>
    /// Total achievements in the game.
    /// </summary>
    public int TotalAchievements { get; set; }

    /// <summary>
    /// Achievements earned by the user.
    /// </summary>
    public int EarnedAchievements { get; set; }

    /// <summary>
    /// Completion percentage.
    /// </summary>
    public double CompletionPercentage
    {
        get
        {
            if (TotalAchievements == 0) return 0;
            return (double)EarnedAchievements / TotalAchievements * 100;
        }
    }
}

/// <summary>
/// Represents a recently unlocked achievement.
/// </summary>
public class RecentAchievement
{
    /// <summary>
    /// The achievement ID.
    /// </summary>
    public int AchievementId { get; set; }

    /// <summary>
    /// The achievement title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The achievement description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Points value of the achievement.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// True points (weighted) value.
    /// </summary>
    public int TruePoints { get; set; }

    /// <summary>
    /// Badge URL for the achievement.
    /// </summary>
    public string BadgeUrl { get; set; } = string.Empty;

    /// <summary>
    /// The game ID this achievement belongs to.
    /// </summary>
    public long GameId { get; set; }

    /// <summary>
    /// The game title.
    /// </summary>
    public string GameTitle { get; set; } = string.Empty;

    /// <summary>
    /// When the achievement was unlocked.
    /// </summary>
    public DateTime? EarnedAt { get; set; }

    /// <summary>
    /// Whether this was unlocked in hardcore mode.
    /// </summary>
    public bool IsHardcore { get; set; }

    /// <summary>
    /// Converts to an Achievement model.
    /// </summary>
    public Achievement ToAchievement()
    {
        return new Achievement
        {
            Id = AchievementId,
            Title = Title,
            Description = Description,
            Points = Points,
            TrueRatio = TruePoints,
            BadgeUri = BadgeUrl,
            GameId = (int)GameId,
            GameTitle = GameTitle,
            DateEarned = EarnedAt
        };
    }
}

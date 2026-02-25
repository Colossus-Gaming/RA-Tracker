namespace RATracker.Models;

/// <summary>
/// Represents an achievement unlock event for notification purposes.
/// </summary>
public class AchievementUnlockEvent
{
    /// <summary>
    /// The unlocked achievement.
    /// </summary>
    public Achievement Achievement { get; set; } = null!;

    /// <summary>
    /// The game the achievement belongs to.
    /// </summary>
    public long GameId { get; set; }

    /// <summary>
    /// The game title.
    /// </summary>
    public string GameTitle { get; set; } = string.Empty;

    /// <summary>
    /// When the achievement was unlocked.
    /// </summary>
    public DateTime UnlockedAt { get; set; }

    /// <summary>
    /// Whether this was unlocked in hardcore mode.
    /// </summary>
    public bool IsHardcore { get; set; }

    /// <summary>
    /// The user who unlocked the achievement.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The achievement set ID (if multi-set game).
    /// </summary>
    public long? AchievementSetId { get; set; }

    /// <summary>
    /// The achievement set name (if multi-set game).
    /// </summary>
    public string? AchievementSetName { get; set; }

    /// <summary>
    /// Whether this unlock resulted in game mastery (all achievements unlocked).
    /// </summary>
    public bool TriggeredMastery { get; set; }

    /// <summary>
    /// Whether this unlock resulted in set completion (all achievements in set unlocked).
    /// </summary>
    public bool TriggeredSetCompletion { get; set; }

    /// <summary>
    /// Creates an AchievementUnlockEvent from an Achievement.
    /// </summary>
    public static AchievementUnlockEvent FromAchievement(Achievement achievement, string userId)
    {
        return new AchievementUnlockEvent
        {
            Achievement = achievement,
            GameId = achievement.GameId,
            GameTitle = achievement.GameTitle,
            UnlockedAt = achievement.DateEarned ?? DateTime.UtcNow,
            UserId = userId
        };
    }
}

/// <summary>
/// Result of comparing current progress with previous state to detect unlocks.
/// </summary>
public class UnlockDetectionResult
{
    /// <summary>
    /// The newly unlocked achievements detected.
    /// </summary>
    public List<AchievementUnlockEvent> NewUnlocks { get; set; } = new();

    /// <summary>
    /// Whether any new unlocks were detected.
    /// </summary>
    public bool HasNewUnlocks => NewUnlocks.Count > 0;

    /// <summary>
    /// Whether the current progress shows game mastery.
    /// </summary>
    public bool GameMastered { get; set; }

    /// <summary>
    /// Whether game mastery was just achieved (wasn't mastered before).
    /// </summary>
    public bool JustMastered { get; set; }

    /// <summary>
    /// Achievement sets that were just completed (if multi-set game).
    /// </summary>
    public List<AchievementSetProgress> CompletedSets { get; set; } = new();

    /// <summary>
    /// Total points earned from new unlocks.
    /// </summary>
    public int PointsEarned => NewUnlocks.Sum(u => u.Achievement.Points);

    /// <summary>
    /// Total true points earned from new unlocks.
    /// </summary>
    public int TruePointsEarned => NewUnlocks.Sum(u => u.Achievement.TrueRatio);
}

/// <summary>
/// State tracker for detecting achievement unlocks between polls.
/// </summary>
public class ProgressStateTracker
{
    private readonly Dictionary<long, HashSet<int>> _previousUnlockedByGame = new();
    private readonly Dictionary<long, bool> _previousMasteryByGame = new();
    private readonly Dictionary<long, HashSet<long>> _previousCompletedSetsByGame = new();

    /// <summary>
    /// Gets or sets the previous game ID being tracked.
    /// </summary>
    public long? PreviousGameId { get; set; }

    /// <summary>
    /// Updates the tracker with current progress and returns detected changes.
    /// </summary>
    /// <param name="progress">The current user game progress.</param>
    /// <returns>Detection result with any new unlocks.</returns>
    public UnlockDetectionResult DetectChanges(UserGameProgress progress)
    {
        var result = new UnlockDetectionResult();
        var gameId = progress.GameId;

        // Get previous state for this game
        if (!_previousUnlockedByGame.TryGetValue(gameId, out var previousUnlocked))
        {
            previousUnlocked = new HashSet<int>();
        }

        if (!_previousMasteryByGame.TryGetValue(gameId, out var wasMastered))
        {
            wasMastered = false;
        }

        if (!_previousCompletedSetsByGame.TryGetValue(gameId, out var previousCompletedSets))
        {
            previousCompletedSets = new HashSet<long>();
        }

        // Detect new unlocks
        var currentUnlocked = new HashSet<int>();
        foreach (var achievement in progress.UnlockedAchievements)
        {
            currentUnlocked.Add(achievement.Id);

            if (!previousUnlocked.Contains(achievement.Id))
            {
                var unlockEvent = AchievementUnlockEvent.FromAchievement(achievement, progress.UserId);
                result.NewUnlocks.Add(unlockEvent);
            }
        }

        // Detect mastery
        result.GameMastered = progress.IsMastered;
        result.JustMastered = progress.IsMastered && !wasMastered;

        // Detect set completions (for multi-set games)
        if (progress.HasMultipleSets)
        {
            foreach (var setProgress in progress.AchievementSets)
            {
                if (setProgress.IsCompleted && !previousCompletedSets.Contains(setProgress.SetId))
                {
                    result.CompletedSets.Add(setProgress);
                }
            }
        }

        // Mark mastery/set completion on unlock events
        if (result.HasNewUnlocks)
        {
            var lastUnlock = result.NewUnlocks.Last();
            lastUnlock.TriggeredMastery = result.JustMastered;

            if (result.CompletedSets.Count > 0)
            {
                lastUnlock.TriggeredSetCompletion = true;
                var completedSet = result.CompletedSets.First();
                lastUnlock.AchievementSetId = completedSet.SetId;
                lastUnlock.AchievementSetName = completedSet.SetName;
            }
        }

        // Update state
        _previousUnlockedByGame[gameId] = currentUnlocked;
        _previousMasteryByGame[gameId] = progress.IsMastered;
        _previousCompletedSetsByGame[gameId] = new HashSet<long>(
            progress.AchievementSets.Where(s => s.IsCompleted).Select(s => s.SetId));
        PreviousGameId = gameId;

        return result;
    }

    /// <summary>
    /// Resets the tracker state for a specific game.
    /// </summary>
    public void ResetGame(long gameId)
    {
        _previousUnlockedByGame.Remove(gameId);
        _previousMasteryByGame.Remove(gameId);
        _previousCompletedSetsByGame.Remove(gameId);
    }

    /// <summary>
    /// Resets all tracker state.
    /// </summary>
    public void ResetAll()
    {
        _previousUnlockedByGame.Clear();
        _previousMasteryByGame.Clear();
        _previousCompletedSetsByGame.Clear();
        PreviousGameId = null;
    }

    /// <summary>
    /// Initializes the tracker with existing progress (no unlocks detected).
    /// </summary>
    public void InitializeWithProgress(UserGameProgress progress)
    {
        var gameId = progress.GameId;
        _previousUnlockedByGame[gameId] = new HashSet<int>(progress.UnlockedAchievements.Select(a => a.Id));
        _previousMasteryByGame[gameId] = progress.IsMastered;
        _previousCompletedSetsByGame[gameId] = new HashSet<long>(
            progress.AchievementSets.Where(s => s.IsCompleted).Select(s => s.SetId));
        PreviousGameId = gameId;
    }
}

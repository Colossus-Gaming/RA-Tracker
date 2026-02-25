using RATracker.Models;

namespace RATracker.WPF.Services;

/// <summary>
/// Provides user achievement progress data from RetroAchievements.
/// This interface abstracts the progress-related API calls to allow switching
/// between V1 and V2 API implementations.
/// </summary>
public interface IAchievementProgressProvider
{
    #region User Progress

    /// <summary>
    /// Gets the user summary including rank, points, and recent achievements.
    /// </summary>
    /// <param name="username">The username to get the summary for.</param>
    /// <param name="includeRecentAchievements">Whether to include recent achievements in the response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user summary with progress data.</returns>
    Task<UserSummary?> GetUserSummaryAsync(string username, bool includeRecentAchievements = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's rank and score.
    /// </summary>
    /// <param name="username">The username to get the rank for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's rank and score information.</returns>
    Task<UserRankAndScore?> GetUserRankAndScoreAsync(string username, CancellationToken cancellationToken = default);

    #endregion

    #region Game Progress

    /// <summary>
    /// Gets game information with the user's achievement progress.
    /// </summary>
    /// <param name="username">The username to get progress for.</param>
    /// <param name="gameId">The game ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The game info with user's achievement unlock status.</returns>
    Task<GameInfo?> GetGameInfoAndProgressAsync(string username, long gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user's recently played games.
    /// </summary>
    /// <param name="username">The username to get recently played games for.</param>
    /// <param name="count">The maximum number of games to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of recently played games with basic progress info.</returns>
    Task<List<GameInfo>> GetRecentlyPlayedGamesAsync(string username, int count = 10, CancellationToken cancellationToken = default);

    #endregion

    #region Achievement Progress

    /// <summary>
    /// Gets the user's recently unlocked achievements.
    /// </summary>
    /// <param name="username">The username to get recent achievements for.</param>
    /// <param name="minutes">The time window in minutes to look back for recent unlocks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of recently unlocked achievements.</returns>
    Task<List<Achievement>> GetRecentAchievementsAsync(string username, int minutes = 60, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets achievements for a specific game with the user's unlock status.
    /// </summary>
    /// <param name="username">The username to get progress for.</param>
    /// <param name="gameId">The game ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of achievements with unlock status.</returns>
    Task<List<Achievement>> GetGameAchievementsWithProgressAsync(string username, long gameId, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Represents a user's rank and score data.
/// </summary>
public class UserRankAndScore
{
    /// <summary>
    /// The user's current rank on the leaderboard.
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// The user's total points (standard scoring).
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// The user's total true points (weighted scoring based on rarity).
    /// </summary>
    public int TotalTruePoints { get; set; }

    /// <summary>
    /// The RetroRatio (TruePoints / Points).
    /// </summary>
    public string RetroRatio
    {
        get
        {
            if (TotalPoints == 0) return "0.00";
            return ((float)TotalTruePoints / TotalPoints).ToString("0.00");
        }
    }
}

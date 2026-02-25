using RATracker.Models;

namespace RATracker.WPF.Services;

/// <summary>
/// Service interface for fetching user progress on games and achievements.
/// This abstraction allows switching between V1 and V2 API implementations.
/// </summary>
public interface IProgressService
{
    #region User Game Progress

    /// <summary>
    /// Gets a user's progress on a specific game.
    /// </summary>
    /// <param name="userId">The user identifier (username or ULID).</param>
    /// <param name="gameId">The game ID.</param>
    /// <param name="includeAchievementSets">Whether to include multi-set progress details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's progress on the game, or null if not found.</returns>
    Task<UserGameProgress?> GetUserGameProgressAsync(
        string userId, 
        long gameId, 
        bool includeAchievementSets = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's recently unlocked achievements.
    /// </summary>
    /// <param name="userId">The user identifier (username or ULID).</param>
    /// <param name="count">The maximum number of achievements to return (default 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of recently unlocked achievements.</returns>
    Task<List<RecentAchievement>> GetUserRecentAchievementsAsync(
        string userId, 
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's recently played games with progress.
    /// </summary>
    /// <param name="userId">The user identifier (username or ULID).</param>
    /// <param name="count">The maximum number of games to return (default 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of recently played games with basic progress info.</returns>
    Task<List<RecentlyPlayedGame>> GetUserRecentlyPlayedGamesAsync(
        string userId, 
        int count = 10,
        CancellationToken cancellationToken = default);

    #endregion

    #region Unlock Detection

    /// <summary>
    /// Compares current progress with previous state to detect newly unlocked achievements.
    /// </summary>
    /// <param name="currentProgress">The current user game progress.</param>
    /// <param name="previousProgress">The previous user game progress.</param>
    /// <returns>Detection result containing any new unlocks.</returns>
    UnlockDetectionResult DetectNewUnlocks(UserGameProgress currentProgress, UserGameProgress? previousProgress);

    /// <summary>
    /// Checks for new unlocks using recent achievements as a quick detection method.
    /// </summary>
    /// <param name="recentAchievements">Recently unlocked achievements from the API.</param>
    /// <param name="previousUnlockedIds">Set of previously known unlocked achievement IDs.</param>
    /// <param name="userId">The user identifier.</param>
    /// <returns>List of achievement unlock events for newly detected unlocks.</returns>
    List<AchievementUnlockEvent> DetectNewUnlocksFromRecent(
        List<RecentAchievement> recentAchievements,
        HashSet<int> previousUnlockedIds,
        string userId);

    #endregion

    #region User Summary

    /// <summary>
    /// Gets updated user rank and score information.
    /// </summary>
    /// <param name="userId">The user identifier (username or ULID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user summary with current rank and score.</returns>
    Task<UserSummary?> GetUserSummaryAsync(
        string userId,
        CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Result of a progress service operation.
/// </summary>
/// <typeparam name="T">The type of data returned.</typeparam>
public class ProgressResult<T>
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The data returned by the operation.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Which API version was used to fetch the data.
    /// </summary>
    public ApiVersion ApiVersionUsed { get; set; }

    /// <summary>
    /// Whether V1 fallback was used.
    /// </summary>
    public bool UsedFallback { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static ProgressResult<T> Ok(T data, ApiVersion apiVersion = ApiVersion.V2, bool usedFallback = false)
    {
        return new ProgressResult<T>
        {
            Success = true,
            Data = data,
            ApiVersionUsed = apiVersion,
            UsedFallback = usedFallback
        };
    }

    /// <summary>
    /// Creates a failure result.
    /// </summary>
    public static ProgressResult<T> Fail(string errorMessage, ApiVersion apiVersion = ApiVersion.V2)
    {
        return new ProgressResult<T>
        {
            Success = false,
            ErrorMessage = errorMessage,
            ApiVersionUsed = apiVersion
        };
    }
}

/// <summary>
/// Indicates which API version was used.
/// </summary>
public enum ApiVersion
{
    /// <summary>
    /// The V1 (legacy) API.
    /// </summary>
    V1,

    /// <summary>
    /// The V2 (JSON:API) API.
    /// </summary>
    V2
}

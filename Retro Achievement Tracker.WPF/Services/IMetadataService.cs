using RATracker.Models;

namespace RATracker.WPF.Services;

/// <summary>
/// Service interface for fetching metadata (systems, games, users) without user progress.
/// This abstraction allows switching between V1 and V2 API implementations.
/// </summary>
public interface IMetadataService
{
    #region Systems

    /// <summary>
    /// Gets all active systems (consoles) from RetroAchievements.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all active systems.</returns>
    Task<List<SystemInfo>> GetSystemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific system by ID.
    /// </summary>
    /// <param name="systemId">The system ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The system information, or null if not found.</returns>
    Task<SystemInfo?> GetSystemAsync(long systemId, CancellationToken cancellationToken = default);

    #endregion

    #region Games

    /// <summary>
    /// Gets games for a specific system.
    /// </summary>
    /// <param name="systemId">The system ID to get games for.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of games for the system.</returns>
    Task<List<GameInfo>> GetGamesForSystemAsync(long systemId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed game information by ID (without user progress).
    /// </summary>
    /// <param name="gameId">The game ID.</param>
    /// <param name="includeAchievements">Whether to include achievement metadata.</param>
    /// <param name="includeAchievementSets">Whether to include achievement set information (V2 API multi-set support).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The game information, or null if not found.</returns>
    Task<GameInfo?> GetGameAsync(long gameId, bool includeAchievements = false, bool includeAchievementSets = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for games by title.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching games.</returns>
    Task<List<GameInfo>> SearchGamesAsync(string searchTerm, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    #endregion

    #region Users

    /// <summary>
    /// Gets user information by username or identifier (without progress data).
    /// </summary>
    /// <param name="identifier">The username, display name, or ULID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user summary, or null if not found.</returns>
    Task<UserSummary?> GetUserAsync(string identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for users by username.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching users.</returns>
    Task<List<UserSummary>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    #endregion

    #region Hubs (Collections)

    /// <summary>
    /// Gets a list of hubs/collections.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of hubs.</returns>
    Task<List<HubInfo>> GetHubsAsync(int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific hub by ID.
    /// </summary>
    /// <param name="hubId">The hub ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The hub information, or null if not found.</returns>
    Task<HubInfo?> GetHubAsync(long hubId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the games in a specific hub.
    /// </summary>
    /// <param name="hubId">The hub ID.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of games in the hub.</returns>
    Task<List<GameInfo>> GetHubGamesAsync(long hubId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    #endregion
}

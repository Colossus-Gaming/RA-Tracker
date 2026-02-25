using RATracker.Models;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Http.V2.Mappers;

namespace RATracker.WPF.Services;

/// <summary>
/// Partial implementation of IAchievementProgressProvider using the V2 API.
/// This provider handles metadata that V2 supports, but falls back to V1 for user progress data
/// that V2 doesn't yet support.
/// </summary>
/// <remarks>
/// As of Phase 3, the V2 API doesn't fully support user progress endpoints.
/// This implementation provides V2 support for:
/// - User summary (partial - rank, points from /users/{id})
/// - Game metadata (without progress)
/// 
/// For full progress data, use V1AchievementProgressProvider or a composite provider.
/// </remarks>
public class V2AchievementProgressProvider : IAchievementProgressProvider, IDisposable
{
    private readonly V2Client _v2Client;
    private readonly V1AchievementProgressProvider? _v1Fallback;
    private readonly bool _ownsClients;

    /// <summary>
    /// Creates a new V2AchievementProgressProvider with optional V1 fallback.
    /// </summary>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    /// <param name="authUsername">The authenticated username (required for V1 fallback).</param>
    /// <param name="enableV1Fallback">Whether to enable V1 fallback for unsupported operations.</param>
    public V2AchievementProgressProvider(string apiKey, string? authUsername = null, bool enableV1Fallback = true)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required", nameof(apiKey));

        _v2Client = new V2Client(apiKey);
        _ownsClients = true;

        if (enableV1Fallback && !string.IsNullOrWhiteSpace(authUsername))
        {
            _v1Fallback = new V1AchievementProgressProvider(authUsername, apiKey);
        }
    }

    /// <summary>
    /// Creates a new V2AchievementProgressProvider with existing clients.
    /// </summary>
    /// <param name="v2Client">The V2 client to use.</param>
    /// <param name="v1Fallback">Optional V1 fallback provider.</param>
    public V2AchievementProgressProvider(V2Client v2Client, V1AchievementProgressProvider? v1Fallback = null)
    {
        _v2Client = v2Client ?? throw new ArgumentNullException(nameof(v2Client));
        _v1Fallback = v1Fallback;
        _ownsClients = false;
    }

    #region User Progress

    /// <inheritdoc />
    /// <remarks>
    /// V2 supports user lookup via /users/{identifier}, but may not include recent achievements.
    /// Falls back to V1 if recent achievements are requested and V1 fallback is enabled.
    /// </remarks>
    public async Task<UserSummary?> GetUserSummaryAsync(string username, bool includeRecentAchievements = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        // If recent achievements are needed and we have V1 fallback, use it
        if (includeRecentAchievements && _v1Fallback != null)
        {
            return await _v1Fallback.GetUserSummaryAsync(username, includeRecentAchievements, cancellationToken);
        }

        try
        {
            var document = await _v2Client.GetUserAsync(username, cancellationToken);
            
            if (document.HasErrors)
                return null;

            var resource = document.GetSingleResource();
            return resource != null ? V2ResourceMapper.MapToUserSummary(resource) : null;
        }
        catch (V2ApiException ex) when (ex.StatusCode == 404)
        {
            return null;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// V2 includes rank in the user resource. Falls back to V1 if V2 doesn't have the data.
    /// </remarks>
    public async Task<UserRankAndScore?> GetUserRankAndScoreAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        try
        {
            var document = await _v2Client.GetUserAsync(username, cancellationToken);
            
            if (document.HasErrors)
            {
                // Fall back to V1 if available
                if (_v1Fallback != null)
                    return await _v1Fallback.GetUserRankAndScoreAsync(username, cancellationToken);
                return null;
            }

            var resource = document.GetSingleResource();
            if (resource == null)
                return null;

            return new UserRankAndScore
            {
                Rank = resource.GetAttribute<int?>("playerRank") ?? 0,
                TotalPoints = resource.GetAttribute<int?>("points") ?? 0,
                TotalTruePoints = resource.GetAttribute<int?>("pointsWeighted") ?? 0
            };
        }
        catch (V2ApiException)
        {
            // Fall back to V1 if available
            if (_v1Fallback != null)
                return await _v1Fallback.GetUserRankAndScoreAsync(username, cancellationToken);
            return null;
        }
    }

    #endregion

    #region Game Progress

    /// <inheritdoc />
    /// <remarks>
    /// V2 doesn't support user progress on games yet. Falls back to V1.
    /// </remarks>
    public async Task<GameInfo?> GetGameInfoAndProgressAsync(string username, long gameId, CancellationToken cancellationToken = default)
    {
        // V2 doesn't support user progress on games yet - must use V1
        if (_v1Fallback != null)
        {
            return await _v1Fallback.GetGameInfoAndProgressAsync(username, gameId, cancellationToken);
        }

        // If no V1 fallback, return game metadata without progress
        try
        {
            var query = V2QueryBuilder.Create()
                .Include(V2Constants.Relationships.System);

            var document = await _v2Client.GetGameAsync(gameId.ToString(), query, cancellationToken);
            
            if (document.HasErrors)
                return null;

            var resource = document.GetSingleResource();
            if (resource == null)
                return null;

            var includedIndex = document.GetIncludedIndex();
            return V2ResourceMapper.MapToGameInfo(resource, includedIndex);
        }
        catch (V2ApiException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// V2 doesn't support recently played games yet. Falls back to V1.
    /// </remarks>
    public async Task<List<GameInfo>> GetRecentlyPlayedGamesAsync(string username, int count = 10, CancellationToken cancellationToken = default)
    {
        // V2 doesn't support this endpoint yet - must use V1
        if (_v1Fallback != null)
        {
            return await _v1Fallback.GetRecentlyPlayedGamesAsync(username, count, cancellationToken);
        }

        return new List<GameInfo>();
    }

    #endregion

    #region Achievement Progress

    /// <inheritdoc />
    /// <remarks>
    /// V2 doesn't support recent achievements yet. Falls back to V1.
    /// </remarks>
    public async Task<List<Achievement>> GetRecentAchievementsAsync(string username, int minutes = 60, CancellationToken cancellationToken = default)
    {
        // V2 doesn't support this endpoint yet - must use V1
        if (_v1Fallback != null)
        {
            return await _v1Fallback.GetRecentAchievementsAsync(username, minutes, cancellationToken);
        }

        return new List<Achievement>();
    }

    /// <inheritdoc />
    /// <remarks>
    /// V2 doesn't support user achievement progress yet. Falls back to V1.
    /// </remarks>
    public async Task<List<Achievement>> GetGameAchievementsWithProgressAsync(string username, long gameId, CancellationToken cancellationToken = default)
    {
        // V2 doesn't support user progress on achievements yet - must use V1
        if (_v1Fallback != null)
        {
            return await _v1Fallback.GetGameAchievementsWithProgressAsync(username, gameId, cancellationToken);
        }

        return new List<Achievement>();
    }

    #endregion

    public void Dispose()
    {
        if (_ownsClients)
        {
            _v2Client.Dispose();
            _v1Fallback?.Dispose();
        }
    }
}

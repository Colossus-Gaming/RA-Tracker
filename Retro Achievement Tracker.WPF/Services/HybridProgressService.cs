using System.Diagnostics;
using System.Net;
using System.Net.Http;
using RATracker.Models;
using RATracker.WPF.Http.V2;

namespace RATracker.WPF.Services;

/// <summary>
/// Simple logger interface for progress service observability.
/// </summary>
public interface IProgressServiceLogger
{
    /// <summary>
    /// Logs a debug message.
    /// </summary>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// Logs a warning message with an exception.
    /// </summary>
    void LogWarning(Exception ex, string message, params object[] args);
}

/// <summary>
/// Default debug-output logger implementation.
/// </summary>
public class DebugProgressServiceLogger : IProgressServiceLogger
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static readonly DebugProgressServiceLogger Instance = new();

    /// <inheritdoc />
    public void LogDebug(string message, params object[] args)
    {
        Debug.WriteLine($"[DEBUG] {string.Format(message.Replace("{", "{{").Replace("}", "}}"), args)}");
    }

    /// <inheritdoc />
    public void LogWarning(string message, params object[] args)
    {
        Debug.WriteLine($"[WARN] {string.Format(message.Replace("{", "{{").Replace("}", "}}"), args)}");
    }

    /// <inheritdoc />
    public void LogWarning(Exception ex, string message, params object[] args)
    {
        Debug.WriteLine($"[WARN] {string.Format(message.Replace("{", "{{").Replace("}", "}}"), args)}: {ex.Message}");
    }
}

/// <summary>
/// Null logger implementation (no-op).
/// </summary>
public class NullProgressServiceLogger : IProgressServiceLogger
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static readonly NullProgressServiceLogger Instance = new();

    /// <inheritdoc />
    public void LogDebug(string message, params object[] args) { }

    /// <inheritdoc />
    public void LogWarning(string message, params object[] args) { }

    /// <inheritdoc />
    public void LogWarning(Exception ex, string message, params object[] args) { }
}

/// <summary>
/// Hybrid implementation of IProgressService that can use V2 API with V1 fallback.
/// </summary>
public class HybridProgressService : IProgressService, IDisposable
{
    private readonly V2ProgressService? _v2Service;
    private readonly V1ProgressService? _v1Service;
    private readonly IFeatureFlagService _featureFlags;
    private readonly IProgressServiceLogger _logger;
    private readonly string _username;
    private readonly string _apiKey;

    /// <summary>
    /// Creates a new HybridProgressService.
    /// </summary>
    /// <param name="username">The RetroAchievements username.</param>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    /// <param name="featureFlags">Feature flag service for controlling API version selection.</param>
    /// <param name="logger">Optional logger for observability.</param>
    /// <param name="v2ApiLogger">Optional V2 API logger.</param>
    public HybridProgressService(
        string username,
        string apiKey,
        IFeatureFlagService featureFlags,
        IProgressServiceLogger? logger = null,
        IV2ApiLogger? v2ApiLogger = null)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _featureFlags = featureFlags ?? throw new ArgumentNullException(nameof(featureFlags));
        _logger = logger ?? NullProgressServiceLogger.Instance;

        // Initialize V2 service if V2 is potentially enabled
        if (featureFlags.UseV2ForProgress || featureFlags.EnableV1Fallback)
        {
            _v2Service = new V2ProgressService(apiKey, v2ApiLogger);
        }

        // Initialize V1 service if V1 fallback is enabled or V2 is disabled
        if (!featureFlags.UseV2ForProgress || featureFlags.EnableV1Fallback)
        {
            _v1Service = new V1ProgressService(username, apiKey);
        }
    }

    /// <summary>
    /// Creates a new HybridProgressService with session-based V2 authentication.
    /// </summary>
    public HybridProgressService(
        string username,
        string apiKey,
        CookieContainer sessionCookies,
        string sessionUserAgent,
        IFeatureFlagService featureFlags,
        IProgressServiceLogger? logger = null,
        IV2ApiLogger? v2ApiLogger = null)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _apiKey = apiKey ?? string.Empty;
        _featureFlags = featureFlags ?? throw new ArgumentNullException(nameof(featureFlags));
        _logger = logger ?? NullProgressServiceLogger.Instance;

        if (featureFlags.UseV2ForProgress || featureFlags.EnableV1Fallback)
        {
            var v2Client = new V2Client(sessionCookies, sessionUserAgent, apiKey, logger: v2ApiLogger);
            _v2Service = new V2ProgressService(v2Client, v2ApiLogger);
        }

        if (!featureFlags.UseV2ForProgress || featureFlags.EnableV1Fallback)
        {
            _v1Service = new V1ProgressService(username, apiKey);
        }
    }

    #region IProgressService Implementation

    /// <inheritdoc />
    public async Task<UserGameProgress?> GetUserGameProgressAsync(
        string userId,
        long gameId,
        bool includeAchievementSets = false,
        CancellationToken cancellationToken = default)
    {
        // Try V2 first if enabled
        if (_featureFlags.UseV2ForProgress && _v2Service != null)
        {
            try
            {
                _logger.LogDebug("Fetching user game progress from V2 API for user {0}, game {1}", userId, gameId);
                
                var result = await _v2Service.GetUserGameProgressAsync(userId, gameId, includeAchievementSets, cancellationToken);
                if (result != null)
                {
                    _logger.LogDebug("Successfully fetched progress from V2 API");
                    return result;
                }

                _logger.LogWarning("V2 API returned null for user game progress");
            }
            catch (Exception ex) when (_featureFlags.EnableV1Fallback)
            {
                _logger.LogWarning(ex, "V2 API failed for user game progress, falling back to V1");
            }
        }

        // Use V1 if V2 is disabled or failed
        if (_v1Service != null)
        {
            _logger.LogDebug("Fetching user game progress from V1 API for game {0}", gameId);
            var result = await _v1Service.GetUserGameProgressAsync(userId, gameId, includeAchievementSets, cancellationToken);
            if (result != null)
            {
                _logger.LogDebug("Successfully fetched progress from V1 API (fallback: {0})", _featureFlags.UseV2ForProgress);
            }
            return result;
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<List<RecentAchievement>> GetUserRecentAchievementsAsync(
        string userId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        // Try V2 first if enabled
        if (_featureFlags.UseV2ForProgress && _v2Service != null)
        {
            try
            {
                _logger.LogDebug("Fetching recent achievements from V2 API for user {0}", userId);
                
                var result = await _v2Service.GetUserRecentAchievementsAsync(userId, count, cancellationToken);
                if (result.Count > 0)
                {
                    _logger.LogDebug("Successfully fetched {0} recent achievements from V2 API", result.Count);
                    return result;
                }
            }
            catch (Exception ex) when (_featureFlags.EnableV1Fallback)
            {
                _logger.LogWarning(ex, "V2 API failed for recent achievements, falling back to V1");
            }
        }

        // Use V1 if V2 is disabled or failed
        if (_v1Service != null)
        {
            _logger.LogDebug("Fetching recent achievements from V1 API");
            var result = await _v1Service.GetUserRecentAchievementsAsync(userId, count, cancellationToken);
            _logger.LogDebug("Fetched {0} recent achievements from V1 API", result.Count);
            return result;
        }

        return new List<RecentAchievement>();
    }

    /// <inheritdoc />
    public async Task<List<RecentlyPlayedGame>> GetUserRecentlyPlayedGamesAsync(
        string userId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        // Try V2 first if enabled
        if (_featureFlags.UseV2ForProgress && _v2Service != null)
        {
            try
            {
                _logger.LogDebug("Fetching recently played games from V2 API for user {0}", userId);
                
                var result = await _v2Service.GetUserRecentlyPlayedGamesAsync(userId, count, cancellationToken);
                if (result.Count > 0)
                {
                    _logger.LogDebug("Successfully fetched {0} recently played games from V2 API", result.Count);
                    return result;
                }
            }
            catch (Exception ex) when (_featureFlags.EnableV1Fallback)
            {
                _logger.LogWarning(ex, "V2 API failed for recently played games, falling back to V1");
            }
        }

        // Use V1 if V2 is disabled or failed
        if (_v1Service != null)
        {
            _logger.LogDebug("Fetching recently played games from V1 API");
            var result = await _v1Service.GetUserRecentlyPlayedGamesAsync(userId, count, cancellationToken);
            _logger.LogDebug("Fetched {0} recently played games from V1 API", result.Count);
            return result;
        }

        return new List<RecentlyPlayedGame>();
    }

    /// <inheritdoc />
    public UnlockDetectionResult DetectNewUnlocks(UserGameProgress currentProgress, UserGameProgress? previousProgress)
    {
        // Unlock detection logic is the same regardless of API version
        return _v2Service?.DetectNewUnlocks(currentProgress, previousProgress) 
               ?? _v1Service?.DetectNewUnlocks(currentProgress, previousProgress) 
               ?? new UnlockDetectionResult();
    }

    /// <inheritdoc />
    public List<AchievementUnlockEvent> DetectNewUnlocksFromRecent(
        List<RecentAchievement> recentAchievements,
        HashSet<int> previousUnlockedIds,
        string userId)
    {
        return _v2Service?.DetectNewUnlocksFromRecent(recentAchievements, previousUnlockedIds, userId) 
               ?? _v1Service?.DetectNewUnlocksFromRecent(recentAchievements, previousUnlockedIds, userId) 
               ?? new List<AchievementUnlockEvent>();
    }

    /// <inheritdoc />
    public async Task<UserSummary?> GetUserSummaryAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        // User summary - use V2 if UseV2ForUserLookup is enabled
        if (_featureFlags.UseV2ForUserLookup && _v2Service != null)
        {
            try
            {
                _logger.LogDebug("Fetching user summary from V2 API for user {0}", userId);
                
                var result = await _v2Service.GetUserSummaryAsync(userId, cancellationToken);
                if (result != null)
                {
                    _logger.LogDebug("Successfully fetched user summary from V2 API");
                    return result;
                }
            }
            catch (Exception ex) when (_featureFlags.EnableV1Fallback)
            {
                _logger.LogWarning(ex, "V2 API failed for user summary, falling back to V1");
            }
        }

        // Use V1 if V2 is disabled or failed
        if (_v1Service != null)
        {
            _logger.LogDebug("Fetching user summary from V1 API");
            return await _v1Service.GetUserSummaryAsync(userId, cancellationToken);
        }

        return null;
    }

    #endregion

    public void Dispose()
    {
        _v2Service?.Dispose();
        _v1Service?.Dispose();
    }
}

/// <summary>
/// Implementation of IProgressService using the V1 (legacy) API.
/// </summary>
public class V1ProgressService : IProgressService, IDisposable
{
    private readonly string _username;
    private readonly string _apiKey;
    private readonly V1ApiClient _client;

    /// <summary>
    /// Creates a new V1ProgressService.
    /// </summary>
    /// <param name="username">The RetroAchievements username.</param>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    public V1ProgressService(string username, string apiKey)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _client = new V1ApiClient(username, apiKey);
    }

    /// <inheritdoc />
    public async Task<UserGameProgress?> GetUserGameProgressAsync(
        string userId,
        long gameId,
        bool includeAchievementSets = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var gameInfo = await _client.GetGameInfoAndProgressAsync(gameId, cancellationToken);
            if (gameInfo == null)
            {
                return null;
            }

            // Populate game context on achievements
            gameInfo.Achievements?.ForEach(achievement =>
            {
                achievement.GameId = (int)gameInfo.Id;
                achievement.GameTitle = gameInfo.Title;
            });

            return UserGameProgress.FromGameInfo(gameInfo, userId);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<List<RecentAchievement>> GetUserRecentAchievementsAsync(
        string userId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var achievements = await _client.GetRecentAchievementsAsync(count, cancellationToken);
            return achievements.Select(a => new RecentAchievement
            {
                AchievementId = a.Id,
                Title = a.Title,
                Description = a.Description,
                Points = a.Points,
                TruePoints = a.TrueRatio,
                BadgeUrl = a.BadgeUri,
                GameId = a.GameId,
                GameTitle = a.GameTitle,
                EarnedAt = a.DateEarned
            }).ToList();
        }
        catch
        {
            return new List<RecentAchievement>();
        }
    }

    /// <inheritdoc />
    public async Task<List<RecentlyPlayedGame>> GetUserRecentlyPlayedGamesAsync(
        string userId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var games = await _client.GetRecentlyPlayedGamesAsync(count, cancellationToken);
            return games.Select(g => new RecentlyPlayedGame
            {
                GameId = g.Id,
                Title = g.Title,
                ConsoleName = g.ConsoleName,
                BadgeUrl = g.BadgeUri,
                LastPlayed = g.LastPlayed,
                TotalAchievements = g.AchievementsPossible,
                EarnedAchievements = g.AchievementsEarned
            }).ToList();
        }
        catch
        {
            return new List<RecentlyPlayedGame>();
        }
    }

    /// <inheritdoc />
    public UnlockDetectionResult DetectNewUnlocks(UserGameProgress currentProgress, UserGameProgress? previousProgress)
    {
        var result = new UnlockDetectionResult();

        if (previousProgress == null)
        {
            return result;
        }

        var previousUnlockedIds = new HashSet<int>(previousProgress.UnlockedAchievements.Select(a => a.Id));

        foreach (var achievement in currentProgress.UnlockedAchievements)
        {
            if (!previousUnlockedIds.Contains(achievement.Id))
            {
                var unlockEvent = AchievementUnlockEvent.FromAchievement(achievement, currentProgress.UserId);
                result.NewUnlocks.Add(unlockEvent);
            }
        }

        result.GameMastered = currentProgress.IsMastered;
        result.JustMastered = currentProgress.IsMastered && !previousProgress.IsMastered;

        if (result.HasNewUnlocks && result.JustMastered)
        {
            result.NewUnlocks.Last().TriggeredMastery = true;
        }

        return result;
    }

    /// <inheritdoc />
    public List<AchievementUnlockEvent> DetectNewUnlocksFromRecent(
        List<RecentAchievement> recentAchievements,
        HashSet<int> previousUnlockedIds,
        string userId)
    {
        var newUnlocks = new List<AchievementUnlockEvent>();

        foreach (var recent in recentAchievements)
        {
            if (!previousUnlockedIds.Contains(recent.AchievementId))
            {
                var achievement = recent.ToAchievement();
                var unlockEvent = AchievementUnlockEvent.FromAchievement(achievement, userId);
                newUnlocks.Add(unlockEvent);
            }
        }

        return newUnlocks;
    }

    /// <inheritdoc />
    public async Task<UserSummary?> GetUserSummaryAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _client.GetUserSummaryAsync(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}

/// <summary>
/// Simple HTTP client wrapper for V1 API calls.
/// </summary>
internal class V1ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _username;
    private readonly string _apiKey;
    private const string BaseUrl = "https://retroachievements.org";

    public V1ApiClient(string username, string apiKey)
    {
        _username = username;
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
    }

    public async Task<GameInfo?> GetGameInfoAndProgressAsync(long gameId, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/API/API_GetGameInfoAndUserProgress.php?z={_username}&y={_apiKey}&u={_username}&g={gameId}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<GameInfo>(json);
    }

    public async Task<List<Achievement>> GetRecentAchievementsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/API/API_GetUserRecentAchievements.php?z={_username}&y={_apiKey}&u={_username}&c={count}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return new List<Achievement>();
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Achievement>>(json) ?? new List<Achievement>();
    }

    public async Task<List<GameInfo>> GetRecentlyPlayedGamesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/API/API_GetUserRecentlyPlayedGames.php?z={_username}&y={_apiKey}&u={_username}&c={count}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return new List<GameInfo>();
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<List<GameInfo>>(json) ?? new List<GameInfo>();
    }

    public async Task<UserSummary?> GetUserSummaryAsync(CancellationToken cancellationToken = default)
    {
        var url = $"{BaseUrl}/API/API_GetUserSummary.php?z={_username}&y={_apiKey}&u={_username}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<UserSummary>(json);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

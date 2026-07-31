using System.Diagnostics;
using System.Net;
using System.Net.Http;
using RATracker.Models;
using RATracker.WPF.Http.V1;
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
            _v1Service = new V1ProgressService(username, _apiKey);
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
            _v1Service = new V1ProgressService(username, _apiKey);
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
        // Efficient hybrid (confirmed by live probing 2026-05-29):
        //
        // Phase 1 (parallel — 1 v1 + 3 v2 calls when subsets are enabled):
        //   - v1 GetGameInfoAndUserProgress     → full core achievement list + unlock status (single call).
        //   - v2 games/{id}?include=achievementSets → every set on the game (Core/Bonus/Specialty/Exclusive)
        //     with `achievementsPublished` totals and the type-per-game from `types[]`.
        //   - v2 users/{u}/player-achievement-sets?filter[gameId] → engaged-set earned counts.
        //   - v2 users/{u}/player-achievements?filter[gameId] → every unlock the user has on this game,
        //     indexed by achievement id (needed to apply unlock dates to non-core achievements).
        //
        // Phase 2 (parallel, only if multiset): v2 achievements?filter[achievementSetId]={setId} per
        // non-core set → that set's definitions; merged onto progress with set tags + unlock dates.

        var v1Task = _v1Service != null
            ? _v1Service.GetUserGameProgressAsync(userId, gameId, includeAchievementSets, cancellationToken)
            : Task.FromResult<UserGameProgress?>(null);

        var enrichSets = includeAchievementSets && _v2Service != null && _featureFlags.EnableMultiSet;
        Task<List<AchievementSetProgress>>? gameSetsTask = null;
        Task<List<AchievementSetProgress>>? playerSetsTask = null;
        Task<Dictionary<int, (DateTime? UnlockedAt, bool Hardcore)>>? unlocksTask = null;
        if (enrichSets)
        {
            gameSetsTask = _v2Service!.GetGameAchievementSetsAsync(gameId, cancellationToken);
            playerSetsTask = _v2Service!.GetPlayerAchievementSetsAsync(userId, gameId, cancellationToken);
            unlocksTask = _v2Service!.GetPlayerUnlocksForGameAsync(userId, gameId, cancellationToken);
        }

        UserGameProgress? progress;
        try
        {
            progress = await v1Task;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "V1 user game progress failed");
            progress = null;
        }

        // Last-resort v2 fallback if v1 was unavailable or returned null.
        if (progress == null && _v2Service != null && _featureFlags.UseV2ForProgress)
        {
            try
            {
                progress = await _v2Service.GetUserGameProgressAsync(userId, gameId, includeAchievementSets, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "V2 user game progress fallback failed");
            }
        }

        if (progress == null || !enrichSets) return progress;

        // Merge phase-1 set data + tag v1 core achievements + fetch non-core achievements.
        try
        {
            var gameSets = gameSetsTask != null ? await gameSetsTask : new List<AchievementSetProgress>();
            var playerSets = playerSetsTask != null ? await playerSetsTask : new List<AchievementSetProgress>();
            var unlocks = unlocksTask != null ? await unlocksTask : new Dictionary<int, (DateTime? UnlockedAt, bool Hardcore)>();

            // Build the authoritative set list: prefer the game's full set list (includes unengaged sets),
            // fold in engagement counts from player-achievement-sets where present.
            var mergedSets = MergeSetProgress(gameSets, playerSets);
            if (mergedSets.Count > 0) progress.AchievementSets = mergedSets;

            // Tag v1's core achievements with the core set's metadata so the UI can group/route them.
            var coreSet = mergedSets.FirstOrDefault(s => s.SetType == AchievementSetType.Core);
            if (coreSet != null)
            {
                foreach (var a in progress.Achievements)
                {
                    a.SetId = coreSet.SetId;
                    a.SetType = coreSet.SetType;
                    if (string.IsNullOrEmpty(a.SetName)) a.SetName = coreSet.SetName;
                }
            }

            // Phase 2 — for multiset games, fetch ALL game achievements in ONE call (v2 has no
            // filter[achievementSetId] — that returns 400 — but filter[gameId]+include=achievementSet
            // gives every achievement with its set membership), then add non-core ones, tagged with
            // unlock dates. Skipped entirely for single-set games (saves the call).
            var nonCoreSetIds = new HashSet<long>(mergedSets
                .Where(s => s.SetType != AchievementSetType.Core && s.SetId > 0 && s.TotalAchievements > 0)
                .Select(s => s.SetId));
            if (nonCoreSetIds.Count > 0)
            {
                var allAchievements = await _v2Service!.GetGameAchievementsWithSetsAsync(gameId, cancellationToken);

                // Subset achievements belong to a DIFFERENT gameId than the base game, so the
                // gameId-filtered unlock map above does NOT contain their unlocks. Fetch each engaged
                // non-core set's unlocks by achievementSetId (player-achievements #4633) and merge them
                // in — otherwise every subset achievement reads as locked (e.g. a set shows 0/39 when
                // the user has unlocked 8). Only engaged sets (EarnedAchievements > 0, from the
                // player-achievement-sets aggregate) are queried, so untouched subsets cost nothing.
                var engagedNonCoreSetIds = mergedSets
                    .Where(s => s.SetType != AchievementSetType.Core && s.SetId > 0 && s.EarnedAchievements > 0)
                    .Select(s => s.SetId)
                    .Distinct()
                    .ToList();
                if (engagedNonCoreSetIds.Count > 0)
                {
                    var setUnlockMaps = await Task.WhenAll(engagedNonCoreSetIds
                        .Select(setId => _v2Service!.GetPlayerUnlocksForSetAsync(userId, setId, cancellationToken)));
                    foreach (var map in setUnlockMaps)
                    {
                        foreach (var kv in map) unlocks[kv.Key] = kv.Value;
                    }
                }

                var existingIds = new HashSet<int>(progress.Achievements.Select(a => a.Id));
                var added = 0;
                var applied = 0;
                foreach (var a in allAchievements)
                {
                    if (a.SetId is not long setId || !nonCoreSetIds.Contains(setId)) continue;
                    if (!existingIds.Add(a.Id)) continue;
                    a.GameId = (int)gameId;
                    a.GameTitle = progress.GameTitle;
                    if (unlocks.TryGetValue(a.Id, out var unlock))
                    {
                        a.DateEarned = unlock.UnlockedAt;
                        applied++;
                    }
                    progress.Achievements.Add(a);
                    added++;
                }
                _logger.LogDebug($"Enriched {nonCoreSetIds.Count} non-core set(s): added {added} achievements, applied {applied} unlocks (from {engagedNonCoreSetIds.Count} engaged set(s)).");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "V2 set enrichment failed (continuing with v1 base progress)");
        }

        return progress;
    }

    /// <summary>
    /// Merges the per-game full set list with the engaged-set counts (earned achievements/points)
    /// from player-achievement-sets. Sets present only in the game list contribute their totals
    /// with zero engagement; sets engaged with override the type when the game list lacks it.
    /// </summary>
    private static List<AchievementSetProgress> MergeSetProgress(
        List<AchievementSetProgress> gameSets,
        List<AchievementSetProgress> playerSets)
    {
        if (gameSets.Count == 0) return playerSets;

        var byId = playerSets.GroupBy(s => s.SetId).ToDictionary(g => g.Key, g => g.First());
        var merged = new List<AchievementSetProgress>(gameSets.Count);
        foreach (var g in gameSets)
        {
            if (byId.TryGetValue(g.SetId, out var p))
            {
                merged.Add(new AchievementSetProgress
                {
                    SetId = g.SetId,
                    SetName = !string.IsNullOrEmpty(g.SetName) ? g.SetName : p.SetName,
                    SetType = g.SetType != AchievementSetType.Unknown ? g.SetType : p.SetType,
                    TotalAchievements = g.TotalAchievements,
                    TotalPoints = g.TotalPoints,
                    EarnedAchievements = p.EarnedAchievements,
                    EarnedPoints = p.EarnedPoints,
                    BadgeUrl = !string.IsNullOrEmpty(g.BadgeUrl) ? g.BadgeUrl : p.BadgeUrl
                });
            }
            else
            {
                merged.Add(g);
            }
        }
        return merged;
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
        // v1 API_GetUserSummary returns everything the UI needs in a single call: UserName, Rank,
        // TotalPoints, TotalTruePoints, Motto, UserPic, LastGameID. The v2 user resource does NOT
        // carry Rank or LastGameID (the UI would show "Rank #0"), so v1 is the right primary here.
        if (_v1Service != null)
        {
            try
            {
                var result = await _v1Service.GetUserSummaryAsync(userId, cancellationToken);
                if (result != null) return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "V1 user summary failed");
            }
        }

        // Fallback to v2 only if v1 is unavailable (rare).
        if (_v2Service != null)
        {
            try
            {
                return await _v2Service.GetUserSummaryAsync(userId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "V2 user summary fallback failed");
            }
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
            var dto = await _client.GetGameProgressDtoAsync(gameId, cancellationToken);
            if (dto == null)
            {
                Debug.WriteLine($"[V1Progress] game={gameId} returned null DTO");
                return null;
            }

            var gameInfo = V1Mapper.MapGameProgress(dto);
            gameInfo.Achievements?.ForEach(achievement =>
            {
                achievement.GameId = (int)gameInfo.Id;
                achievement.GameTitle = gameInfo.Title;
            });

            var progress = UserGameProgress.FromGameInfo(gameInfo, userId);

            // Trust the API's authoritative counts for earned/total.
            progress.TotalAchievements = dto.NumAchievements;
            progress.EarnedAchievements = dto.NumAwardedToUser;

            Debug.WriteLine($"[V1Progress] game={gameId} ok title='{gameInfo.Title}' earned={dto.NumAwardedToUser}/{dto.NumAchievements} achievementsList={gameInfo.Achievements?.Count ?? 0}");
            return progress;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[V1Progress] game={gameId} EXCEPTION {ex.GetType().Name}: {ex.Message}");
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
            var dtos = await _client.GetRecentAchievementsDtoAsync(count, cancellationToken);
            return dtos.Select(V1Mapper.MapRecentAchievement).ToList();
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
            var dtos = await _client.GetRecentlyPlayedDtoAsync(count, cancellationToken);
            return dtos.Select(V1Mapper.MapRecentlyPlayed).ToList();
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
            var dto = await _client.GetUserSummaryDtoAsync(cancellationToken);
            return dto != null ? V1Mapper.MapUserSummary(dto) : null;
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
/// Simple HTTP client wrapper for the public v1 Web API. Returns DTOs (see <see cref="V1Mapper"/>)
/// that match the real v1 field names; callers map these to domain models.
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

    private async Task<T?> GetJsonAsync<T>(string url, CancellationToken cancellationToken)
    {
        // Log URL with the API key redacted so the diagnostic trail doesn't leak credentials.
        var logUrl = System.Text.RegularExpressions.Regex.Replace(url, @"([?&])y=[^&]+", "$1y=***");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            sw.Stop();
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"[V1API] GET {logUrl} -> {(int)response.StatusCode} {response.ReasonPhrase} ({sw.ElapsedMilliseconds}ms)");
                return default;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            Debug.WriteLine($"[V1API] GET {logUrl} -> 200 OK ({sw.ElapsedMilliseconds}ms, {json.Length} chars)");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
        catch (Exception ex)
        {
            sw.Stop();
            Debug.WriteLine($"[V1API] GET {logUrl} -> EXCEPTION {ex.GetType().Name}: {ex.Message} ({sw.ElapsedMilliseconds}ms)");
            throw;
        }
    }

    public Task<V1GameProgressDto?> GetGameProgressDtoAsync(long gameId, CancellationToken cancellationToken = default)
        => GetJsonAsync<V1GameProgressDto>(
            $"{BaseUrl}/API/API_GetGameInfoAndUserProgress.php?z={_username}&y={_apiKey}&u={_username}&g={gameId}",
            cancellationToken);

    public async Task<List<V1RecentAchievementDto>> GetRecentAchievementsDtoAsync(int count = 10, CancellationToken cancellationToken = default)
        => await GetJsonAsync<List<V1RecentAchievementDto>>(
            $"{BaseUrl}/API/API_GetUserRecentAchievements.php?z={_username}&y={_apiKey}&u={_username}&c={count}",
            cancellationToken) ?? new List<V1RecentAchievementDto>();

    public async Task<List<V1RecentlyPlayedDto>> GetRecentlyPlayedDtoAsync(int count = 10, CancellationToken cancellationToken = default)
        => await GetJsonAsync<List<V1RecentlyPlayedDto>>(
            $"{BaseUrl}/API/API_GetUserRecentlyPlayedGames.php?z={_username}&y={_apiKey}&u={_username}&c={count}",
            cancellationToken) ?? new List<V1RecentlyPlayedDto>();

    public Task<V1UserSummaryDto?> GetUserSummaryDtoAsync(CancellationToken cancellationToken = default)
        => GetJsonAsync<V1UserSummaryDto>(
            $"{BaseUrl}/API/API_GetUserSummary.php?z={_username}&y={_apiKey}&u={_username}",
            cancellationToken);

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

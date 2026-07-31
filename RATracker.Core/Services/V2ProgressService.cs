using System.Globalization;
using Newtonsoft.Json.Linq;
using RATracker.Models;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Http.V2.JsonApi;
using RATracker.WPF.Http.V2.Mappers;

namespace RATracker.WPF.Services;

/// <summary>
/// Implementation of IProgressService using the V2 API.
/// </summary>
public class V2ProgressService : IProgressService, IDisposable
{
    private readonly V2Client _client;
    private readonly bool _ownsClient;
    private readonly IV2ApiLogger _logger;

    /// <summary>
    /// Creates a new V2ProgressService with the specified API key.
    /// </summary>
    /// <param name="apiKey">The RetroAchievements API key.</param>
    /// <param name="logger">Optional logger for observability.</param>
    public V2ProgressService(string apiKey, IV2ApiLogger? logger = null)
    {
        _logger = logger ?? NullApiLogger.Instance;
        _client = new V2Client(apiKey, logger: _logger);
        _ownsClient = true;
    }

    /// <summary>
    /// Creates a new V2ProgressService with an existing V2Client.
    /// </summary>
    /// <param name="client">The V2Client to use.</param>
    /// <param name="logger">Optional logger for observability.</param>
    public V2ProgressService(V2Client client, IV2ApiLogger? logger = null)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? NullApiLogger.Instance;
        _ownsClient = false;
    }

    #region IProgressService Implementation

    /// <inheritdoc />
    public async Task<UserGameProgress?> GetUserGameProgressAsync(
        string userId,
        long gameId,
        bool includeAchievementSets = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        try
        {
            // V2 has no "progress" route. Per-game progress is the player-games resource filtered
            // by gameId, scoped to the user: GET /v2/users/{id}/player-games?filter[gameId]={gameId}
            var queryBuilder = V2QueryBuilder.Create()
                .Filter("gameId", gameId)
                .Include("game");

            if (includeAchievementSets)
            {
                queryBuilder.Include("achievementSets");
                queryBuilder.Include("playerAchievementSets");
            }

            var document = await _client.GetRelationshipAsync(
                "users", userId, "player-games",
                queryBuilder,
                cancellationToken);

            if (document.HasErrors)
            {
                _logger.LogError("GET", $"/users/{userId}/player-games?filter[gameId]={gameId}",
                    string.Join(", ", document.Errors?.Select(e => e.Detail) ?? Array.Empty<string>()));
                return null;
            }

            var resource = document.GetResourceCollection().FirstOrDefault()
                           ?? document.GetSingleResource();
            if (resource == null)
            {
                return null;
            }

            var includedIndex = document.GetIncludedIndex();
            var progress = MapToUserGameProgress(resource, includedIndex, userId);

            // The player-games resource doesn't carry the per-achievement list (only set-level data),
            // so when there are no achievements, defer to the v1 fallback which returns the full list
            // with unlock status in a single call.
            return progress.Achievements.Count > 0 ? progress : null;
        }
        catch (V2ApiException ex) when (ex.StatusCode == 404)
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
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<RecentAchievement>();
        }

        try
        {
            // V2 API endpoint: GET /v2/users/{id}/player-achievements?include=achievement&page[size]=N
            var queryBuilder = V2QueryBuilder.Create()
                .PageSize(count)
                .Include("achievement");

            var document = await _client.GetRelationshipAsync(
                "users", userId, "player-achievements",
                queryBuilder,
                cancellationToken);

            if (document.HasErrors)
            {
                return new List<RecentAchievement>();
            }

            var includedIndex = document.GetIncludedIndex();
            return document.GetResourceCollection()
                .Where(r => r.Type == "player-achievements" || r.Type == V2Constants.ResourceTypes.Achievements)
                .Select(r => MapToRecentAchievement(r, includedIndex))
                .ToList();
        }
        catch (V2ApiException)
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
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<RecentlyPlayedGame>();
        }

        try
        {
            // V2 API endpoint: GET /v2/users/{id}/player-games?include=game,game.system&sort=-lastPlayedAt&page[size]=N
            var queryBuilder = V2QueryBuilder.Create()
                .SortDescending("lastPlayedAt")
                .PageSize(count)
                .Include("game");

            var document = await _client.GetRelationshipAsync(
                "users", userId, "player-games",
                queryBuilder,
                cancellationToken);

            if (document.HasErrors)
            {
                return new List<RecentlyPlayedGame>();
            }

            var includedIndex = document.GetIncludedIndex();
            return document.GetResourceCollection()
                .Where(r => r.Type == "player-games" || r.Type == V2Constants.ResourceTypes.Games || r.Type == "user-game-progress")
                .Select(r => MapToRecentlyPlayedGame(r, includedIndex))
                .ToList();
        }
        catch (V2ApiException)
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
            // No previous state, no unlocks to detect
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

        // Check for mastery
        result.GameMastered = currentProgress.IsMastered;
        result.JustMastered = currentProgress.IsMastered && !previousProgress.IsMastered;

        // Check for set completions (multi-set games)
        if (currentProgress.HasMultipleSets && previousProgress.HasMultipleSets)
        {
            var previousCompletedSetIds = new HashSet<long>(
                previousProgress.AchievementSets.Where(s => s.IsCompleted).Select(s => s.SetId));

            foreach (var setProgress in currentProgress.AchievementSets)
            {
                if (setProgress.IsCompleted && !previousCompletedSetIds.Contains(setProgress.SetId))
                {
                    result.CompletedSets.Add(setProgress);
                }
            }
        }

        // Mark the last unlock with mastery/completion flags
        if (result.HasNewUnlocks)
        {
            var lastUnlock = result.NewUnlocks.Last();
            lastUnlock.TriggeredMastery = result.JustMastered;

            if (result.CompletedSets.Count > 0)
            {
                var completedSet = result.CompletedSets.First();
                lastUnlock.TriggeredSetCompletion = true;
                lastUnlock.AchievementSetId = completedSet.SetId;
                lastUnlock.AchievementSetName = completedSet.SetName;
            }
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
                unlockEvent.IsHardcore = recent.IsHardcore;
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
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        try
        {
            var document = await _client.GetUserAsync(userId, cancellationToken);

            if (document.HasErrors)
            {
                return null;
            }

            var resource = document.GetSingleResource();
            return resource != null ? V2ResourceMapper.MapToUserSummary(resource) : null;
        }
        catch (V2ApiException)
        {
            return null;
        }
    }

    /// <summary>
    /// Fetches every achievement set defined for a game (regardless of whether the user has engaged
    /// with them) from <c>/v2/games/{id}?include=achievementSets</c>. The set type per game is read
    /// from the achievement-set's <c>types[]</c> array (matched by gameId).
    /// </summary>
    public async Task<List<AchievementSetProgress>> GetGameAchievementSetsAsync(
        long gameId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryBuilder = V2QueryBuilder.Create().Include("achievementSets");
            var document = await _client.GetResourceAsync("games", gameId.ToString(), queryBuilder, cancellationToken);

            if (document.HasErrors) return new List<AchievementSetProgress>();

            var gameResource = document.GetSingleResource();
            var includedIndex = document.GetIncludedIndex();
            if (gameResource == null) return new List<AchievementSetProgress>();

            var setsRelationship = gameResource.GetRelationship("achievementSets");
            if (setsRelationship == null) return new List<AchievementSetProgress>();

            var sets = new List<AchievementSetProgress>();
            foreach (var setRef in setsRelationship.GetIdentifierCollection())
            {
                if (!includedIndex.TryGetValue((setRef.Type, setRef.Id), out var setResource)) continue;

                var setProgress = new AchievementSetProgress
                {
                    SetId = long.TryParse(setResource.Id, out var sid) ? sid : 0,
                    SetName = setResource.GetAttribute<string>("title") ?? string.Empty,
                    TotalAchievements = setResource.GetAttribute<int?>("achievementsPublished") ?? 0,
                    TotalPoints = setResource.GetAttribute<int?>("pointsTotal") ?? 0,
                    BadgeUrl = setResource.GetAttribute<string>("badgeUrl") ?? string.Empty
                };

                // Each achievement-set carries a types[] array of {gameId, type} pairs (a set can be
                // attached to multiple games, with potentially different types per game).
                if (setResource.Attributes?["types"] is JArray typesArray)
                {
                    foreach (var entry in typesArray)
                    {
                        if (entry.Value<long?>("gameId") == gameId)
                        {
                            var typeStr = entry.Value<string?>("type");
                            if (!string.IsNullOrEmpty(typeStr))
                            {
                                setProgress.SetType = AchievementSet.ParseSetType(typeStr);
                            }
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(setProgress.SetName))
                {
                    setProgress.SetName = setProgress.SetType == AchievementSetType.Core
                        ? "Core" : setProgress.SetType.ToString();
                }

                sets.Add(setProgress);
            }
            return sets;
        }
        catch (V2ApiException)
        {
            return new List<AchievementSetProgress>();
        }
    }

    /// <summary>
    /// Fetches every achievement defined for a game with set membership, paginated.
    /// (<c>/v2/achievements?filter[gameId]={id}&amp;include=achievementSet&amp;page[size]=100&amp;page[number]=N</c>).
    /// v2 has no <c>filter[achievementSetId]</c> (returns 400), so this is the efficient route to learn
    /// which set each achievement belongs to for multiset games.
    /// </summary>
    public async Task<List<Achievement>> GetGameAchievementsWithSetsAsync(
        long gameId,
        CancellationToken cancellationToken = default)
    {
        var result = new List<Achievement>();
        try
        {
            const int maxPages = 20; // safety cap (covers any realistic game)
            int page = 1;
            while (page <= maxPages)
            {
                var queryBuilder = V2QueryBuilder.Create()
                    .Filter("gameId", gameId)
                    .Include("achievementSet")
                    .PageSize(100)
                    .Page(page);
                var document = await _client.GetCollectionAsync("achievements", queryBuilder, cancellationToken);
                if (document.HasErrors) break;

                var includedIndex = document.GetIncludedIndex();
                foreach (var resource in document.GetResourceCollection().Where(r => r.Type == "achievements"))
                {
                    var achievement = V2ResourceMapper.MapToAchievement(resource, includedIndex);

                    var setRef = resource.GetRelationship("achievementSet")?.GetSingleIdentifier();
                    if (setRef != null && long.TryParse(setRef.Id, out var sid))
                    {
                        achievement.SetId = sid;
                        if (includedIndex.TryGetValue((setRef.Type, setRef.Id), out var setResource))
                        {
                            achievement.SetName = setResource.GetAttribute<string>("title") ?? string.Empty;

                            if (setResource.Attributes?["types"] is JArray typesArray)
                            {
                                foreach (var entry in typesArray)
                                {
                                    if (entry.Value<long?>("gameId") == gameId)
                                    {
                                        var typeStr = entry.Value<string?>("type");
                                        if (!string.IsNullOrEmpty(typeStr))
                                        {
                                            achievement.SetType = AchievementSet.ParseSetType(typeStr);
                                        }
                                        break;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(achievement.SetName))
                            {
                                achievement.SetName = achievement.SetType == AchievementSetType.Core
                                    ? "Core" : achievement.SetType.ToString();
                            }
                        }
                    }

                    result.Add(achievement);
                }

                var lastPage = document.Meta?["page"]?["lastPage"]?.Value<int>() ?? page;
                if (page >= lastPage) break;
                page++;
            }
        }
        catch (V2ApiException)
        {
            // best-effort; return what we have
        }
        return result;
    }

    /// <summary>
    /// Fetches every unlock the user has on a specific game across all sets, indexed by achievement id.
    /// Used to apply unlock timestamps onto non-core-set achievement definitions.
    /// </summary>
    public Task<Dictionary<int, (DateTime? UnlockedAt, bool Hardcore)>> GetPlayerUnlocksForGameAsync(
        string userId,
        long gameId,
        CancellationToken cancellationToken = default)
        => GetPlayerUnlocksAsync(userId, "gameId", gameId, cancellationToken);

    /// <summary>
    /// Fetches the user's unlocks for a single achievement set, keyed by achievement id. Subset
    /// achievements belong to a different gameId than the base game, so they are NOT returned by the
    /// gameId-filtered query — they must be fetched by achievementSetId (player-achievements #4633).
    /// </summary>
    public Task<Dictionary<int, (DateTime? UnlockedAt, bool Hardcore)>> GetPlayerUnlocksForSetAsync(
        string userId,
        long achievementSetId,
        CancellationToken cancellationToken = default)
        => GetPlayerUnlocksAsync(userId, "achievementSetId", achievementSetId, cancellationToken);

    private async Task<Dictionary<int, (DateTime? UnlockedAt, bool Hardcore)>> GetPlayerUnlocksAsync(
        string userId,
        string filterKey,
        long filterValue,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<int, (DateTime? UnlockedAt, bool Hardcore)>();
        if (string.IsNullOrWhiteSpace(userId)) return result;

        try
        {
            const int maxPages = 20;
            int page = 1;
            while (page <= maxPages)
            {
                var queryBuilder = V2QueryBuilder.Create()
                    .Filter(filterKey, filterValue)
                    .PageSize(100)
                    .Page(page);
                var document = await _client.GetRelationshipAsync(
                    "users", userId, "player-achievements", queryBuilder, cancellationToken);

                if (document.HasErrors) break;

                foreach (var resource in document.GetResourceCollection().Where(r => r.Type == "player-achievements"))
                {
                    var achRef = resource.GetRelationship("achievement")?.GetSingleIdentifier();
                    if (achRef == null || !int.TryParse(achRef.Id, out var achId)) continue;

                    var hardcoreAtStr = resource.GetAttribute<string>("unlockedHardcoreAt");
                    var unlockedAtStr = resource.GetAttribute<string>("unlockedAt");
                    DateTime? when = null;
                    if (!string.IsNullOrEmpty(hardcoreAtStr) &&
                        DateTime.TryParse(hardcoreAtStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var hc))
                    {
                        when = hc;
                    }
                    else if (!string.IsNullOrEmpty(unlockedAtStr) &&
                             DateTime.TryParse(unlockedAtStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var so))
                    {
                        when = so;
                    }
                    result[achId] = (when, !string.IsNullOrEmpty(hardcoreAtStr));
                }

                var lastPage = document.Meta?["page"]?["lastPage"]?.Value<int>() ?? page;
                if (page >= lastPage) break;
                page++;
            }
        }
        catch (V2ApiException)
        {
            // best-effort
        }
        return result;
    }

    /// <summary>
    /// Fetches per-set progress for a user on a specific game from v2's player-achievement-sets resource.
    /// Each returned <see cref="AchievementSetProgress"/> carries the set type (Core/Bonus/Specialty/Exclusive)
    /// from <c>setContext</c>, unlock counts, and the total achievement count from the included achievement-set.
    /// This is the efficient single-call source for subset progress aggregates (verified live).
    /// </summary>
    public async Task<List<AchievementSetProgress>> GetPlayerAchievementSetsAsync(
        string userId,
        long gameId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<AchievementSetProgress>();
        }

        try
        {
            var queryBuilder = V2QueryBuilder.Create()
                .Filter("gameId", gameId)
                .Include("achievementSet");

            var document = await _client.GetRelationshipAsync(
                "users", userId, "player-achievement-sets",
                queryBuilder,
                cancellationToken);

            if (document.HasErrors)
            {
                return new List<AchievementSetProgress>();
            }

            var includedIndex = document.GetIncludedIndex();
            return document.GetResourceCollection()
                .Where(r => r.Type == "player-achievement-sets")
                .Select(r => MapPlayerAchievementSet(r, includedIndex))
                .ToList();
        }
        catch (V2ApiException)
        {
            return new List<AchievementSetProgress>();
        }
    }

    private static AchievementSetProgress MapPlayerAchievementSet(
        JsonApiResource resource,
        Dictionary<(string Type, string Id), JsonApiResource> includedIndex)
    {
        var progress = new AchievementSetProgress
        {
            EarnedAchievements = resource.GetAttribute<int?>("achievementsUnlocked") ?? 0,
            EarnedPoints = resource.GetAttribute<int?>("points") ?? 0
        };

        // setContext is an array of {achievementSetId, gameId, type} pairs — the first carries the
        // type discriminator (core/bonus/specialty/exclusive) for this player-achievement-set record.
        long setIdFromContext = 0;
        if (resource.Attributes?["setContext"] is JArray setContextArray && setContextArray.Count > 0)
        {
            var first = setContextArray[0];
            setIdFromContext = first.Value<long?>("achievementSetId") ?? 0;
            var typeStr = first.Value<string?>("type");
            if (!string.IsNullOrEmpty(typeStr))
            {
                progress.SetType = AchievementSet.ParseSetType(typeStr);
            }
        }

        var setRelationship = resource.GetRelationship("achievementSet");
        var setRef = setRelationship?.GetSingleIdentifier();
        if (setRef != null && long.TryParse(setRef.Id, out var sid))
        {
            progress.SetId = sid;
        }
        else if (setIdFromContext > 0)
        {
            progress.SetId = setIdFromContext;
        }

        // Total achievement + point counts live on the included achievement-set resource.
        if (setRef != null && includedIndex.TryGetValue((setRef.Type, setRef.Id), out var setResource))
        {
            progress.TotalAchievements = setResource.GetAttribute<int?>("achievementsPublished") ?? 0;
            progress.TotalPoints = setResource.GetAttribute<int?>("pointsTotal") ?? 0;
            progress.SetName = setResource.GetAttribute<string>("title") ?? string.Empty;
        }

        if (string.IsNullOrEmpty(progress.SetName))
        {
            progress.SetName = progress.SetType == AchievementSetType.Core ? "Core" : progress.SetType.ToString();
        }

        return progress;
    }

    #endregion

    #region Private Mapping Methods

    private static UserGameProgress MapToUserGameProgress(
        JsonApiResource resource,
        Dictionary<(string Type, string Id), JsonApiResource> includedIndex,
        string userId)
    {
        var progress = new UserGameProgress
        {
            UserId = userId,
            GameId = long.TryParse(resource.GetAttribute<string>("gameId") ?? resource.Id, out var gid) ? gid : 0,
            EarnedPoints = resource.GetAttribute<int?>("pointsUnlocked") ??
                           resource.GetAttribute<int?>("scoreAchieved") ?? 0,
            TotalPoints = resource.GetAttribute<int?>("pointsTotal") ??
                          resource.GetAttribute<int?>("possibleScore") ?? 0,
            EarnedTruePoints = resource.GetAttribute<int?>("pointsWeightedUnlocked") ?? 0,
            TotalTruePoints = resource.GetAttribute<int?>("pointsWeightedTotal") ?? 0
        };

        // Only assign achievement counts when the API reports them; otherwise leave them
        // unset so UserGameProgress derives them from the mapped achievement list.
        var earnedCount = resource.GetAttribute<int?>("achievementsUnlocked") ??
                          resource.GetAttribute<int?>("numAwardedToUser");
        if (earnedCount.HasValue) progress.EarnedAchievements = earnedCount.Value;

        var totalCount = resource.GetAttribute<int?>("achievementsTotal") ??
                         resource.GetAttribute<int?>("numAchievements");
        if (totalCount.HasValue) progress.TotalAchievements = totalCount.Value;

        // Parse last played date
        var lastPlayedStr = resource.GetAttribute<string>("lastPlayedAt") ??
                            resource.GetAttribute<string>("lastPlayed");
        if (!string.IsNullOrEmpty(lastPlayedStr) &&
            DateTime.TryParse(lastPlayedStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var lastPlayed))
        {
            progress.LastPlayed = lastPlayed;
        }

        // Map game relationship
        var gameRelationship = resource.GetRelationship("game");
        if (gameRelationship != null)
        {
            var gameId = gameRelationship.GetSingleIdentifier();
            if (gameId != null && includedIndex.TryGetValue((gameId.Type, gameId.Id), out var gameResource))
            {
                progress.GameTitle = gameResource.GetAttribute<string>("title") ?? string.Empty;
                progress.GameId = long.TryParse(gameResource.Id, out var parsedGameId) ? parsedGameId : 0;

                // Map system from game
                var systemRelationship = gameResource.GetRelationship("system");
                if (systemRelationship != null)
                {
                    var systemId = systemRelationship.GetSingleIdentifier();
                    if (systemId != null && includedIndex.TryGetValue((systemId.Type, systemId.Id), out var systemResource))
                    {
                        progress.ConsoleName = systemResource.GetAttribute<string>("name") ?? string.Empty;
                    }
                }
            }
        }

        // Map achievement sets first (multi-set / subset support). Each set carries its own
        // achievements; we tag every achievement with its set membership so downstream code can
        // group by Core / Bonus / Specialty / Exclusive without re-querying.
        var mappedAchievementIds = new HashSet<int>();
        var setsRelationship = resource.GetRelationship("achievementSets");
        if (setsRelationship != null)
        {
            foreach (var setId in setsRelationship.GetIdentifierCollection())
            {
                if (!includedIndex.TryGetValue((setId.Type, setId.Id), out var setResource))
                    continue;

                var set = V2ResourceMapper.MapToAchievementSet(setResource, includedIndex);

                foreach (var achievement in set.Achievements)
                {
                    achievement.GameId = (int)progress.GameId;
                    achievement.GameTitle = progress.GameTitle;
                    achievement.SetId = set.Id;
                    achievement.SetType = set.SetType;
                    achievement.SetName = set.Name;
                    if (mappedAchievementIds.Add(achievement.Id))
                        progress.Achievements.Add(achievement);
                }

                progress.AchievementSets.Add(AchievementSetProgress.FromAchievementSet(set));
            }
        }

        // Fall back to the flat achievements relationship when the sets did not carry achievements
        // (single-set games, or a response shape that lists achievements at the game level).
        if (progress.Achievements.Count == 0)
        {
            var achievementsRelationship = resource.GetRelationship("achievements");
            if (achievementsRelationship != null)
            {
                foreach (var achId in achievementsRelationship.GetIdentifierCollection())
                {
                    if (!includedIndex.TryGetValue((achId.Type, achId.Id), out var achResource))
                        continue;

                    var achievement = V2ResourceMapper.MapToAchievement(achResource, includedIndex);
                    achievement.GameId = (int)progress.GameId;
                    achievement.GameTitle = progress.GameTitle;
                    if (mappedAchievementIds.Add(achievement.Id))
                        progress.Achievements.Add(achievement);
                }
            }
        }

        return progress;
    }

    private static RecentAchievement MapToRecentAchievement(
        JsonApiResource resource,
        Dictionary<(string Type, string Id), JsonApiResource> includedIndex)
    {
        var recent = new RecentAchievement();

        // A "player-achievements" resource carries the unlock timing and links to the achievement,
        // whose details (title/points/badge) live in the included "achievements" resource. For a plain
        // "achievements" resource (no relationship) we read the details directly off it.
        var achievementRelationship = resource.GetRelationship("achievement");
        JsonApiResource? detail = null;
        var achId = achievementRelationship?.GetSingleIdentifier();
        if (achId != null)
        {
            recent.AchievementId = int.TryParse(achId.Id, out var aid) ? aid : 0;
            includedIndex.TryGetValue((achId.Type, achId.Id), out detail);
        }
        if (recent.AchievementId == 0)
        {
            recent.AchievementId = int.TryParse(resource.Id, out var rid) ? rid : 0;
        }
        detail ??= resource;

        recent.Title = detail.GetAttribute<string>("title") ?? string.Empty;
        recent.Description = detail.GetAttribute<string>("description") ?? string.Empty;
        recent.Points = detail.GetAttribute<int?>("points") ?? 0;
        recent.TruePoints = detail.GetAttribute<int?>("pointsWeighted") ?? 0;
        recent.BadgeUrl = detail.GetAttribute<string>("badgeUrl") ?? string.Empty;

        // Unlock timing comes from the player-achievements attributes.
        var unlockedAtStr = resource.GetAttribute<string>("unlockedAt") ??
                            resource.GetAttribute<string>("earnedAt") ??
                            resource.GetAttribute<string>("dateEarned");
        if (!string.IsNullOrEmpty(unlockedAtStr) &&
            DateTime.TryParse(unlockedAtStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var unlockedAt))
        {
            recent.EarnedAt = unlockedAt;
        }
        recent.IsHardcore = !string.IsNullOrEmpty(resource.GetAttribute<string>("unlockedHardcoreAt"))
                            || (resource.GetAttribute<bool?>("hardcore") ?? false);

        // The game id/title may be on the included achievement's game relationship when present.
        var gameRelationship = detail.GetRelationship("game");
        var gameId = gameRelationship?.GetSingleIdentifier();
        if (gameId != null)
        {
            recent.GameId = long.TryParse(gameId.Id, out var gid) ? gid : 0;
            if (includedIndex.TryGetValue((gameId.Type, gameId.Id), out var gameResource))
            {
                recent.GameTitle = gameResource.GetAttribute<string>("title") ?? string.Empty;
            }
        }

        return recent;
    }

    private static RecentlyPlayedGame MapToRecentlyPlayedGame(
        JsonApiResource resource,
        Dictionary<(string Type, string Id), JsonApiResource> includedIndex)
    {
        var game = new RecentlyPlayedGame
        {
            // player-games exposes no unlock count, so these lookups always miss and this stays 0.
            // Kept (rather than dropped) so the shape matches the v1 mapper; see the property docs.
            EarnedAchievements = resource.GetAttribute<int?>("achievementsUnlocked") ??
                                  resource.GetAttribute<int?>("achievementsUnlockedHardcore") ?? 0,
            TotalAchievements = resource.GetAttribute<int?>("achievementsTotal") ?? 0
        };

        // The player-games resource id is a per-player record id, NOT the game id. The real game id and
        // its details come from the included "game" resource via the relationship.
        var gameRelationship = resource.GetRelationship("game");
        var gameRef = gameRelationship?.GetSingleIdentifier();
        JsonApiResource? gameResource = null;
        if (gameRef != null)
        {
            game.GameId = long.TryParse(gameRef.Id, out var gid) ? gid : 0;
            includedIndex.TryGetValue((gameRef.Type, gameRef.Id), out gameResource);
        }
        if (game.GameId == 0)
        {
            game.GameId = long.TryParse(resource.GetAttribute<string>("gameId") ?? string.Empty, out var gid2) ? gid2 : 0;
        }

        if (gameResource != null)
        {
            game.Title = gameResource.GetAttribute<string>("title") ?? string.Empty;
            game.BadgeUrl = gameResource.GetAttribute<string>("badgeUrl") ??
                            gameResource.GetAttribute<string>("imageIcon") ?? string.Empty;

            // The player-games attributes don't include achievementsTotal, so fall back to the
            // included games resource's achievementsPublished. Without this every recently-played
            // entry reports 0/0 and filtering-by-published becomes impossible.
            if (game.TotalAchievements == 0)
            {
                game.TotalAchievements = gameResource.GetAttribute<int?>("achievementsPublished") ?? 0;
            }

            var systemRelationship = gameResource.GetRelationship("system");
            var systemId = systemRelationship?.GetSingleIdentifier();
            if (systemId != null && includedIndex.TryGetValue((systemId.Type, systemId.Id), out var systemResource))
            {
                game.ConsoleName = systemResource.GetAttribute<string>("name") ?? string.Empty;
            }
        }

        // Parse last played date from the player-games attributes.
        var lastPlayedStr = resource.GetAttribute<string>("lastPlayedAt") ??
                            resource.GetAttribute<string>("lastPlayed");
        if (!string.IsNullOrEmpty(lastPlayedStr) &&
            DateTime.TryParse(lastPlayedStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var lastPlayed))
        {
            game.LastPlayed = lastPlayed;
        }

        return game;
    }

    #endregion

    public void Dispose()
    {
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }
}

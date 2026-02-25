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
            // V2 API endpoint: GET /users/{id}/games/{gameId}/progress
            var queryBuilder = V2QueryBuilder.Create()
                .Include("game")
                .Include("game.system");

            if (includeAchievementSets)
            {
                queryBuilder.Include("achievements");
                queryBuilder.Include("achievementSets");
            }

            var document = await _client.GetAsync(
                $"/users/{userId}/games/{gameId}/progress",
                queryBuilder,
                cancellationToken);

            if (document.HasErrors)
            {
                _logger.LogError("GET", $"/users/{userId}/games/{gameId}/progress", 
                    string.Join(", ", document.Errors?.Select(e => e.Detail) ?? Array.Empty<string>()));
                return null;
            }

            var resource = document.GetSingleResource();
            if (resource == null)
            {
                return null;
            }

            var includedIndex = document.GetIncludedIndex();
            return MapToUserGameProgress(resource, includedIndex, userId);
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
            // V2 API endpoint: GET /users/{id}/achievements?sort=-earnedAt&page[size]=N
            var queryBuilder = V2QueryBuilder.Create()
                .SortDescending("earnedAt")
                .PageSize(count)
                .Include("game");

            var document = await _client.GetRelationshipAsync(
                "users", userId, "achievements",
                queryBuilder,
                cancellationToken);

            if (document.HasErrors)
            {
                return new List<RecentAchievement>();
            }

            var includedIndex = document.GetIncludedIndex();
            return document.GetResourceCollection()
                .Where(r => r.Type == V2Constants.ResourceTypes.Achievements)
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
            // V2 API endpoint: GET /users/{id}/games?sort=-lastPlayedAt&page[size]=N
            var queryBuilder = V2QueryBuilder.Create()
                .SortDescending("lastPlayedAt")
                .PageSize(count)
                .Include("system");

            var document = await _client.GetRelationshipAsync(
                "users", userId, "games",
                queryBuilder,
                cancellationToken);

            if (document.HasErrors)
            {
                return new List<RecentlyPlayedGame>();
            }

            var includedIndex = document.GetIncludedIndex();
            return document.GetResourceCollection()
                .Where(r => r.Type == V2Constants.ResourceTypes.Games || r.Type == "user-game-progress")
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
            EarnedAchievements = resource.GetAttribute<int?>("achievementsUnlocked") ?? 
                                  resource.GetAttribute<int?>("numAwardedToUser") ?? 0,
            TotalAchievements = resource.GetAttribute<int?>("achievementsTotal") ?? 
                                 resource.GetAttribute<int?>("numAchievements") ?? 0,
            EarnedPoints = resource.GetAttribute<int?>("pointsUnlocked") ?? 
                           resource.GetAttribute<int?>("scoreAchieved") ?? 0,
            TotalPoints = resource.GetAttribute<int?>("pointsTotal") ?? 
                          resource.GetAttribute<int?>("possibleScore") ?? 0,
            EarnedTruePoints = resource.GetAttribute<int?>("pointsWeightedUnlocked") ?? 0,
            TotalTruePoints = resource.GetAttribute<int?>("pointsWeightedTotal") ?? 0
        };

        // Parse last played date
        var lastPlayedStr = resource.GetAttribute<string>("lastPlayedAt") ?? 
                            resource.GetAttribute<string>("lastPlayed");
        if (!string.IsNullOrEmpty(lastPlayedStr) && DateTime.TryParse(lastPlayedStr, out var lastPlayed))
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

        // Map achievements if included
        var achievementsRelationship = resource.GetRelationship("achievements");
        if (achievementsRelationship != null)
        {
            var achievementIds = achievementsRelationship.GetIdentifierCollection();
            foreach (var achId in achievementIds)
            {
                if (includedIndex.TryGetValue((achId.Type, achId.Id), out var achResource))
                {
                    var achievement = V2ResourceMapper.MapToAchievement(achResource, includedIndex);
                    achievement.GameId = (int)progress.GameId;
                    achievement.GameTitle = progress.GameTitle;
                    progress.Achievements.Add(achievement);
                }
            }
        }

        // Map achievement sets if included
        var setsRelationship = resource.GetRelationship("achievementSets");
        if (setsRelationship != null)
        {
            var setIds = setsRelationship.GetIdentifierCollection();
            foreach (var setId in setIds)
            {
                if (includedIndex.TryGetValue((setId.Type, setId.Id), out var setResource))
                {
                    var set = V2ResourceMapper.MapToAchievementSet(setResource, includedIndex);
                    progress.AchievementSets.Add(AchievementSetProgress.FromAchievementSet(set));
                }
            }
        }

        return progress;
    }

    private static RecentAchievement MapToRecentAchievement(
        JsonApiResource resource,
        Dictionary<(string Type, string Id), JsonApiResource> includedIndex)
    {
        var recentAchievement = new RecentAchievement
        {
            AchievementId = int.TryParse(resource.Id, out var id) ? id : 0,
            Title = resource.GetAttribute<string>("title") ?? string.Empty,
            Description = resource.GetAttribute<string>("description") ?? string.Empty,
            Points = resource.GetAttribute<int?>("points") ?? 0,
            TruePoints = resource.GetAttribute<int?>("pointsWeighted") ?? 0,
            BadgeUrl = resource.GetAttribute<string>("badgeUrl") ?? string.Empty,
            IsHardcore = resource.GetAttribute<bool?>("hardcore") ?? 
                         resource.GetAttribute<string>("mode")?.ToLowerInvariant() == "hardcore"
        };

        // Parse earned date
        var earnedAtStr = resource.GetAttribute<string>("earnedAt") ?? 
                          resource.GetAttribute<string>("dateEarned") ??
                          resource.GetAttribute<string>("unlockedAt");
        if (!string.IsNullOrEmpty(earnedAtStr) && DateTime.TryParse(earnedAtStr, out var earnedAt))
        {
            recentAchievement.EarnedAt = earnedAt;
        }

        // Map game relationship
        var gameRelationship = resource.GetRelationship("game");
        if (gameRelationship != null)
        {
            var gameId = gameRelationship.GetSingleIdentifier();
            if (gameId != null)
            {
                recentAchievement.GameId = long.TryParse(gameId.Id, out var gid) ? gid : 0;

                if (includedIndex.TryGetValue((gameId.Type, gameId.Id), out var gameResource))
                {
                    recentAchievement.GameTitle = gameResource.GetAttribute<string>("title") ?? string.Empty;
                }
            }
        }

        return recentAchievement;
    }

    private static RecentlyPlayedGame MapToRecentlyPlayedGame(
        JsonApiResource resource,
        Dictionary<(string Type, string Id), JsonApiResource> includedIndex)
    {
        var game = new RecentlyPlayedGame
        {
            GameId = long.TryParse(resource.GetAttribute<string>("gameId") ?? resource.Id, out var id) ? id : 0,
            Title = resource.GetAttribute<string>("title") ?? string.Empty,
            BadgeUrl = resource.GetAttribute<string>("badgeUrl") ?? 
                       resource.GetAttribute<string>("imageIcon") ?? string.Empty,
            EarnedAchievements = resource.GetAttribute<int?>("achievementsUnlocked") ?? 
                                  resource.GetAttribute<int?>("numAwardedToUser") ?? 0,
            TotalAchievements = resource.GetAttribute<int?>("achievementsTotal") ?? 
                                 resource.GetAttribute<int?>("numAchievements") ?? 0
        };

        // Parse last played date
        var lastPlayedStr = resource.GetAttribute<string>("lastPlayedAt") ?? 
                            resource.GetAttribute<string>("lastPlayed");
        if (!string.IsNullOrEmpty(lastPlayedStr) && DateTime.TryParse(lastPlayedStr, out var lastPlayed))
        {
            game.LastPlayed = lastPlayed;
        }

        // Map system relationship
        var systemRelationship = resource.GetRelationship("system");
        if (systemRelationship != null)
        {
            var systemId = systemRelationship.GetSingleIdentifier();
            if (systemId != null && includedIndex.TryGetValue((systemId.Type, systemId.Id), out var systemResource))
            {
                game.ConsoleName = systemResource.GetAttribute<string>("name") ?? string.Empty;
            }
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

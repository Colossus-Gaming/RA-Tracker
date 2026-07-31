using RATracker.Models;
using RATracker.WPF.Http.V2;
using RATracker.WPF.Http.V2.JsonApi;

namespace RATracker.WPF.Http.V2.Mappers;

/// <summary>
/// Maps JSON:API resources to domain models.
/// </summary>
public static class V2ResourceMapper
{
    #region System Mapping

    /// <summary>
    /// Maps a JSON:API system resource to a SystemInfo model.
    /// </summary>
    public static SystemInfo MapToSystemInfo(JsonApiResource resource)
    {
        return new SystemInfo
        {
            Id = long.TryParse(resource.Id, out var id) ? id : 0,
            Name = resource.GetAttribute<string>("name") ?? string.Empty,
            ShortName = resource.GetAttribute<string>("nameShort") ?? string.Empty,
            IconUrl = resource.GetAttribute<string>("iconUrl") ?? string.Empty,
            IsActive = resource.GetAttribute<bool?>("active") ?? true,
            GameCount = resource.GetAttribute<int?>("gamesPublished") ?? 0,
            AchievementCount = resource.GetAttribute<int?>("achievementsPublished") ?? 0
        };
    }

    /// <summary>
    /// Maps a collection of JSON:API system resources to SystemInfo models.
    /// </summary>
    public static List<SystemInfo> MapToSystemInfoList(JsonApiDocument document)
    {
        return document.GetResourceCollection()
            .Where(r => r.Type == "systems")
            .Select(MapToSystemInfo)
            .ToList();
    }

    #endregion

    #region Game Mapping

    /// <summary>
    /// Maps a JSON:API game resource to a GameInfo model.
    /// </summary>
    public static GameInfo MapToGameInfo(JsonApiResource resource, Dictionary<(string Type, string Id), JsonApiResource>? includedIndex = null)
    {
        // v2 game attributes use `*Url` suffixes for media (badgeUrl, imageBoxArtUrl, ...).
        // releasedAt arrives as full ISO ("1999-09-29T00:00:00.000000Z"); the UI shows just the year,
        // so we strip the time part. publisher/developer/genre are not on the v2 games resource —
        // they'd have to come from v1 GetGame.php if needed.
        var releasedRaw = resource.GetAttribute<string>("releasedAt") ?? string.Empty;
        var released = releasedRaw.Length >= 4 && DateTime.TryParse(releasedRaw, out var rd)
            ? rd.Year.ToString()
            : releasedRaw;

        var gameInfo = new GameInfo
        {
            Id = long.TryParse(resource.Id, out var id) ? id : 0,
            Title = resource.GetAttribute<string>("title") ?? string.Empty,
            BadgeUri = resource.GetAttribute<string>("badgeUrl") ?? string.Empty,
            ImageTitle = resource.GetAttribute<string>("imageTitleUrl") ?? resource.GetAttribute<string>("imageTitle") ?? string.Empty,
            ImageIngame = resource.GetAttribute<string>("imageIngameUrl") ?? resource.GetAttribute<string>("imageIngame") ?? string.Empty,
            ImageBoxArt = resource.GetAttribute<string>("imageBoxArtUrl") ?? resource.GetAttribute<string>("imageBoxArt") ?? string.Empty,
            Publisher = resource.GetAttribute<string>("publisher") ?? string.Empty,
            Developer = resource.GetAttribute<string>("developer") ?? string.Empty,
            Genre = resource.GetAttribute<string>("genre") ?? string.Empty,
            Released = released
        };

        // Map system relationship if included
        var systemRelationship = resource.GetRelationship("system");
        if (systemRelationship != null && includedIndex != null)
        {
            var systemId = systemRelationship.GetSingleIdentifier();
            if (systemId != null && includedIndex.TryGetValue((systemId.Type, systemId.Id), out var systemResource))
            {
                gameInfo.ConsoleId = long.TryParse(systemResource.Id, out var consoleId) ? consoleId : 0;
                gameInfo.ConsoleName = systemResource.GetAttribute<string>("name") ?? string.Empty;
            }
        }

        // Map achievements if present in attributes (for backward compatibility)
        // Note: In V2, achievements are typically fetched separately or via achievement sets
        var achievementsData = resource.Attributes?["achievements"];
        if (achievementsData != null)
        {
            try
            {
                var achievements = achievementsData.ToObject<List<Achievement>>();
                if (achievements != null)
                {
                    gameInfo.Achievements = achievements;
                }
            }
            catch
            {
                // Ignore mapping errors for achievements
            }
        }

        return gameInfo;
    }

    /// <summary>
    /// Maps a collection of JSON:API game resources to GameInfo models.
    /// </summary>
    public static List<GameInfo> MapToGameInfoList(JsonApiDocument document)
    {
        var includedIndex = document.GetIncludedIndex();
        return document.GetResourceCollection()
            .Where(r => r.Type == "games")
            .Select(r => MapToGameInfo(r, includedIndex))
            .ToList();
    }

    #endregion

    #region User Mapping

    /// <summary>
    /// Maps a JSON:API user resource to a UserSummary model.
    /// </summary>
    public static UserSummary MapToUserSummary(JsonApiResource resource)
    {
        // v2 splits score into softcore "points" and "pointsHardcore"; the headline RA score is hardcore.
        // The user resource carries no rank or last-game-id, so those stay 0 (current game comes from player-games).
        return new UserSummary
        {
            UserName = resource.GetAttribute<string>("displayName") ?? resource.GetAttribute<string>("username") ?? string.Empty,
            TotalPoints = resource.GetAttribute<int?>("pointsHardcore") ?? resource.GetAttribute<int?>("points") ?? 0,
            TotalTruePoints = resource.GetAttribute<int?>("pointsWeighted") ?? 0,
            Rank = resource.GetAttribute<int?>("playerRank") ?? resource.GetAttribute<int?>("rank") ?? 0,
            Motto = resource.GetAttribute<string>("motto") ?? string.Empty,
            UserPic = resource.GetAttribute<string>("avatarUrl") ?? string.Empty,
            LastGameID = resource.GetAttribute<int?>("lastGameId") ?? 0
        };
    }

    /// <summary>
    /// Maps a collection of JSON:API user resources to UserSummary models.
    /// </summary>
    public static List<UserSummary> MapToUserSummaryList(JsonApiDocument document)
    {
        return document.GetResourceCollection()
            .Where(r => r.Type == "users")
            .Select(MapToUserSummary)
            .ToList();
    }

    #endregion

    #region Achievement Mapping

    /// <summary>
    /// Maps a JSON:API achievement resource to an Achievement model.
    /// </summary>
    public static Achievement MapToAchievement(JsonApiResource resource, Dictionary<(string Type, string Id), JsonApiResource>? includedIndex = null)
    {
        var achievement = new Achievement
        {
            Id = int.TryParse(resource.Id, out var id) ? id : 0,
            Title = resource.GetAttribute<string>("title") ?? string.Empty,
            Description = resource.GetAttribute<string>("description") ?? string.Empty,
            Points = resource.GetAttribute<int?>("points") ?? 0,
            TrueRatio = resource.GetAttribute<int?>("pointsWeighted") ?? 0,
            BadgeUri = resource.GetAttribute<string>("badgeUrl") ?? string.Empty,
            DisplayOrder = resource.GetAttribute<int?>("orderColumn")
                           ?? resource.GetAttribute<int?>("displayOrder")
                           ?? 0
        };

        // Map game relationship if included
        var gameRelationship = resource.GetRelationship("game");
        if (gameRelationship != null)
        {
            var gameId = gameRelationship.GetSingleIdentifier();
            if (gameId != null)
            {
                achievement.GameId = int.TryParse(gameId.Id, out var gid) ? gid : 0;

                if (includedIndex != null && includedIndex.TryGetValue((gameId.Type, gameId.Id), out var gameResource))
                {
                    achievement.GameTitle = gameResource.GetAttribute<string>("title") ?? string.Empty;
                }
            }
        }

        // Map unlock date if present (would come from user progress data)
        var dateEarnedStr = resource.GetAttribute<string>("dateEarned") ?? resource.GetAttribute<string>("unlockedAt");
        if (!string.IsNullOrEmpty(dateEarnedStr) && DateTime.TryParse(dateEarnedStr, out var dateEarned))
        {
            achievement.DateEarned = dateEarned;
        }

        return achievement;
    }

    /// <summary>
    /// Maps a collection of JSON:API achievement resources to Achievement models.
    /// </summary>
    public static List<Achievement> MapToAchievementList(JsonApiDocument document)
    {
        var includedIndex = document.GetIncludedIndex();
        return document.GetResourceCollection()
            .Where(r => r.Type == "achievements")
            .Select(r => MapToAchievement(r, includedIndex))
            .ToList();
    }

    #endregion

    #region Achievement Set Mapping

    /// <summary>
    /// Maps a JSON:API achievement-set resource to an AchievementSet model.
    /// </summary>
    public static AchievementSet MapToAchievementSet(JsonApiResource resource, Dictionary<(string Type, string Id), JsonApiResource>? includedIndex = null)
    {
        var setId = long.TryParse(resource.Id, out var id) ? id : 0;
        
        var achievementSet = new AchievementSet
        {
            Id = setId,
            Name = resource.GetAttribute<string>("name") ?? resource.GetAttribute<string>("title") ?? $"Set {setId}",
            SetType = AchievementSet.ParseSetType(resource.GetAttribute<object>("type") ?? resource.GetAttribute<object>("setType"))
        };

        // Map game relationship
        var gameRelationship = resource.GetRelationship("game");
        if (gameRelationship != null)
        {
            var gameId = gameRelationship.GetSingleIdentifier();
            if (gameId != null && long.TryParse(gameId.Id, out var gid))
            {
                achievementSet.GameId = gid;
            }
        }

        // Map achievements if they are included in the relationship
        var achievementsRelationship = resource.GetRelationship("achievements");
        if (achievementsRelationship != null && includedIndex != null)
        {
            var achievementIds = achievementsRelationship.GetIdentifierCollection();
            foreach (var achievementId in achievementIds)
            {
                if (includedIndex.TryGetValue((achievementId.Type, achievementId.Id), out var achievementResource))
                {
                    var achievement = MapToAchievement(achievementResource, includedIndex);
                    achievementSet.Achievements.Add(achievement);
                }
            }
        }

        // Also check for achievements directly in attributes (some API responses might include them inline)
        var achievementsData = resource.Attributes?["achievements"];
        if (achievementsData != null && achievementSet.Achievements.Count == 0)
        {
            try
            {
                var achievements = achievementsData.ToObject<List<Achievement>>();
                if (achievements != null)
                {
                    achievementSet.Achievements = achievements;
                }
            }
            catch
            {
                // Ignore mapping errors
            }
        }

        return achievementSet;
    }

    /// <summary>
    /// Maps a collection of JSON:API achievement-set resources to AchievementSet models.
    /// </summary>
    public static List<AchievementSet> MapToAchievementSetList(JsonApiDocument document)
    {
        var includedIndex = document.GetIncludedIndex();
        return document.GetResourceCollection()
            .Where(r => r.Type == V2Constants.ResourceTypes.AchievementSets)
            .Select(r => MapToAchievementSet(r, includedIndex))
            .ToList();
    }

    /// <summary>
    /// Extracts achievement sets from included resources in a game response.
    /// </summary>
    public static List<AchievementSet> ExtractAchievementSetsFromIncluded(
        JsonApiResource gameResource,
        Dictionary<(string Type, string Id), JsonApiResource> includedIndex)
    {
        var achievementSets = new List<AchievementSet>();

        var setsRelationship = gameResource.GetRelationship(V2Constants.Relationships.AchievementSets);
        if (setsRelationship == null)
            return achievementSets;

        var setIdentifiers = setsRelationship.GetIdentifierCollection();
        foreach (var setId in setIdentifiers)
        {
            if (includedIndex.TryGetValue((setId.Type, setId.Id), out var setResource))
            {
                var achievementSet = MapToAchievementSet(setResource, includedIndex);
                achievementSets.Add(achievementSet);
            }
        }

        // Sort sets: core first, then bonus, then others by ID
        achievementSets = achievementSets
            .OrderBy(s => s.SetType)
            .ThenBy(s => s.Id)
            .ToList();

        return achievementSets;
    }

    #endregion

    #region Hub Mapping

    /// <summary>
    /// Maps a JSON:API hub resource to a HubInfo model.
    /// </summary>
    public static HubInfo MapToHubInfo(JsonApiResource resource, Dictionary<(string Type, string Id), JsonApiResource>? includedIndex = null)
    {
        var hub = new HubInfo
        {
            Id = long.TryParse(resource.Id, out var id) ? id : 0,
            Name = resource.GetAttribute<string>("name") ?? resource.GetAttribute<string>("title") ?? string.Empty,
            Description = resource.GetAttribute<string>("description") ?? string.Empty,
            BadgeUrl = resource.GetAttribute<string>("badgeUrl") ?? resource.GetAttribute<string>("iconUrl") ?? string.Empty,
            GameCount = resource.GetAttribute<int?>("gamesCount") ?? resource.GetAttribute<int?>("gameCount") ?? 0,
            AchievementCount = resource.GetAttribute<int?>("achievementsCount") ?? resource.GetAttribute<int?>("achievementCount") ?? 0,
            PointsTotal = resource.GetAttribute<int?>("pointsTotal") ?? 0
        };

        // Map parent hub relationship if present
        var parentRelationship = resource.GetRelationship("parent");
        if (parentRelationship != null)
        {
            var parentId = parentRelationship.GetSingleIdentifier();
            if (parentId != null && long.TryParse(parentId.Id, out var pid))
            {
                hub.ParentHubId = pid;
            }
        }

        // Map linked hubs if included
        var linksRelationship = resource.GetRelationship(V2Constants.Relationships.Links);
        if (linksRelationship != null && includedIndex != null)
        {
            var linkIds = linksRelationship.GetIdentifierCollection();
            foreach (var linkId in linkIds)
            {
                if (includedIndex.TryGetValue((linkId.Type, linkId.Id), out var linkedResource))
                {
                    hub.Links.Add(new HubLink
                    {
                        LinkedHubId = long.TryParse(linkedResource.Id, out var linkedId) ? linkedId : 0,
                        LinkedHubName = linkedResource.GetAttribute<string>("name") ?? string.Empty,
                        LinkType = linkedResource.GetAttribute<string>("linkType") ?? "related"
                    });
                }
            }
        }

        // Map games if included
        var gamesRelationship = resource.GetRelationship(V2Constants.Relationships.Games);
        if (gamesRelationship != null && includedIndex != null)
        {
            var gameIds = gamesRelationship.GetIdentifierCollection();
            foreach (var gameId in gameIds)
            {
                if (includedIndex.TryGetValue((gameId.Type, gameId.Id), out var gameResource))
                {
                    hub.Games.Add(MapToGameInfo(gameResource, includedIndex));
                }
            }
        }

        return hub;
    }

    /// <summary>
    /// Maps a collection of JSON:API hub resources to HubInfo models.
    /// </summary>
    public static List<HubInfo> MapToHubInfoList(JsonApiDocument document)
    {
        var includedIndex = document.GetIncludedIndex();
        return document.GetResourceCollection()
            .Where(r => r.Type == V2Constants.ResourceTypes.Hubs)
            .Select(r => MapToHubInfo(r, includedIndex))
            .ToList();
    }

    #endregion

    #region Legacy Support

    /// <summary>
    /// Maps a JSON:API system resource to console information (legacy tuple format).
    /// </summary>
    [Obsolete("Use MapToSystemInfo instead")]
    public static (long Id, string Name) MapToSystem(JsonApiResource resource)
    {
        var id = long.TryParse(resource.Id, out var systemId) ? systemId : 0;
        var name = resource.GetAttribute<string>("name") ?? string.Empty;
        return (id, name);
    }

    /// <summary>
    /// Maps a collection of JSON:API system resources to (id, name) tuples (legacy format).
    /// </summary>
    [Obsolete("Use MapToSystemInfoList instead")]
    public static List<(long Id, string Name)> MapToSystemList(JsonApiDocument document)
    {
        return document.GetResourceCollection()
            .Where(r => r.Type == "systems")
            .Select(MapToSystem)
            .ToList();
    }

    #endregion
}

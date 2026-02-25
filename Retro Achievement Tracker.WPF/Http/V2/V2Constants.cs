namespace RATracker.WPF.Http.V2;

/// <summary>
/// Constants for the V2 API.
/// </summary>
public static class V2Constants
{
    /// <summary>
    /// The base URL for the RetroAchievements API.
    /// </summary>
    public const string BaseUrl = "https://retroachievements.org";

    /// <summary>
    /// The base path for the V2 API.
    /// </summary>
    public const string ApiPath = "/api/v2";

    /// <summary>
    /// The JSON:API media type.
    /// </summary>
    public const string JsonApiMediaType = "application/vnd.api+json";

    /// <summary>
    /// Resource type constants.
    /// </summary>
    public static class ResourceTypes
    {
        public const string Systems = "systems";
        public const string Games = "games";
        public const string Users = "users";
        public const string Achievements = "achievements";
        public const string AchievementSets = "achievement-sets";
        public const string Leaderboards = "leaderboards";
        public const string Hubs = "hubs";
    }

    /// <summary>
    /// Common relationship names.
    /// </summary>
    public static class Relationships
    {
        public const string System = "system";
        public const string Game = "game";
        public const string Games = "games";
        public const string User = "user";
        public const string AchievementSets = "achievementSets";
        public const string Achievements = "achievements";
        public const string Entries = "entries";
        public const string Links = "links";
    }

    /// <summary>
    /// Common filter field names.
    /// </summary>
    public static class Filters
    {
        public const string SystemId = "systemId";
        public const string Active = "active";
        public const string Role = "role";
        public const string Title = "title";
        public const string ParentId = "parentId";
        public const string User = "user";
    }

    /// <summary>
    /// Common sort field names.
    /// </summary>
    public static class SortFields
    {
        public const string Name = "name";
        public const string Title = "title";
        public const string PlayersTotal = "playersTotal";
        public const string PointsTotal = "pointsTotal";
        public const string PointsWeighted = "pointsWeighted";
        public const string EarnedAt = "earnedAt";
        public const string LastPlayedAt = "lastPlayedAt";
    }

    /// <summary>
    /// Progress-related attribute names.
    /// </summary>
    public static class ProgressAttributes
    {
        public const string AchievementsUnlocked = "achievementsUnlocked";
        public const string AchievementsTotal = "achievementsTotal";
        public const string PointsUnlocked = "pointsUnlocked";
        public const string PointsWeightedUnlocked = "pointsWeightedUnlocked";
        public const string LastPlayedAt = "lastPlayedAt";
        public const string EarnedAt = "earnedAt";
        public const string Hardcore = "hardcore";
    }

    /// <summary>
    /// Default pagination values.
    /// </summary>
    public static class Pagination
    {
        public const int DefaultPageSize = 50;
        public const int MaxPageSize = 100;
    }
}

namespace RATracker.Models;

/// <summary>
/// Represents a hub/collection from RetroAchievements V2 API.
/// Hubs are curated collections of games organized by theme, series, or other criteria.
/// </summary>
public class HubInfo
{
    /// <summary>
    /// The unique identifier for this hub.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The display name of the hub.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A description of the hub's contents or theme.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The URL to the hub's badge/icon image.
    /// </summary>
    public string BadgeUrl { get; set; } = string.Empty;

    /// <summary>
    /// The total number of games in this hub.
    /// </summary>
    public int GameCount { get; set; }

    /// <summary>
    /// The total number of achievements across all games in this hub.
    /// </summary>
    public int AchievementCount { get; set; }

    /// <summary>
    /// The total points available across all games in this hub.
    /// </summary>
    public int PointsTotal { get; set; }

    /// <summary>
    /// The parent hub ID if this is a sub-hub.
    /// </summary>
    public long? ParentHubId { get; set; }

    /// <summary>
    /// The games contained in this hub.
    /// Populated when explicitly requested with includes.
    /// </summary>
    public List<GameInfo> Games { get; set; } = new();

    /// <summary>
    /// Linked/related hubs.
    /// </summary>
    public List<HubLink> Links { get; set; } = new();
}

/// <summary>
/// Represents a link between hubs.
/// </summary>
public class HubLink
{
    /// <summary>
    /// The linked hub's ID.
    /// </summary>
    public long LinkedHubId { get; set; }

    /// <summary>
    /// The linked hub's name.
    /// </summary>
    public string LinkedHubName { get; set; } = string.Empty;

    /// <summary>
    /// The type of link relationship.
    /// </summary>
    public string LinkType { get; set; } = string.Empty;
}

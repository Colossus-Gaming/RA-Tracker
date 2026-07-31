namespace RATracker.Models;

/// <summary>
/// Represents a gaming system/console from RetroAchievements.
/// </summary>
public class SystemInfo
{
    /// <summary>
    /// The unique identifier of the system.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The display name of the system (e.g., "Super Nintendo Entertainment System").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The short name of the system (e.g., "SNES").
    /// </summary>
    public string ShortName { get; set; } = string.Empty;

    /// <summary>
    /// The URL to the system's icon.
    /// </summary>
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the system is currently active on RetroAchievements.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// The total number of games with achievements on this system.
    /// </summary>
    public int GameCount { get; set; }

    /// <summary>
    /// The total number of achievements across all games on this system.
    /// </summary>
    public int AchievementCount { get; set; }

    public override string ToString() => Name;
}

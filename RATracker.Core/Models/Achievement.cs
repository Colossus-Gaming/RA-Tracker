namespace RATracker.Models;

/// <summary>
/// Represents a RetroAchievements achievement.
/// </summary>
public class Achievement : IEquatable<Achievement>, IComparable<Achievement>, ICloneable
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public int TrueRatio { get; set; }
    public string BadgeUri { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime? DateEarned { get; set; }

    /// <summary>
    /// The type of achievement set this achievement belongs to.
    /// Defaults to Core for backward compatibility.
    /// </summary>
    public AchievementSetType SetType { get; set; } = AchievementSetType.Core;

    /// <summary>
    /// The ID of the achievement set this achievement belongs to.
    /// </summary>
    public long? SetId { get; set; }

    /// <summary>
    /// The display name of the achievement set (e.g., "Bonus", "Specialty").
    /// </summary>
    public string? SetName { get; set; }

    /// <summary>
    /// Whether this achievement is from the core/base achievement set.
    /// </summary>
    public bool IsCore => SetType == AchievementSetType.Core;

    /// <summary>
    /// Whether this achievement is from a non-core set (bonus, specialty, exclusive).
    /// </summary>
    public bool IsSubSet => SetType != AchievementSetType.Core;

    public int CompareTo(Achievement? other)
    {
        if (other == null) return 1;

        if (other.DateEarned.HasValue)
        {
            if (DateEarned.HasValue)
            {
                if (DateEarned.Value.Equals(other.DateEarned.Value))
                {
                    if (DisplayOrder.Equals(other.DisplayOrder))
                    {
                        return Id.CompareTo(other.Id);
                    }
                    return DisplayOrder.CompareTo(other.DisplayOrder);
                }
                return DateEarned.Value.CompareTo(other.DateEarned.Value);
            }
            return -1;
        }
        else if (DateEarned.HasValue)
        {
            return 1;
        }

        if (DisplayOrder.Equals(other.DisplayOrder))
        {
            return Id.CompareTo(other.Id);
        }
        return other.DisplayOrder.CompareTo(DisplayOrder);
    }

    public bool Equals(Achievement? other)
    {
        return other != null && Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Achievement);

    public override int GetHashCode() => Id.GetHashCode();

    public object Clone()
    {
        return MemberwiseClone();
    }
}

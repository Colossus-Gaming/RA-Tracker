namespace RATracker.Models;

/// <summary>
/// Represents a RetroAchievements user summary.
/// </summary>
public class UserSummary : IEquatable<UserSummary>, ICloneable
{
    public string UserName { get; set; } = string.Empty;
    public int LastGameID { get; set; }
    public int TotalPoints { get; set; }
    public int TotalTruePoints { get; set; }
    public int Rank { get; set; }
    public string Motto { get; set; } = string.Empty;
    public string UserPic { get; set; } = string.Empty;
    public List<Achievement> Achievements { get; set; } = new();

    public string RetroRatio
    {
        get
        {
            if (TotalPoints == 0) return "0.00";
            return ((float)TotalTruePoints / TotalPoints).ToString("0.00");
        }
    }

    public bool Equals(UserSummary? other)
    {
        return other != null
            && LastGameID == other.LastGameID
            && TotalPoints == other.TotalPoints
            && TotalTruePoints == other.TotalTruePoints
            && Rank == other.Rank;
    }

    public override bool Equals(object? obj) => Equals(obj as UserSummary);

    public override int GetHashCode() => HashCode.Combine(LastGameID, TotalPoints, TotalTruePoints, Rank);

    public object Clone()
    {
        return MemberwiseClone();
    }
}

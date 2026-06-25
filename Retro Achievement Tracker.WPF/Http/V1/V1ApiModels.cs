using System.Globalization;
using Newtonsoft.Json;
using RATracker.Models;

namespace RATracker.WPF.Http.V1;

// Data-transfer objects matching the actual field names returned by the public v1 Web API.
// The v1 JSON uses names like "GameID"/"User"/"BadgeName" and returns a game's achievements as a
// dictionary keyed by achievement id — neither of which maps cleanly onto the domain models, so we
// deserialize into these DTOs and map explicitly via V1Mapper.

internal sealed class V1UserSummaryDto
{
    [JsonProperty("User")] public string? User { get; set; }
    [JsonProperty("ULID")] public string? Ulid { get; set; }
    [JsonProperty("LastGameID")] public int LastGameID { get; set; }
    [JsonProperty("TotalPoints")] public int TotalPoints { get; set; }
    [JsonProperty("TotalTruePoints")] public int TotalTruePoints { get; set; }
    [JsonProperty("Rank")] public int Rank { get; set; }
    [JsonProperty("Motto")] public string? Motto { get; set; }
    [JsonProperty("UserPic")] public string? UserPic { get; set; }
}

internal sealed class V1RecentlyPlayedDto
{
    [JsonProperty("GameID")] public long GameID { get; set; }
    [JsonProperty("ConsoleID")] public long ConsoleID { get; set; }
    [JsonProperty("ConsoleName")] public string? ConsoleName { get; set; }
    [JsonProperty("Title")] public string? Title { get; set; }
    [JsonProperty("ImageIcon")] public string? ImageIcon { get; set; }
    [JsonProperty("ImageBoxArt")] public string? ImageBoxArt { get; set; }
    [JsonProperty("LastPlayed")] public string? LastPlayed { get; set; }
    [JsonProperty("NumPossibleAchievements")] public int NumPossibleAchievements { get; set; }
    [JsonProperty("NumAchieved")] public int NumAchieved { get; set; }
    [JsonProperty("ScoreAchieved")] public int ScoreAchieved { get; set; }
    [JsonProperty("PossibleScore")] public int PossibleScore { get; set; }
}

internal sealed class V1AchievementDto
{
    [JsonProperty("ID")] public int Id { get; set; }
    [JsonProperty("Title")] public string? Title { get; set; }
    [JsonProperty("Description")] public string? Description { get; set; }
    [JsonProperty("Points")] public int Points { get; set; }
    [JsonProperty("TrueRatio")] public int TrueRatio { get; set; }
    [JsonProperty("BadgeName")] public string? BadgeName { get; set; }
    [JsonProperty("DisplayOrder")] public int DisplayOrder { get; set; }
    [JsonProperty("DateEarned")] public string? DateEarned { get; set; }
    [JsonProperty("DateEarnedHardcore")] public string? DateEarnedHardcore { get; set; }
}

internal sealed class V1GameProgressDto
{
    [JsonProperty("ID")] public long Id { get; set; }
    [JsonProperty("Title")] public string? Title { get; set; }
    [JsonProperty("ConsoleID")] public long ConsoleID { get; set; }
    [JsonProperty("ConsoleName")] public string? ConsoleName { get; set; }
    [JsonProperty("ImageIcon")] public string? ImageIcon { get; set; }
    [JsonProperty("ImageTitle")] public string? ImageTitle { get; set; }
    [JsonProperty("ImageIngame")] public string? ImageIngame { get; set; }
    [JsonProperty("ImageBoxArt")] public string? ImageBoxArt { get; set; }
    [JsonProperty("Publisher")] public string? Publisher { get; set; }
    [JsonProperty("Developer")] public string? Developer { get; set; }
    [JsonProperty("Genre")] public string? Genre { get; set; }
    [JsonProperty("Released")] public string? Released { get; set; }
    [JsonProperty("NumAchievements")] public int NumAchievements { get; set; }
    [JsonProperty("NumAwardedToUser")] public int NumAwardedToUser { get; set; }
    [JsonProperty("NumAwardedToUserHardcore")] public int NumAwardedToUserHardcore { get; set; }
    [JsonProperty("Achievements")] public Dictionary<string, V1AchievementDto>? Achievements { get; set; }
}

internal sealed class V1RecentAchievementDto
{
    [JsonProperty("ID")] public int Id { get; set; }
    [JsonProperty("AchievementID")] public int AchievementId { get; set; }
    [JsonProperty("GameID")] public long GameID { get; set; }
    [JsonProperty("GameTitle")] public string? GameTitle { get; set; }
    [JsonProperty("Title")] public string? Title { get; set; }
    [JsonProperty("Description")] public string? Description { get; set; }
    [JsonProperty("Points")] public int Points { get; set; }
    [JsonProperty("TrueRatio")] public int TrueRatio { get; set; }
    [JsonProperty("BadgeName")] public string? BadgeName { get; set; }
    [JsonProperty("Date")] public string? Date { get; set; }
    [JsonProperty("HardcoreMode")] public int HardcoreMode { get; set; }

    /// <summary>The recent-achievements endpoint uses "AchievementID"; some payloads use "ID".</summary>
    public int EffectiveId => AchievementId != 0 ? AchievementId : Id;
}

/// <summary>
/// Maps v1 Web API DTOs to the app's domain models, constructing absolute media URLs and parsing dates.
/// </summary>
internal static class V1Mapper
{
    private const string MediaBase = "https://media.retroachievements.org";

    public static string BadgeUrl(string? badgeName) =>
        string.IsNullOrWhiteSpace(badgeName) ? string.Empty : $"{MediaBase}/Badge/{badgeName}.png";

    public static string MediaUrl(string? path) =>
        string.IsNullOrWhiteSpace(path) ? string.Empty : $"{MediaBase}{path}";

    public static DateTime? ParseDate(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    public static UserSummary MapUserSummary(V1UserSummaryDto dto) => new()
    {
        UserName = dto.User ?? string.Empty,
        LastGameID = dto.LastGameID,
        TotalPoints = dto.TotalPoints,
        TotalTruePoints = dto.TotalTruePoints,
        Rank = dto.Rank,
        Motto = dto.Motto ?? string.Empty,
        UserPic = MediaUrl(dto.UserPic)
    };

    public static RecentlyPlayedGame MapRecentlyPlayed(V1RecentlyPlayedDto dto) => new()
    {
        GameId = dto.GameID,
        Title = dto.Title ?? string.Empty,
        ConsoleName = dto.ConsoleName ?? string.Empty,
        BadgeUrl = MediaUrl(dto.ImageIcon),
        LastPlayed = ParseDate(dto.LastPlayed),
        TotalAchievements = dto.NumPossibleAchievements,
        EarnedAchievements = dto.NumAchieved
    };

    public static Achievement MapAchievement(V1AchievementDto dto) => new()
    {
        Id = dto.Id,
        Title = dto.Title ?? string.Empty,
        Description = dto.Description ?? string.Empty,
        Points = dto.Points,
        TrueRatio = dto.TrueRatio,
        BadgeUri = BadgeUrl(dto.BadgeName),
        DisplayOrder = dto.DisplayOrder,
        DateEarned = ParseDate(dto.DateEarnedHardcore) ?? ParseDate(dto.DateEarned)
    };

    public static RecentAchievement MapRecentAchievement(V1RecentAchievementDto dto) => new()
    {
        AchievementId = dto.EffectiveId,
        Title = dto.Title ?? string.Empty,
        Description = dto.Description ?? string.Empty,
        Points = dto.Points,
        TruePoints = dto.TrueRatio,
        BadgeUrl = BadgeUrl(dto.BadgeName),
        GameId = dto.GameID,
        GameTitle = dto.GameTitle ?? string.Empty,
        EarnedAt = ParseDate(dto.Date),
        IsHardcore = dto.HardcoreMode == 1
    };

    public static GameInfo MapGameProgress(V1GameProgressDto dto)
    {
        var game = new GameInfo
        {
            Id = dto.Id,
            Title = dto.Title ?? string.Empty,
            ConsoleId = dto.ConsoleID,
            BadgeUri = MediaUrl(dto.ImageIcon),
            ImageTitle = MediaUrl(dto.ImageTitle),
            ImageIngame = MediaUrl(dto.ImageIngame),
            ImageBoxArt = MediaUrl(dto.ImageBoxArt),
            Publisher = dto.Publisher ?? string.Empty,
            Developer = dto.Developer ?? string.Empty,
            Genre = dto.Genre ?? string.Empty,
            Released = dto.Released ?? string.Empty
        };

        // The v1 payload carries a console name directly; prefer it over the id-based lookup.
        if (!string.IsNullOrWhiteSpace(dto.ConsoleName))
            game.ConsoleName = dto.ConsoleName;

        if (dto.Achievements != null)
        {
            game.Achievements = dto.Achievements.Values
                .Select(MapAchievement)
                .OrderBy(a => a.DisplayOrder)
                .ToList();
        }

        return game;
    }
}
